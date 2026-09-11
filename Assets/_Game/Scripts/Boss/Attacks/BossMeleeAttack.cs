using System.Collections;
using System.Collections.Generic;
using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Boss.Attacks
{
    public sealed class BossMeleeAttack : BossAttackBase
    {
        [SerializeField, Min(0f)] private float range = 1.45f;
        [SerializeField] private Vector2 hitSize = new Vector2(2.2f, 1.35f);
        [SerializeField] private LayerMask targetLayerMask = 1 << 8;

        public override bool CanStart(Transform target, float distance) => target != null && distance <= range && IsReady;

        protected override IEnumerator ExecuteActive(Transform target)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            Vector2 center = (Vector2)transform.position + direction * (range * .5f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var hitReceivers = new HashSet<IRawDamageReceiver>();
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, hitSize, angle, targetLayerMask))
            {
                if (hit == null || hit.transform.IsChildOf(transform)) continue;
                IRawDamageReceiver receiver = FindDamageReceiver(hit.transform);
                if (receiver != null && hitReceivers.Add(receiver)) receiver.TakeRawDamage(RawDamage);
            }

            yield return new WaitForSeconds(activeDuration);
        }
    }
}
