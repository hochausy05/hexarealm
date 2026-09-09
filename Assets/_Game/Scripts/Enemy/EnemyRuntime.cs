using UnityEngine;

namespace HexaRealm.Enemy
{
    /// <summary>
    /// Owns the single EnemyData reference used by runtime enemy components.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class EnemyRuntime : MonoBehaviour
    {
        [SerializeField] private EnemyData data;

        public EnemyData Data => data;

        private void Awake()
        {
            if (data == null || !data.IsValid)
            {
                Debug.LogError("EnemyRuntime requires a valid EnemyData asset.", this);
                enabled = false;
            }
        }
    }
}
