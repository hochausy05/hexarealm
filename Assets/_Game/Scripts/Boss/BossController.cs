using HexaRealm.Combat;
using System;
using UnityEngine;

namespace HexaRealm.Boss
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BossRuntime), typeof(BossHealth))]
    public sealed class BossController : MonoBehaviour
    {
        public enum BossState { Dormant, Engaged, Attacking, Returning, Dead }
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private BossRuntime runtime;
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private BossCombatController combat;
        [SerializeField, Min(0f)] private float homeStopDistance = .1f;

        private Transform target;
        private Vector2 homePosition;
        public BossState CurrentState { get; private set; } = BossState.Dormant;
        public Transform Target => target;
        public Vector2 HomePosition => homePosition;
        public bool IsDead => CurrentState == BossState.Dead;
        public event Action Engaged;
        public event Action ResetToDormant;
        public event Action Died;

        private void Awake() { ResolveReferences(); homePosition = body != null ? body.position : transform.position; }
        private void OnEnable() { ResolveReferences(); if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.Died += HandleDeath; }
        private void OnDisable() { if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.Died -= HandleDeath; StopMovement(); }
        private void Update()
        {
            if (CurrentState == BossState.Returning && Vector2.Distance(body.position, homePosition) <= homeStopDistance)
            {
                StopMovement(); bossHealth.RestoreFullHealth(); CurrentState = BossState.Dormant; ResetToDormant?.Invoke();
            }
        }
        private void FixedUpdate()
        {
            if (body == null) return;
            if (CurrentState == BossState.Engaged && target != null && !combat.IsAttackRunning) combat.Tick(target);
            if (CurrentState == BossState.Engaged && !combat.IsAttackRunning && target != null) MoveTowards(target.position);
            else if (CurrentState == BossState.Returning) MoveTowards(homePosition);
            else if (CurrentState != BossState.Attacking) StopMovement();
        }
        public void Engage(Transform player)
        {
            if (player == null || IsDead) return;
            target = player; CurrentState = BossState.Engaged; Engaged?.Invoke();
        }
        public void Disengage()
        {
            if (IsDead) return;
            target = null; combat.CancelActiveAttack(); StopMovement(); CurrentState = BossState.Returning;
        }
        public void SetAttacking(bool attacking)
        {
            if (IsDead) return;
            CurrentState = attacking ? BossState.Attacking : target != null ? BossState.Engaged : BossState.Returning;
            if (attacking) StopMovement();
        }
        private void HandleDeath()
        {
            if (IsDead) return;
            target = null; combat.CancelActiveAttack(); StopMovement(); CurrentState = BossState.Dead; Died?.Invoke();
        }
        private void MoveTowards(Vector2 destination)
        {
            if (runtime == null || runtime.Data == null) return;
            Vector2 delta = destination - body.position;
            body.linearVelocity = delta.sqrMagnitude > .0001f ? delta.normalized * runtime.Data.MoveSpeed : Vector2.zero;
        }
        private void StopMovement() { if (body != null) body.linearVelocity = Vector2.zero; }
        private void ResolveReferences() { if (body == null) body = GetComponent<Rigidbody2D>(); if (runtime == null) runtime = GetComponent<BossRuntime>(); if (bossHealth == null) bossHealth = GetComponent<BossHealth>(); if (combat == null) combat = GetComponent<BossCombatController>(); }
    }
}
