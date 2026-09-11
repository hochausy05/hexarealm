using UnityEngine;

namespace HexaRealm.Boss
{
    [DefaultExecutionOrder(-200)]
    public sealed class BossRuntime : MonoBehaviour
    {
        [SerializeField] private BossData data;
        public BossData Data => data;

        private void Awake()
        {
            if (data == null || !data.IsValid)
            {
                Debug.LogError("BossRuntime requires a valid BossData asset.", this);
                enabled = false;
            }
        }
    }
}
