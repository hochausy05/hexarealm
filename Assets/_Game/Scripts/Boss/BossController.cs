using System;
using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Boss
{
    [DisallowMultipleComponent]
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
        public bool IsDead => CurrentState == BossState.Dead;
        public bool HasResolvedReferences => body != null && body.gameObject == gameObject
            && runtime != null && runtime.gameObject == gameObject && runtime.HasValidData
            && bossHealth != null && bossHealth.gameObject == gameObject && bossHealth.enabled && bossHealth.HasResolvedReferences
            && combat != null && combat.gameObject == gameObject && combat.enabled && combat.HasResolvedReferences;
        public event Action Engaged;
        public event Action Disengaged;
        public event Action ResetToDormant;
        public event Action Died;

        private void Awake()
        {
            bool hasValidConfiguration = ResolveAndValidateReferences();
            homePosition = body != null ? body.position : transform.position;
            if (!hasValidConfiguration) DisableForInvalidConfiguration();
        }

        private void OnEnable()
        {
            if (!ResolveAndValidateReferences())
            {
                DisableForInvalidConfiguration();
                return;
            }

            bossHealth.Health.Died -= HandleDeath;
            bossHealth.Health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.Died -= HandleDeath;
            StopMovement();
        }

        private void FixedUpdate()
        {
            if (!HasResolvedReferences)
            {
                DisableForInvalidConfiguration();
                return;
            }

            if (CurrentState == BossState.Returning)
            {
                if (Vector2.Distance(body.position, homePosition) <= homeStopDistance)
                {
                    StopMovement();
                    bossHealth.Health.ResetToMaxHealth(runtime.Data.MaxHealth);
                    CurrentState = BossState.Dormant;
                    ResetToDormant?.Invoke();
                    return;
                }

                MoveTowards(homePosition);
                return;
            }

            if (CurrentState == BossState.Attacking) return;

            if (CurrentState == BossState.Engaged && target != null)
            {
                if (!combat.IsAttackRunning) combat.Tick(target);
                if (!combat.IsAttackRunning) MoveTowards(target.position);
                return;
            }

            StopMovement();
        }

        public void Engage(Transform player)
        {
            if (player == null || IsDead) return;
            if (!HasResolvedReferences)
            {
                DisableForInvalidConfiguration();
                return;
            }

            if (target == player && (CurrentState == BossState.Engaged || CurrentState == BossState.Attacking)) return;
            target = player;
            CurrentState = BossState.Engaged;
            Engaged?.Invoke();
        }

        public void Disengage()
        {
            if (IsDead || CurrentState == BossState.Dormant || CurrentState == BossState.Returning) return;
            if (!HasResolvedReferences)
            {
                DisableForInvalidConfiguration();
                return;
            }

            target = null;
            combat.CancelActiveAttack();
            StopMovement();
            CurrentState = BossState.Returning;
            Disengaged?.Invoke();
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
            target = null;
            combat.CancelActiveAttack();
            StopMovement();
            CurrentState = BossState.Dead;
            Died?.Invoke();
        }

        private void MoveTowards(Vector2 destination)
        {
            if (runtime == null || !runtime.HasValidData)
            {
                StopMovement();
                return;
            }

            Vector2 delta = destination - body.position;
            body.linearVelocity = delta.sqrMagnitude > .0001f ? delta.normalized * runtime.Data.MoveSpeed : Vector2.zero;
        }

        private void StopMovement()
        {
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void ResolveReferences()
        {
            if (body == null || body.gameObject != gameObject) body = GetComponent<Rigidbody2D>();
            if (runtime == null || runtime.gameObject != gameObject) runtime = GetComponent<BossRuntime>();
            if (bossHealth == null || bossHealth.gameObject != gameObject) bossHealth = GetComponent<BossHealth>();
            if (combat == null || combat.gameObject != gameObject) combat = GetComponent<BossCombatController>();
        }

        private bool ResolveAndValidateReferences()
        {
            ResolveReferences();
            if (bossHealth != null) bossHealth.ResolveAndValidateReferences();
            if (combat != null) combat.ResolveAndValidateReferences();
            return HasResolvedReferences;
        }

        private void DisableForInvalidConfiguration()
        {
            Debug.LogError(
                "BossController requires Rigidbody2D, BossRuntime with valid BossData, valid BossHealth, and a valid BossCombatController with at least one attack on the same Boss root.",
                this);
            StopMovement();
            enabled = false;
        }
    }
}
