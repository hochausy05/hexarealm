using System;
using HexaRealm.Interaction;
using UnityEngine;

namespace HexaRealm.Loot
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class LootChest : MonoBehaviour, IInteractable
    {
        private enum ChestState { Closed, Granting, Opened }

        [SerializeField] private LootBundleData lootBundle;
        [SerializeField] private bool autoEquipWeaponReward;
        [SerializeField] private bool autoEquipArmorReward;
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private GameObject openVisual;

        private ChestState state;
        private bool hasLoggedInvalidConfiguration;

        public event Action Opened;
        public bool IsOpened => state == ChestState.Opened;

        private void Awake() => RefreshVisual();
        private void OnEnable() => RefreshVisual();
        private void OnValidate() => RefreshVisual();

        public bool IsInteractionAvailable(PlayerInteractor interactor)
        {
            return interactor != null && state == ChestState.Closed;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!IsInteractionAvailable(interactor)) return;

            PlayerLootReceiver receiver = interactor.GetComponent<PlayerLootReceiver>();
            if (lootBundle == null || !lootBundle.HasAnyReward || receiver == null || !receiver.CanReceive(lootBundle))
            {
                LogInvalidConfigurationOnce();
                return;
            }

            state = ChestState.Granting;
            if (!receiver.TryReceive(lootBundle, autoEquipWeaponReward, autoEquipArmorReward))
            {
                state = ChestState.Closed;
                LogInvalidConfigurationOnce();
                return;
            }

            state = ChestState.Opened;
            RefreshVisual();
            Opened?.Invoke();
        }

        private void RefreshVisual()
        {
            bool isOpened = state == ChestState.Opened;
            if (closedVisual != null) closedVisual.SetActive(!isOpened);
            if (openVisual != null) openVisual.SetActive(isOpened);
        }

        private void LogInvalidConfigurationOnce()
        {
            if (hasLoggedInvalidConfiguration) return;
            hasLoggedInvalidConfiguration = true;
            Debug.LogWarning($"LootChest '{name}' remains closed: assign a non-empty LootBundleData and a valid PlayerLootReceiver.", this);
        }
    }
}
