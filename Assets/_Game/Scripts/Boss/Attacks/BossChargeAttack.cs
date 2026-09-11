using System.Collections;
using System.Collections.Generic;
using HexaRealm.Combat;
using HexaRealm.Enemy;
using UnityEngine;

namespace HexaRealm.Boss.Attacks
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossChargeAttack : BossAttackBase
    {
        [SerializeField, Min(0f)] private float minimumRange = 2f;
        [SerializeField, Min(0f)] private float maximumRange = 6f;
        [SerializeField, Min(0f)] private float chargeSpeed = 7f;
        [SerializeField, Min(0f)] private float hitRadius = .7f;
        [SerializeField] private LayerMask targetLayerMask = 1 << 8;
        private Rigidbody2D body;
        private Vector2 lockedDirection;
        private bool stoppedByCollision;
        private readonly HashSet<IRawDamageReceiver> hitReceivers = new HashSet<IRawDamageReceiver>();
        protected override void Awake() { base.Awake(); body = GetComponent<Rigidbody2D>(); }
        public override bool CanStart(Transform target, float distance) => target != null && distance >= minimumRange && distance <= maximumRange && !IsRunning;
        protected override IEnumerator ExecuteActive(Transform target)
        {
            lockedDirection = ((Vector2)target.position - body.position).normalized;
            hitReceivers.Clear(); stoppedByCollision = false; float endTime = Time.time + activeDuration;
            while (Time.time < endTime && !stoppedByCollision)
            {
                body.linearVelocity = lockedDirection * chargeSpeed;
                Collider2D[] hits = Physics2D.OverlapCircleAll(body.position, hitRadius, targetLayerMask);
                foreach (Collider2D hit in hits)
                {
                    if (hit == null || hit.transform.IsChildOf(transform) || !EnemyTargeting.TryFindDamageReceiver(hit.transform, out _, out IRawDamageReceiver receiver, out _) || !hitReceivers.Add(receiver)) continue;
                    receiver.TakeRawDamage(RawDamage);
                }
                yield return new WaitForFixedUpdate();
            }
        }
        protected override void OnCancelledOrFinished() { if (body != null) body.linearVelocity = Vector2.zero; hitReceivers.Clear(); }
        private void OnCollisionEnter2D(Collision2D collision) { if (IsRunning) { stoppedByCollision = true; if (body != null) body.linearVelocity = Vector2.zero; } }
    }
}
