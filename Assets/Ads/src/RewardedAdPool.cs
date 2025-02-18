#nullable enable

using BubbleShooter.Core;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BubbleShooter.Ads {
    public sealed class RewardedAdPool {
        private readonly string adUnitId;
        public TimeSpan refreshRate;
        public TimeSpan retryRate;

        private readonly List<RefreshedAd> ads = new();
        private CancellationTokenSource? loadCancelSource;
        private readonly object adsLock = new();

        public ILogger? logger;
        private const string logTag = nameof(RewardedAdPool);

        public RewardedAdPool(
            string adUnitId,
            int preloadCount = 1,
            TimeSpan? refreshRate = null,
            TimeSpan? retryRate = null,
            ILogger? logger = null
        ) {
            this.adUnitId = adUnitId;
            this.refreshRate = refreshRate ?? Defaults.refreshRate;
            this.retryRate = retryRate ?? Defaults.retryRate;
            this.logger = logger;

            if (preloadCount > 0) {
                this.PreloadAds(preloadCount).FireAndForget();
            }
        }

        public bool TryGetAd([NotNullWhen(true)] out RewardedAd? ad) {
            this.PreloadAds(1).FireAndForget();

            lock (this.adsLock) {
                int index = this.ads.FindIndex(refreshedAd => refreshedAd.ad.CanShowAd());

                if (index < 0) {
                    ad = null;

                    return false;
                }

                RefreshedAd refreshedAd = this.ads[index];
                this.ads.RemoveAt(index);
                refreshedAd.StopRefreshing();
                ad = refreshedAd.ad;
            }

            return true;
        }

        public async Task PreloadAds(int count) {
            if (count <= 0) {
                return;
            }

            this.loadCancelSource ??= new CancellationTokenSource();
            IEnumerable<RewardedAd> ads;

            try {
                if (count == 1) {
                    ads = Enumerable.Repeat(await this.LoadAd(this.loadCancelSource.Token), 1);
                } else {
                    ads = await Task.WhenAll(
                        Enumerable.Range(0, count)
                        .Select(_ => this.LoadAd(this.loadCancelSource.Token))
                    );
                }
            } catch (TaskCanceledException) {
                return;
            }

            lock (this.adsLock) {
                this.ads.AddRange(ads.Select(ad => new RefreshedAd(ad, this)));
            }
        }

        public void Clear() {
            this.loadCancelSource?.Cancel();
            this.loadCancelSource = null;

            lock (this.adsLock) {
                foreach (RefreshedAd ad in this.ads) {
                    ad.Destroy();
                }

                this.ads.Clear();
            }
        }

        private async Task<RewardedAd> LoadAd(CancellationToken cancel) {
            RewardedAd? ad = await Load(this);

            cancel.ThrowIfCancellationRequested();

            while (ad == null) {
                await Task.Delay(this.retryRate, cancel);

                ad = await Load(this);

                cancel.ThrowIfCancellationRequested();
            }

            return ad;

            static Task<RewardedAd?> Load(RewardedAdPool pool) {
                TaskCompletionSource<RewardedAd?> adSource = new();

                RewardedAd.Load(pool.adUnitId, new AdRequest(), (ad, error) => {
                    if (error != null) {
                        pool.logger?.LogError(logTag, $"Failed to load ad: {error}");

                        adSource.SetResult(null);

                        return;
                    }

                    adSource.SetResult(ad);
                });

                return adSource.Task;
            }
        }

        public static class Defaults {
            public static readonly TimeSpan refreshRate = TimeSpan.FromMinutes(50);
            public static readonly TimeSpan retryRate = TimeSpan.FromSeconds(10);
        }

        private struct RefreshedAd {
            public RewardedAd ad;
            private readonly RewardedAdPool pool;
            private readonly CancellationTokenSource refreshCancelSource;

            public RefreshedAd(RewardedAd ad, RewardedAdPool pool) {
                this.ad = ad;
                this.pool = pool;
                this.refreshCancelSource = new CancellationTokenSource();

                this.RefreshLoop(this.refreshCancelSource.Token);
            }

            public readonly void Destroy() {
                this.StopRefreshing();
                this.ad.Destroy();
            }

            public readonly void StopRefreshing() {
                this.refreshCancelSource.Cancel();
            }

            private async void RefreshLoop(CancellationToken cancel) {
                while (true) {
                    RewardedAd newAd;

                    try {
                        await Task.Delay(this.pool.refreshRate, cancel);

                        newAd = await this.pool.LoadAd(cancel);
                    } catch (TaskCanceledException) {
                        return;
                    }

                    lock (this.pool.adsLock) {
                        this.ad.Destroy();
                        this.ad = newAd;
                    }
                }
            }
        }
    }
}
