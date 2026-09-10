using UnityEngine;

namespace HexaRealm.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerAreaTransition : MonoBehaviour
    {
        private Rigidbody2D playerRigidbody;
        private PlayerController playerController;
        private PlayerDash playerDash;
        private int lastTransitionFrame = -1;

        private void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
            playerController = GetComponent<PlayerController>();
            playerDash = GetComponent<PlayerDash>();
        }

        public bool TryTransition(Transform destination)
        {
            if (destination == null || lastTransitionFrame == Time.frameCount) return false;

            lastTransitionFrame = Time.frameCount;
            playerDash?.ResetForAreaTransition();
            playerController?.SetMovementLocked(true);
            playerRigidbody.position = destination.position;
            playerRigidbody.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            playerController?.SetMovementLocked(false);
            return true;
        }
    }
}
