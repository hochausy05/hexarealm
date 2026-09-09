using UnityEngine;
using UnityEngine.InputSystem;

namespace HexaRealm.Interaction
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        private const string InteractActionPath = "Gameplay/Interact";

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.25f;
        [SerializeField] private LayerMask interactableLayerMask;

        private readonly Collider2D[] nearbyColliders = new Collider2D[16];
        private InputAction interactAction;

        private void Awake()
        {
            if (inputActions != null)
            {
                interactAction = inputActions.FindAction(InteractActionPath);
            }
        }

        private void OnEnable()
        {
            if (interactAction == null) return;
            interactAction.performed += HandleInteractPerformed;
            interactAction.Enable();
        }

        private void OnDisable()
        {
            if (interactAction == null) return;
            interactAction.performed -= HandleInteractPerformed;
            interactAction.Disable();
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            IInteractable target = FindClosestInteractable();
            target?.Interact(this);
        }

        private IInteractable FindClosestInteractable()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, interactionRadius, nearbyColliders, interactableLayerMask);
            IInteractable closest = null;
            float closestDistanceSquared = float.MaxValue;

            for (int index = 0; index < count; index++)
            {
                Collider2D collider = nearbyColliders[index];
                nearbyColliders[index] = null;
                if (collider == null) continue;

                MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>();
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (!(behaviour is IInteractable interactable) || !interactable.IsInteractionAvailable(this)) continue;
                    float distanceSquared = ((Vector2)collider.ClosestPoint(transform.position) - (Vector2)transform.position).sqrMagnitude;
                    if (distanceSquared < closestDistanceSquared)
                    {
                        closestDistanceSquared = distanceSquared;
                        closest = interactable;
                    }
                }
            }

            return closest;
        }
    }
}
