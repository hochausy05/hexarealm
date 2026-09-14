using System;
using UnityEngine;

namespace HexaRealm.Progression
{
    /// <summary>Holds the player's runtime Soul balance.</summary>
    public sealed class PlayerSoulWallet : MonoBehaviour
    {
        [SerializeField, Min(0)] private int currentSouls;

        public event Action<int, int> SoulsChanged;

        public int CurrentSouls => currentSouls;

        public void AddSouls(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetCurrentSouls((int)Math.Min((long)currentSouls + amount, int.MaxValue));
        }

        public bool CanAfford(int amount)
        {
            return amount > 0 && currentSouls >= amount;
        }

        public bool TrySpendSouls(int amount)
        {
            if (!CanAfford(amount))
            {
                return false;
            }

            SetCurrentSouls(currentSouls - amount);
            return true;
        }

        /// <summary>Restores the authoritative balance without applying reward or spending semantics.</summary>
        public void RestoreExactSouls(int amount)
        {
            SetCurrentSouls(Mathf.Max(0, amount));
        }

        private void SetCurrentSouls(int value)
        {
            int newValue = Mathf.Max(0, value);

            if (currentSouls == newValue)
            {
                return;
            }

            int oldValue = currentSouls;
            currentSouls = newValue;
            SoulsChanged?.Invoke(oldValue, currentSouls);
        }

        private void OnValidate()
        {
            currentSouls = Mathf.Max(0, currentSouls);
        }
    }
}
