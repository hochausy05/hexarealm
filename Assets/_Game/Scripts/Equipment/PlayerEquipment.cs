using HexaRealm.Combat;
using HexaRealm.Player;
using UnityEngine;

namespace HexaRealm.Equipment
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStats))]
    public sealed class PlayerEquipment : MonoBehaviour, IWeaponCombatModifiers
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private SpriteRenderer weaponSpriteRenderer;

        [Header("Prototype Starting Equipment (not final balance)")]
        [SerializeField] private WeaponData startingWeapon;

        private WeaponData equippedWeapon;
        private bool hasInitializedStartingWeapon;

        public WeaponData EquippedWeapon => equippedWeapon;
        public float AttackSpeedMultiplier => equippedWeapon != null ? equippedWeapon.AttackSpeedMultiplier : 1f;
        public float CritBonus => equippedWeapon != null ? equippedWeapon.CritBonus : 0f;
        public float RangeBonus => equippedWeapon != null ? equippedWeapon.RangeBonus : 0f;
        public Sprite SlashVFXSprite => equippedWeapon != null ? equippedWeapon.SlashVFXSprite : null;

        private void Awake()
        {
            ResolveReferences();
            InitializeStartingWeapon();
            RecalculateEquipmentModifiers();
            RefreshWeaponVisual();
        }

        private void OnEnable()
        {
            ResolveReferences();
            InitializeStartingWeapon();
            RecalculateEquipmentModifiers();
            RefreshWeaponVisual();
        }

        public void EquipWeapon(WeaponData weapon)
        {
            equippedWeapon = weapon;
            hasInitializedStartingWeapon = true;
            RecalculateEquipmentModifiers();
            RefreshWeaponVisual();
        }

        public void UnequipWeapon()
        {
            EquipWeapon(null);
        }

        /// <summary>Rebuilds all equipment stat totals from current slots; safe to call repeatedly.</summary>
        public void RecalculateEquipmentModifiers()
        {
            ResolveReferences();
            if (playerStats == null)
            {
                return;
            }

            float weaponAttack = equippedWeapon != null ? SanitizeNonNegative(equippedWeapon.Damage) : 0f;
            playerStats.SetEquipmentModifier(PlayerStatType.Vitality, 0f);
            playerStats.SetEquipmentModifier(PlayerStatType.Attack, weaponAttack);
            playerStats.SetEquipmentModifier(PlayerStatType.Defense, 0f);
            playerStats.SetEquipmentModifier(PlayerStatType.Agility, 0f);
            playerStats.SetEquipmentModifier(PlayerStatType.Rage, 0f);
        }

        private void OnValidate()
        {
            ResolveReferences();
            if (!Application.isPlaying)
            {
                RefreshWeaponVisual();
            }
        }

        private void InitializeStartingWeapon()
        {
            if (hasInitializedStartingWeapon)
            {
                return;
            }

            equippedWeapon = startingWeapon;
            hasInitializedStartingWeapon = true;
        }

        private void RefreshWeaponVisual()
        {
            if (weaponSpriteRenderer == null)
            {
                return;
            }

            Sprite sprite = equippedWeapon != null ? equippedWeapon.WeaponSprite : null;
            weaponSpriteRenderer.sprite = sprite;
            weaponSpriteRenderer.enabled = sprite != null;
        }

        private void ResolveReferences()
        {
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            if (weaponSpriteRenderer == null)
            {
                Transform weaponTransform = transform.Find("WeaponSprite");
                weaponSpriteRenderer = weaponTransform != null ? weaponTransform.GetComponent<SpriteRenderer>() : null;
            }
        }

        private static float SanitizeNonNegative(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) ? Mathf.Max(0f, value) : 0f;
        }
    }
}
