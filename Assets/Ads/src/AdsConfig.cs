#nullable enable

using UnityEngine;

namespace BubbleShooter.Ads {
    [CreateAssetMenu(
        fileName = nameof(AdsConfig),
        menuName = AdsMenuNames.ads + nameof(AdsConfig)
    )]
    public sealed class AdsConfig : ScriptableObject {
        [SerializeField]
        private string extraPointsAdUnitId = "";

        public string ExtraPointsAdUnitId => this.extraPointsAdUnitId;
    }
}
