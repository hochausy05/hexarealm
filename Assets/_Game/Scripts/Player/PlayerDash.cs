using UnityEngine;
using UnityEngine.InputSystem;

namespace HexaRealm.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerController))]
    public sealed class PlayerDash : MonoBehaviour
    {
        private const string DashActionPath = "Gameplay/Dash";

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Min(0f)] private float dashSpeed = 10f;
        [SerializeField, Min(0f)] private float dashDuration = 0.15f;
        [SerializeField, Min(0f)] private float dashCooldown = 0.65f;

        private Rigidbody2D playerRigidbody;
        private PlayerController playerController;
        private InputAction dashAction;
        private Vector2 dashDirection;
        private float dashTimeRemaining;
        private float cooldownRemaining;

        public bool IsDashing { get; private set; }

        private void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
            playerController = GetComponent<PlayerController>();

            if (inputActions == null)
            {
                Debug.LogError("PlayerDash requires an Input Action Asset.", this);
                enabled = false;
                return;
            }

            dashAction = inputActions.FindAction(DashActionPath);

            if (dashAction == null)
            {
                Debug.LogError($"Input action '{DashActionPath}' was not found.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            dashAction?.Enable();
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= Time.deltaTime;
            }

            if (dashAction.WasPressedThisFrame())
            {
                TryStartDash();
            }
        }

        private void FixedUpdate()
        {
            if (!IsDashing)
            {
                return;
            }

            playerRigidbody.linearVelocity = dashDirection * dashSpeed;
            dashTimeRemaining -= Time.fixedDeltaTime;

            if (dashTimeRemaining <= 0f)
            {
                EndDash();
            }
        }

        private void TryStartDash()
        {
            if (IsDashing || cooldownRemaining > 0f)
            {
                return;
            }

            Vector2 requestedDirection = playerController.CurrentMoveDirection.sqrMagnitude > 0f
                ? playerController.CurrentMoveDirection
                : playerController.LastMoveDirection;

            if (requestedDirection.sqrMagnitude <= 0f)
            {
                return;
            }

            dashDirection = requestedDirection.normalized;
            dashTimeRemaining = dashDuration;
            cooldownRemaining = dashCooldown;
            IsDashing = true;
            playerController.SetMovementLocked(true);
        }

        private void EndDash()
        {
            IsDashing = false;
            playerRigidbody.linearVelocity = Vector2.zero;
            playerController.SetMovementLocked(false);
        }

        private void OnDisable()
        {
            dashAction?.Disable();

            if (IsDashing)
            {
                EndDash();
            }
        }
    }
}
