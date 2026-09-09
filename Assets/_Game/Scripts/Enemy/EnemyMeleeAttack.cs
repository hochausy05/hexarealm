using System.Collections.Generic;
using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Enemy
{
    [RequireComponent(typeof(EnemyRuntime))]
    public sealed class EnemyMeleeAttack : MonoBehaviour
    {
        [SerializeField] private EnemyRuntime enemyRuntime;
        [SerializeField] private LayerMask targetLayerMask = 1 << 8;
        [SerializeField, Min(0f)] private float attackRange = 0.85f;
        [SerializeField, Min(0f)] private float attackCooldown = 1f;

        private float nextAttackTime;

        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float AttackValue => enemyRuntime != null && enemyRuntime.Data != null ? enemyRuntime.Data.Attack : 0f;

        private void Awake()
        {
            if (enemyRuntime == null)
            {
                enemyRuntime = GetComponent<EnemyRuntime>();
            }
        }

        public bool TryAttack(Transform expectedTarget)
        {
            if (expectedTarget == null || enemyRuntime == null || enemyRuntime.Data == null || Time.time < nextAttackTime)
            {
                return false;
            }

            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, attackRange, targetLayerMask);
            var hitReceivers = new HashSet<IRawDamageReceiver>();

            foreach (Collider2D overlap in overlaps)
            {
                if (overlap == null || overlap.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (!EnemyTargeting.TryFindDamageReceiver(
                        overlap.transform,
                        out Transform receiverTransform,
                        out IRawDamageReceiver receiver,
                        out _)
                    || receiverTransform != expectedTarget
                    || !hitReceivers.Add(receiver))
                {
                    continue;
                }

                receiver.TakeRawDamage(enemyRuntime.Data.Attack);
                nextAttackTime = Time.time + attackCooldown;
                return true;
            }

            return false;
        }

        private void OnValidate()
        {
            attackRange = Mathf.Max(0f, attackRange);
            attackCooldown = Mathf.Max(0f, attackCooldown);
        }
    }
}
