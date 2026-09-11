using UnityEngine;

namespace HexaRealm.Boss
{
    [DisallowMultipleComponent]
    public sealed class BossRuntime : MonoBehaviour
    {
        [SerializeField] private BossData data;
        public BossData Data => data;
        public bool HasValidData => data != null && data.IsValid;
    }
}
