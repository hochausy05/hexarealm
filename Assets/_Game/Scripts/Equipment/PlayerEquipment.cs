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
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private Sprite defaultBodySprite;

        [Header("Prototype Starting Equipment (not final balance)")]
        [SerializeField] private WeaponData startingWeapon;
        [SerializeField] private ArmorData startingArmor;

        private WeaponData equippedWeapon;
        private ArmorData equippedArmor;
        private bool hasInitializedStartingWeapon;
        private bool hasInitializedStartingArmor;
        private bool hasCapturedDefaultBodySprite;

        public WeaponData EquippedWeapon => equippedWeapon;
        public ArmorData EquippedArmor => equippedArmor;
        public float AttackSpeedMultiplier => equippedWeapon != null ? equippedWeapon.AttackSpeedMultiplier : 1f;
        public float CritBonus => equippedWeapon != null ? equippedWeapon.CritBonus : 0f;
        public float RangeBonus => equippedWeapon != null ? equippedWeapon.RangeBonus : 0f;
        public Sprite SlashVFXSprite => equippedWeapon != null ? equippedWeapon.SlashVFXSprite : null;

        private void Awake()
        {
            ResolveReferences();
            InitializeStartingWeapon();
            InitializeStartingArmor();
            RecalculateEquipmentModifiers();
            RefreshWeaponVisual();
            RefreshBodyVisual();
        }

        private void OnEnable()
        {
            ResolveReferences();
            InitializeStartingWeapon();
            InitializeStartingArmor();
            RecalculateEquipmentModifiers();
            RefreshWeaponVisual();
            RefreshBodyVisual();
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

        public void EquipArmor(ArmorData armor)
        {
            equippedArmor = armor;
            hasInitializedStartingArmor = true;
            RecalculateEquipmentModifiers();
            RefreshBodyVisual();
        }

        public void UnequipArmor()
        {
            EquipArmor(null);
        }

        /// <summary>Rebuilds all equipment stat totals from current slots; safe to call repeatedly.</summary>
        public void RecalculateEquipmentModifiers()
        {
            ResolveReferences();
            if (playerStats == null)
            {
                return;
            }

            float vitality = equippedArmor != null ? SanitizeFinite(equippedArmor.VitalityBonus) : 0f;
            float attack = equippedWeapon != null ? SanitizeNonNegative(equippedWeapon.Damage) : 0f;
            float defense = 0f;
            float agility = 0f;
            float rage = 0f;

            if (equippedArmor != null)
            {
                attack += SanitizeFinite(equippedArmor.AttackBonus);
                defense = SanitizeFinite(equippedArmor.DefenseBonus);
                agility = SanitizeFinite(equippedArmor.AgilityBonus);
                rage = SanitizeFinite(equippedArmor.RageBonus);
            }

            playerStats.SetEquipmentModifier(PlayerStatType.Vitality, vitality);
            playerStats.SetEquipmentModifier(PlayerStatType.Attack, attack);
            playerStats.SetEquipmentModifier(PlayerStatType.Defense, defense);
            playerStats.SetEquipmentModifier(PlayerStatType.Agility, agility);
            playerStats.SetEquipmentModifier(PlayerStatType.Rage, rage);
        }

        private void OnValidate()
        {
            ResolveReferences();
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

        private void InitializeStartingArmor()
        {
            if (hasInitializedStartingArmor)
            {
                return;
            }

            equippedArmor = startingArmor;
            hasInitializedStartingArmor = true;
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

        private void RefreshBodyVisual()
        {
            if (bodySpriteRenderer == null)
            {
                return;
            }

            Sprite sprite = equippedArmor != null && equippedArmor.BodySprite != null
                ? equippedArmor.BodySprite
                : defaultBodySprite;
            bodySpriteRenderer.sprite = sprite;
            bodySpriteRenderer.enabled = sprite != null;
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

            if (bodySpriteRenderer == null)
            {
                Transform bodyTransform = transform.Find("Body");
                bodySpriteRenderer = bodyTransform != null ? bodyTransform.GetComponent<SpriteRenderer>() : null;
            }

            if (!hasCapturedDefaultBodySprite && bodySpriteRenderer != null)
            {
                if (defaultBodySprite == null)
                {
                    defaultBodySprite = bodySpriteRenderer.sprite;
                }

                hasCapturedDefaultBodySprite = true;
            }
        }

        private static float SanitizeNonNegative(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) ? Mathf.Max(0f, value) : 0f;
        }

        private static float SanitizeFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) ? value : 0f;
        }
    }
}
