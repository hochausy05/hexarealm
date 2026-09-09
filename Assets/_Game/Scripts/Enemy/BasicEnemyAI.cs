using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Enemy
{
    [RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth), typeof(EnemyMeleeAttack))]
    public sealed class BasicEnemyAI : MonoBehaviour
    {
        public enum EnemyAIState
        {
            Idle,
            Chase,
            Attack,
            Return,
            Dead
        }

        [Header("References")]
        [SerializeField] private Rigidbody2D enemyRigidbody;
        [SerializeField] private EnemyRuntime enemyRuntime;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private EnemyMeleeAttack meleeAttack;

        [Header("Targeting (prototype)")]
        [SerializeField] private LayerMask targetLayerMask = 1 << 8;
        [SerializeField, Min(0.01f)] private float detectionInterval = 0.2f;
        [SerializeField, Min(0f)] private float detectionRange = 4f;
        [SerializeField, Min(0f)] private float loseAggroRange = 5f;
        [SerializeField, Min(0f)] private float maxLeashDistance = 6f;
        [SerializeField, Min(0f)] private float attackExitRange = 1.1f;
        [SerializeField, Min(0f)] private float homeStopDistance = 0.1f;

        private Transform target;
        private Health targetHealth;
        private Vector2 homePosition;
        private float nextDetectionTime;
        private bool isSubscribed;

        public EnemyAIState CurrentState { get; private set; } = EnemyAIState.Idle;
        public Vector2 HomePosition => homePosition;

        private void Awake()
        {
            ResolveReferences();
            homePosition = transform.position;
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToDeath();

            if (enemyHealth != null && enemyHealth.Health != null && enemyHealth.Health.IsDead)
            {
                SetState(EnemyAIState.Dead);
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromDeath();
            StopMovement();
        }

        private void Update()
        {
            if (CurrentState == EnemyAIState.Dead)
            {
                return;
            }

            switch (CurrentState)
            {
                case EnemyAIState.Idle:
                    UpdateIdle();
                    break;
                case EnemyAIState.Chase:
                    UpdateChase();
                    break;
                case EnemyAIState.Attack:
                    UpdateAttack();
                    break;
                case EnemyAIState.Return:
                    UpdateReturn();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (enemyRigidbody == null || CurrentState == EnemyAIState.Dead)
            {
                return;
            }

            if (CurrentState == EnemyAIState.Chase && target != null)
            {
                MoveTowards(target.position);
            }
            else if (CurrentState == EnemyAIState.Return)
            {
                MoveTowards(homePosition);
            }
            else
            {
                StopMovement();
            }
        }

        private void UpdateIdle()
        {
            StopMovement();

            if (Time.time < nextDetectionTime)
            {
                return;
            }

            nextDetectionTime = Time.time + detectionInterval;
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetLayerMask);

            foreach (Collider2D overlap in overlaps)
            {
                if (overlap == null || !EnemyTargeting.TryFindDamageReceiver(
                        overlap.transform,
                        out Transform receiverTransform,
                        out _,
                        out Health receiverHealth)
                    || receiverHealth == null
                    || receiverHealth.IsDead)
                {
                    continue;
                }

                target = receiverTransform;
                targetHealth = receiverHealth;
                SetState(Vector2.Distance(transform.position, target.position) <= meleeAttack.AttackRange
                    ? EnemyAIState.Attack
                    : EnemyAIState.Chase);
                return;
            }
        }

        private void UpdateChase()
        {
            if (!HasValidTarget() || IsOutsideLeash())
            {
                BeginReturn();
                return;
            }

            if (Vector2.Distance(transform.position, target.position) <= meleeAttack.AttackRange)
            {
                SetState(EnemyAIState.Attack);
            }
        }

        private void UpdateAttack()
        {
            StopMovement();

            if (!HasValidTarget() || IsOutsideLeash())
            {
                BeginReturn();
                return;
            }

            if (Vector2.Distance(transform.position, target.position) > attackExitRange)
            {
                SetState(EnemyAIState.Chase);
                return;
            }

            meleeAttack.TryAttack(target);
        }

        private void UpdateReturn()
        {
            if (Vector2.Distance(transform.position, homePosition) <= homeStopDistance)
            {
                StopMovement();
                SetState(EnemyAIState.Idle);
            }
        }

        private bool HasValidTarget()
        {
            return target != null && targetHealth != null && !targetHealth.IsDead &&
                Vector2.Distance(transform.position, target.position) <= loseAggroRange;
        }

        private bool IsOutsideLeash()
        {
            return Vector2.Distance(transform.position, homePosition) > maxLeashDistance;
        }

        private void BeginReturn()
        {
            target = null;
            targetHealth = null;
            SetState(EnemyAIState.Return);
        }

        private void MoveTowards(Vector2 destination)
        {
            if (enemyRigidbody == null || enemyHealth == null || enemyHealth.Health == null ||
                enemyHealth.Health.IsDead)
            {
                StopMovement();
                return;
            }

            if (enemyRuntime == null || enemyRuntime.Data == null)
            {
                StopMovement();
                return;
            }

            Vector2 direction = destination - enemyRigidbody.position;
            enemyRigidbody.linearVelocity = direction.sqrMagnitude > 0.0001f
                ? direction.normalized * enemyRuntime.Data.Speed
                : Vector2.zero;
        }

        private void HandleDied()
        {
            target = null;
            targetHealth = null;
            SetState(EnemyAIState.Dead);
        }

        private void SetState(EnemyAIState state)
        {
            CurrentState = state;

            if (state != EnemyAIState.Chase && state != EnemyAIState.Return)
            {
                StopMovement();
            }
        }

        private void StopMovement()
        {
            if (enemyRigidbody != null)
            {
                enemyRigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void SubscribeToDeath()
        {
            if (isSubscribed || enemyHealth == null || enemyHealth.Health == null)
            {
                return;
            }

            enemyHealth.Health.Died += HandleDied;
            isSubscribed = true;
        }

        private void UnsubscribeFromDeath()
        {
            if (!isSubscribed || enemyHealth == null || enemyHealth.Health == null)
            {
                return;
            }

            enemyHealth.Health.Died -= HandleDied;
            isSubscribed = false;
        }

        private void ResolveReferences()
        {
            if (enemyRigidbody == null)
            {
                enemyRigidbody = GetComponent<Rigidbody2D>();
            }

            if (enemyHealth == null)
            {
                enemyHealth = GetComponent<EnemyHealth>();
            }

            if (enemyRuntime == null)
            {
                enemyRuntime = GetComponent<EnemyRuntime>();
            }

            if (meleeAttack == null)
            {
                meleeAttack = GetComponent<EnemyMeleeAttack>();
            }
        }

        private void OnValidate()
        {
            detectionInterval = Mathf.Max(0.01f, detectionInterval);
            detectionRange = Mathf.Max(0f, detectionRange);
            loseAggroRange = Mathf.Max(detectionRange + 0.01f, loseAggroRange);
            maxLeashDistance = Mathf.Max(detectionRange, maxLeashDistance);
            attackExitRange = Mathf.Max(meleeAttack != null ? meleeAttack.AttackRange : 0f, attackExitRange);
            homeStopDistance = Mathf.Max(0f, homeStopDistance);
        }
    }
}
