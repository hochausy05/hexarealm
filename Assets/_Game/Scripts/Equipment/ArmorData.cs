using UnityEngine;

namespace HexaRealm.Equipment
{
    /// <summary>Shared authoring data for the single currently supported player armor slot.</summary>
    [CreateAssetMenu(fileName = "ArmorData", menuName = "HexaRealm/Equipment/Armor Data")]
    public sealed class ArmorData : ScriptableObject
    {
        [SerializeField] private string displayName = "New Armor";
        [SerializeField] private float vitalityBonus;
        [SerializeField] private float attackBonus;
        [SerializeField] private float defenseBonus;
        [SerializeField] private float agilityBonus;
        [SerializeField] private float rageBonus;
        [SerializeField] private Sprite bodySprite;

        public string DisplayName => displayName;
        public float VitalityBonus => vitalityBonus;
        public float AttackBonus => attackBonus;
        public float DefenseBonus => defenseBonus;
        public float AgilityBonus => agilityBonus;
        public float RageBonus => rageBonus;
        public Sprite BodySprite => bodySprite;

        // This authoring helper supports isolated EditMode tests; gameplay never mutates ArmorData.
        public void SetAuthoringValues(
            string newDisplayName,
            float newVitalityBonus,
            float newAttackBonus,
            float newDefenseBonus,
            float newAgilityBonus,
            float newRageBonus,
            Sprite newBodySprite = null)
        {
            displayName = newDisplayName;
            vitalityBonus = SanitizeFinite(newVitalityBonus);
            attackBonus = SanitizeFinite(newAttackBonus);
            defenseBonus = SanitizeFinite(newDefenseBonus);
            agilityBonus = SanitizeFinite(newAgilityBonus);
            rageBonus = SanitizeFinite(newRageBonus);
            bodySprite = newBodySprite;
        }

        private void OnValidate()
        {
            vitalityBonus = SanitizeFinite(vitalityBonus);
            attackBonus = SanitizeFinite(attackBonus);
            defenseBonus = SanitizeFinite(defenseBonus);
            agilityBonus = SanitizeFinite(agilityBonus);
            rageBonus = SanitizeFinite(rageBonus);
        }

        private static float SanitizeFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) ? value : 0f;
        }
    }
}
