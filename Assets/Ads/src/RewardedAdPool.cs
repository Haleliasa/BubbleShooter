#nullable enable

using BubbleShooter.Core;
using GoogleMobileAds.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BubbleShooter.Ads {
    [CreateAssetMenu(
        fileName = nameof(RewardedAdPool),
        menuName = AdsMenuNames.ads + nameof(RewardedAdPool)
    )]
    public class RewardedAdPool : ScriptableObject {
        [SerializeField]
        private string adUnitId = "";

        [Tooltip("min")]
        [Min(1)]
        [SerializeField]
        private int refreshRate = 50;

        [Tooltip("sec")]
        [Min(1)]
        [SerializeField]
        private int retryRate = 10;

        private readonly ConcurrentQueue<RefreshedAd> ads = new();
        private CancellationTokenSource loadCancelSource = new();

        private readonly ILogger logger = Debug.unityLogger;
        private const string logTag = nameof(RewardedAdPool);

#if UNITY_EDITOR
        public RewardedAdPool() {
            EditorApplication.playModeStateChanged -= this.OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state) {
            switch (state) {
                case PlayModeStateChange.ExitingEditMode:
                    this.Awake();

                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    this.OnDestroy();

                    break;
            }
        }
#endif

        private void Awake() {
            this.loadCancelSource = new CancellationTokenSource();
        }

        private void OnDestroy() {
            this.loadCancelSource.Cancel();

            while (this.ads.TryDequeue(out RefreshedAd refreshedAd)) {
                refreshedAd.CancelRefresh();
                refreshedAd.ad.Destroy();
            }
        }

        public bool TryGetAd([NotNullWhen(true)] out RewardedAd? ad) {
            this.PreloadAds(1).FireAndForget();

            if (!this.ads.TryDequeue(out RefreshedAd refreshedAd)) {
                ad = null;

                return false;
            }

            refreshedAd.CancelRefresh();
            ad = refreshedAd.ad;

            return true;
        }

        public async Task PreloadAds(int count) {
            IEnumerable<RewardedAd> ads;

            try {
                if (count == 1) {
                    ads = Enumerable.Repeat(await this.PrepareAd(this.loadCancelSource.Token), 1);
                } else {
                    ads = await Task.WhenAll(
                        Enumerable.Range(0, count)
                        .Select(_ => this.PrepareAd(this.loadCancelSource.Token))
                    );
                }
            } catch (TaskCanceledException) {
                return;
            }

            foreach (RewardedAd ad in ads) {
                this.ads.Enqueue(new RefreshedAd(ad, this));
            }
        }

        private async Task<RewardedAd> PrepareAd(CancellationToken cancel) {
            RewardedAd? ad = await Load(this);

            while (ad == null) {
                cancel.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(this.retryRate), cancel);

                ad = await Load(this);
            }

            ad.OnAdFullScreenContentClosed += OnAdClosed;
            ad.OnAdFullScreenContentFailed += OnAdFailed;

            return ad;

            static Task<RewardedAd?> Load(RewardedAdPool pool) {
                TaskCompletionSource<RewardedAd?> adSource = new();

                RewardedAd.Load(pool.adUnitId, new AdRequest(), (ad, error) => {
                    if (error != null) {
                        pool.logger.LogError(logTag, $"Failed to load ad: {error}", pool);

                        adSource.SetResult(null);

                        return;
                    }

                    adSource.SetResult(ad);
                });

                return adSource.Task;
            }

            void OnAdClosed() {
                ad.OnAdFullScreenContentClosed -= OnAdClosed;
                ad.Destroy();
            }

            void OnAdFailed(AdError error) {
                this.logger.LogError(logTag, $"Ad failed: {error}", this);

                ad.OnAdFullScreenContentFailed -= OnAdFailed;
                ad.Destroy();
            }
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

            public readonly void CancelRefresh() {
                this.refreshCancelSource.Cancel();
            }

            private async void RefreshLoop(CancellationToken cancel) {
                while (true) {
                    try {
                        await Task.Delay(TimeSpan.FromMinutes(this.pool.refreshRate), cancel);

                        this.ad = await this.pool.PrepareAd(cancel);
                    } catch (TaskCanceledException) { }

                    if (cancel.IsCancellationRequested) {
                        return;
                    }
                }
            }
        }
    }
}
