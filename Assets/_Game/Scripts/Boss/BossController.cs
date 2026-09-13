using System;
using System.Text;
using HexaRealm.Boss.Attacks;
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
            if (this == null) return;
            Debug.LogError(BuildInvalidConfigurationDiagnostic(), this);
            StopMovement();
            enabled = false;
        }

        private string BuildInvalidConfigurationDiagnostic()
        {
            bool bodyOnSameRoot = body != null && body.gameObject == gameObject;
            bool runtimeOnSameRoot = runtime != null && runtime.gameObject == gameObject;
            bool runtimeHasValidData = runtime != null && runtime.HasValidData;
            bool bossHealthExists = bossHealth != null;
            bool combatExists = combat != null;
            BossData data = runtime != null ? runtime.Data : null;

            var report = new StringBuilder("BossController invalid configuration:");
            report.Append("\n  Body exists and is on same root: ").Append(bodyOnSameRoot);
            report.Append("\n  BossRuntime exists and is on same root: ").Append(runtimeOnSameRoot);
            report.Append("\n  BossRuntime.HasValidData: ").Append(runtimeHasValidData);
            report.Append("\n  BossData.MaxHealth: ").Append(data != null ? data.MaxHealth.ToString() : "<null>");
            report.Append("\n  BossData.Attack: ").Append(data != null ? data.Attack.ToString() : "<null>");
            report.Append("\n  BossData.Defense: ").Append(data != null ? data.Defense.ToString() : "<null>");
            report.Append("\n  BossData.MoveSpeed: ").Append(data != null ? data.MoveSpeed.ToString() : "<null>");
            report.Append("\n  BossHealth exists: ").Append(bossHealthExists);
            report.Append("\n  BossHealth enabled: ").Append(bossHealthExists && bossHealth.enabled);
            report.Append("\n  BossHealth.HasResolvedReferences: ").Append(bossHealthExists && bossHealth.HasResolvedReferences);
            report.Append("\n  BossCombatController exists: ").Append(combatExists);
            report.Append("\n  BossCombatController enabled: ").Append(combatExists && combat.enabled);
            report.Append("\n  BossCombatController.HasResolvedReferences: ").Append(combatExists && combat.HasResolvedReferences);
            report.Append("\n  BossCombatController.AttackCount: ").Append(combatExists ? combat.AttackCount : 0);

            if (combat != null && combat.ConfiguredAttacks != null)
            {
                for (int index = 0; index < combat.ConfiguredAttacks.Count; index++)
                {
                    BossAttackBase attack = combat.ConfiguredAttacks[index];
                    bool attackExists = attack != null;
                    report.Append("\n  Attack[").Append(index).Append("]:");
                    report.Append(" type/name=").Append(attackExists ? attack.GetType().Name + "/" + attack.name : "<null>");
                    report.Append(", null=").Append(!attackExists);
                    report.Append(", enabled=").Append(attackExists && attack.enabled);
                    report.Append(", same root=").Append(attackExists && attack.gameObject == gameObject);
                    report.Append(", HasValidRuntime=").Append(attackExists && attack.HasValidRuntime);
                }
            }

            return report.ToString();
        }
    }
}
