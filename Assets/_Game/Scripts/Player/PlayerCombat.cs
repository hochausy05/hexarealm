using System.Collections.Generic;
using HexaRealm.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HexaRealm.Player
{
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(PlayerController), typeof(PlayerStats), typeof(PlayerHealth))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        private const string AttackActionPath = "Gameplay/Attack";
        private const string PointActionPath = "Gameplay/Point";
        private const float DirectionThreshold = 0.0001f;

        [Header("References")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerDash playerDash;
        [SerializeField] private Transform weaponSpriteTransform;
        [SerializeField] private GameObject slashVFX;
        [SerializeField] private Camera gameplayCamera;

        [Header("Melee Query (prototype)")]
        [SerializeField, Min(0f)] private float attackOffset = 0.75f;
        [SerializeField] private Vector2 attackSize = new Vector2(1.2f, 0.8f);
        [SerializeField] private LayerMask targetLayerMask = 1 << 14;

        [Header("Combat Formula (prototype, not final balance)")]
        [SerializeField, Min(0f)] private float baseAttackInterval = 0.6f;
        [SerializeField, Range(0f, 1f)] private float baseCriticalChance = 0.05f;
        [SerializeField, Min(0f)] private float criticalChancePerRage = 0.01f;
        [SerializeField, Min(1f)] private float criticalMultiplier = 2f;
        [SerializeField, Min(0f)] private float attackSpeedPerRage = 0.05f;
        [SerializeField, Min(0f)] private float slashVFXDuration = 0.12f;

        private InputAction attackAction;
        private InputAction pointAction;
        private IWeaponCombatModifiers weaponCombatModifiers;
        private SpriteRenderer slashVFXRenderer;
        private Sprite defaultSlashVFXSprite;
        private float nextAttackTime;
        private float slashVFXEndTime;

        public Vector2 LastAttackDirection { get; private set; }
        public Vector2 LastValidAimDirection { get; private set; }
        public bool LastSwingWasCritical { get; private set; }

        private void Awake()
        {
            ResolveReferences();
            slashVFXRenderer = slashVFX != null ? slashVFX.GetComponent<SpriteRenderer>() : null;
            defaultSlashVFXSprite = slashVFXRenderer != null ? slashVFXRenderer.sprite : null;

            if (inputActions == null)
            {
                Debug.LogError("PlayerCombat requires an Input Action Asset.", this);
                enabled = false;
                return;
            }

            attackAction = inputActions.FindAction(AttackActionPath);
            pointAction = inputActions.FindAction(PointActionPath);

            if (attackAction == null)
            {
                Debug.LogError($"Input action '{AttackActionPath}' was not found.", this);
                enabled = false;
                return;
            }

            if (pointAction == null)
            {
                Debug.LogWarning(
                    $"Input action '{PointActionPath}' was not found. PlayerCombat will use movement direction as a fallback.",
                    this);
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            attackAction?.Enable();
            pointAction?.Enable();
            SetSlashVFXActive(false);
        }

        private void Update()
        {
            if (slashVFX != null && slashVFX.activeSelf && Time.time >= slashVFXEndTime)
            {
                slashVFX.SetActive(false);
            }

            if (attackAction != null && attackAction.WasPressedThisFrame())
            {
                TryStartAttack();
            }

            UpdateWeaponVisualAim();
        }

        private bool TryStartAttack()
        {
            if (playerHealth == null || playerHealth.Health == null || playerHealth.Health.IsDead)
            {
                return false;
            }

            if (playerDash != null && playerDash.IsDashing)
            {
                return false;
            }

            if (Time.time < nextAttackTime || !TryGetAttackDirection(out Vector2 attackDirection))
            {
                return false;
            }

            LastAttackDirection = attackDirection;
            float finalRage = playerStats.GetFinalStat(PlayerStatType.Rage);
            float criticalChance = CombatMath.CalculateCriticalChance(
                baseCriticalChance,
                finalRage,
                criticalChancePerRage,
                GetWeaponCombatModifiers().CritBonus);

            // One roll belongs to the whole swing, so every receiver gets the same crit result.
            LastSwingWasCritical = criticalChance >= 1f
                || (criticalChance > 0f && Random.value < criticalChance);
            float rawDamage = CombatMath.CalculateRawDamage(
                playerStats.GetFinalStat(PlayerStatType.Attack),
                LastSwingWasCritical,
                criticalMultiplier);

            float attackInterval = CombatMath.CalculateAttackInterval(
                baseAttackInterval,
                finalRage,
                attackSpeedPerRage,
                GetWeaponCombatModifiers().AttackSpeedMultiplier);
            nextAttackTime = Time.time + attackInterval;

            PerformDamageQuery(attackDirection, rawDamage);
            ShowSlashVFX(attackDirection);
            return true;
        }

        private bool TryGetAttackDirection(out Vector2 attackDirection)
        {
            if (TryGetPointerAimDirection(out attackDirection))
            {
                return true;
            }

            if (LastValidAimDirection.sqrMagnitude > DirectionThreshold)
            {
                attackDirection = LastValidAimDirection;
                return true;
            }

            Vector2 requestedDirection = playerController.CurrentMoveDirection.sqrMagnitude > DirectionThreshold
                ? playerController.CurrentMoveDirection
                : playerController.LastMoveDirection;

            if (requestedDirection.sqrMagnitude <= DirectionThreshold)
            {
                attackDirection = Vector2.zero;
                return false;
            }

            attackDirection = requestedDirection.normalized;
            return true;
        }

        private bool TryGetPointerAimDirection(out Vector2 aimDirection)
        {
            aimDirection = Vector2.zero;

            if (pointAction == null || gameplayCamera == null || Pointer.current == null)
            {
                return false;
            }

            Vector2 pointerScreenPosition = pointAction.ReadValue<Vector2>();
            Vector3 playerPosition = transform.position;
            float screenDepth = Mathf.Abs(playerPosition.z - gameplayCamera.transform.position.z);
            Vector3 pointerWorldPosition = gameplayCamera.ScreenToWorldPoint(
                new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, screenDepth));
            Vector2 aimVector = (Vector2)(pointerWorldPosition - playerPosition);

            if (aimVector.sqrMagnitude <= DirectionThreshold)
            {
                return false;
            }

            LastValidAimDirection = aimVector.normalized;
            aimDirection = LastValidAimDirection;
            return true;
        }

        private void PerformDamageQuery(Vector2 attackDirection, float rawDamage)
        {
            float effectiveReach = CombatMath.CalculateMeleeReach(attackOffset, GetWeaponCombatModifiers().RangeBonus);
            Vector2 center = (Vector2)transform.position + attackDirection * effectiveReach;
            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(center, GetValidatedAttackSize(), angle, targetLayerMask);
            ApplyDamageToUniqueReceivers(overlaps, rawDamage, transform);
        }

        private static int ApplyDamageToUniqueReceivers(
            Collider2D[] overlaps,
            float rawDamage,
            Transform sourceRoot)
        {
            var hitReceivers = new HashSet<IRawDamageReceiver>();

            foreach (Collider2D overlap in overlaps)
            {
                if (overlap == null || overlap.transform.IsChildOf(sourceRoot))
                {
                    continue;
                }

                IRawDamageReceiver receiver = FindDamageReceiver(overlap.transform);

                if (receiver == null)
                {
                    continue;
                }

                if (receiver is Component receiverComponent && receiverComponent.transform.IsChildOf(sourceRoot))
                {
                    continue;
                }

                if (hitReceivers.Add(receiver))
                {
                    receiver.TakeRawDamage(rawDamage);
                }
            }

            return hitReceivers.Count;
        }

        private static IRawDamageReceiver FindDamageReceiver(Transform start)
        {
            Transform current = start;

            while (current != null)
            {
                MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();

                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is IRawDamageReceiver receiver)
                    {
                        return receiver;
                    }
                }

                current = current.parent;
            }

            return null;
        }

        private void ShowSlashVFX(Vector2 attackDirection)
        {
            if (slashVFX == null)
            {
                return;
            }

            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            Transform slashTransform = slashVFX.transform;
            slashTransform.localPosition = attackDirection * CombatMath.CalculateMeleeReach(
                attackOffset,
                GetWeaponCombatModifiers().RangeBonus);
            slashTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            if (slashVFXRenderer != null)
            {
                Sprite overrideSprite = GetWeaponCombatModifiers().SlashVFXSprite;
                slashVFXRenderer.sprite = overrideSprite != null ? overrideSprite : defaultSlashVFXSprite;
            }
            slashVFX.SetActive(true);
            slashVFXEndTime = Time.time + Mathf.Max(0f, slashVFXDuration);
        }

        private Vector2 GetValidatedAttackSize()
        {
            return new Vector2(
                Mathf.Max(0f, attackSize.x),
                Mathf.Max(0f, attackSize.y));
        }

        private void ResolveReferences()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (playerDash == null)
            {
                playerDash = GetComponent<PlayerDash>();
            }

            if (weaponSpriteTransform == null)
            {
                Transform weaponTransform = transform.Find("WeaponSprite");
                weaponSpriteTransform = weaponTransform;
            }

            if (weaponCombatModifiers == null)
            {
                weaponCombatModifiers = GetComponent(typeof(IWeaponCombatModifiers)) as IWeaponCombatModifiers;
            }
        }

        private IWeaponCombatModifiers GetWeaponCombatModifiers()
        {
            if (weaponCombatModifiers == null)
            {
                weaponCombatModifiers = GetComponent(typeof(IWeaponCombatModifiers)) as IWeaponCombatModifiers;
            }

            return weaponCombatModifiers ?? NeutralWeaponCombatModifiers.Instance;
        }

        private void UpdateWeaponVisualAim()
        {
            if (weaponSpriteTransform == null || !TryGetAttackDirection(out Vector2 aimDirection))
            {
                return;
            }

            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            weaponSpriteTransform.localPosition = aimDirection * attackOffset;
            weaponSpriteTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void SetSlashVFXActive(bool active)
        {
            if (slashVFX != null)
            {
                slashVFX.SetActive(active);
            }
        }

        private void OnDisable()
        {
            attackAction?.Disable();
            pointAction?.Disable();
            SetSlashVFXActive(false);
            slashVFXEndTime = 0f;
        }

        private void OnValidate()
        {
            attackOffset = Mathf.Max(0f, attackOffset);
            attackSize = GetValidatedAttackSize();
            baseAttackInterval = Mathf.Max(0f, baseAttackInterval);
            baseCriticalChance = Mathf.Clamp01(baseCriticalChance);
            criticalChancePerRage = Mathf.Max(0f, criticalChancePerRage);
            criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
            attackSpeedPerRage = Mathf.Max(0f, attackSpeedPerRage);
            slashVFXDuration = Mathf.Max(0f, slashVFXDuration);
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 direction = Application.isPlaying && TryGetAttackDirection(out Vector2 currentAimDirection)
                ? currentAimDirection
                : LastAttackDirection.sqrMagnitude > DirectionThreshold
                    ? LastAttackDirection
                : Vector2.right;
            Vector2 center = (Vector2)transform.position + direction * CombatMath.CalculateMeleeReach(
                attackOffset,
                GetWeaponCombatModifiers().RangeBonus);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;
            Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
            Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.5f);
            Gizmos.DrawWireCube(Vector3.zero, GetValidatedAttackSize());
            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }

        private sealed class NeutralWeaponCombatModifiers : IWeaponCombatModifiers
        {
            public static readonly NeutralWeaponCombatModifiers Instance = new NeutralWeaponCombatModifiers();

            public float AttackSpeedMultiplier => 1f;
            public float CritBonus => 0f;
            public float RangeBonus => 0f;
            public Sprite SlashVFXSprite => null;
        }
    }
}
