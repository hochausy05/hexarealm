using System.Collections;
using System.Collections.Generic;
using HexaRealm.Combat;
using HexaRealm.Enemy;
using UnityEngine;

namespace HexaRealm.Boss.Attacks
{
    public sealed class BossMeleeAttack : BossAttackBase
    {
        [SerializeField, Min(0f)] private float range = 1.45f;
        [SerializeField] private Vector2 hitSize = new Vector2(2.2f, 1.35f);
        [SerializeField] private LayerMask targetLayerMask = 1 << 8;
        public override bool CanStart(Transform target, float distance) => target != null && distance <= range && !IsRunning;
        protected override IEnumerator ExecuteActive(Transform target)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)transform.position + direction * (range * .5f), hitSize, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, targetLayerMask);
            var receivers = new HashSet<IRawDamageReceiver>();
            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit.transform.IsChildOf(transform) || !EnemyTargeting.TryFindDamageReceiver(hit.transform, out _, out IRawDamageReceiver receiver, out _) || !receivers.Add(receiver)) continue;
                receiver.TakeRawDamage(RawDamage);
            }
            yield return new WaitForSeconds(activeDuration);
        }
        private void OnDrawGizmosSelected() { Gizmos.color = Color.red; Gizmos.DrawWireCube(transform.position, hitSize); }
    }
}
