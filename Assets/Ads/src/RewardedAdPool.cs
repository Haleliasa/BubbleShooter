#nullable enable

using BubbleShooter.Core;
using GoogleMobileAds.Api;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BubbleShooter.Ads {
    [CreateAssetMenu(
        fileName = nameof(RewardedAdPool),
        menuName = AdsMenuNames.ads + nameof(RewardedAdPool)
    )]
    public class RewardedAdPool : ScriptableObject {
        [SerializeField]
        private string adUnitId = "";

        [Min(0)]
        [SerializeField]
        private int startSize = 1;

        [Tooltip("min")]
        [Min(1)]
        [SerializeField]
        private int refreshRate = 50;

        private Queue<RefreshedAd> ads = null!;

        private void Awake() {
            this.ads = new Queue<RefreshedAd>(this.startSize);
        }

        public void Init() {
            for (int i = 0; i < this.startSize; i++) {
                this.AddRefreshedAd().FireAndForget();
            }
        }

        //public ValueTask<RewardedAd> GetAd() {
        //    if (this.ads.TryDequeue(out RefreshedAd refreshedAd)) {

        //    }
        //}

        private async Task AddRefreshedAd() {
            RewardedAd ad = await this.LoadAd();
            this.ads.Enqueue(new RefreshedAd(ad, this));
        }

        private Task<RewardedAd> LoadAd() {
            TaskCompletionSource<RewardedAd> taskSource = new();
            RewardedAd.Load(this.adUnitId, new AdRequest(), (ad, error) => {
                
            });

            return taskSource.Task;
        }

        private struct RefreshedAd {
            public RewardedAd ad;
            private readonly RewardedAdPool pool;
            private readonly CancellationTokenSource cancelSource;

            public RefreshedAd(RewardedAd ad, RewardedAdPool pool) {
                this.ad = ad;
                this.pool = pool;
                this.cancelSource = new CancellationTokenSource();

                this.RefreshLoop(this.cancelSource.Token);
            }

            public readonly void Cancel() {
                this.cancelSource.Cancel();
            }

            private async void RefreshLoop(CancellationToken cancel) {
                while (true) {
                    try {
                        await Task.Delay(this.pool.refreshRate, cancel);
                    } catch (TaskCanceledException) {
                        return;
                    }

                    this.ad = await this.pool.LoadAd();
                }
            }
        }
    }
}
