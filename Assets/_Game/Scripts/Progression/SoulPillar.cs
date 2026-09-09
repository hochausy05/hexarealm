using HexaRealm.Interaction;
using UnityEngine;

namespace HexaRealm.Progression
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class SoulPillar : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(0.1f)] private float panelCloseDistance = 2f;

        private SoulPillarPanel activePanel;
        private PlayerInteractor activeInteractor;

        public bool IsInteractionAvailable(PlayerInteractor interactor)
        {
            return interactor != null;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (activePanel != null && activePanel.IsOpen && activeInteractor == interactor)
            {
                ClosePanel();
                return;
            }

            PlayerUpgradeProgression progression = interactor.GetComponent<PlayerUpgradeProgression>();
            if (progression == null)
            {
                return;
            }

            activeInteractor = interactor;
            activePanel = SoulPillarPanel.GetOrCreate();
            activePanel.Open(progression, ClosePanel);
        }

        private void Update()
        {
            if (activePanel == null || !activePanel.IsOpen || activeInteractor == null)
            {
                return;
            }

            if (Vector2.Distance(transform.position, activeInteractor.transform.position) > panelCloseDistance)
            {
                ClosePanel();
            }
        }

        private void OnDisable()
        {
            ClosePanel();
        }

        private void ClosePanel()
        {
            if (activePanel != null)
            {
                activePanel.Close();
            }

            activePanel = null;
            activeInteractor = null;
        }
    }
}
