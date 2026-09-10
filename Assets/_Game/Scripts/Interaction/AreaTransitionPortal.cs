using HexaRealm.Player;
using UnityEngine;

namespace HexaRealm.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class AreaTransitionPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform destination;

        public Transform Destination => destination;

        public bool IsInteractionAvailable(PlayerInteractor interactor)
        {
            return interactor != null && destination != null &&
                   interactor.GetComponent<PlayerAreaTransition>() != null;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!IsInteractionAvailable(interactor)) return;
            interactor.GetComponent<PlayerAreaTransition>().TryTransition(destination);
        }
    }
}
