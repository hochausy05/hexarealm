using UnityEngine;
using UnityEngine.InputSystem;

namespace HexaRealm.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        private const string MoveActionPath = "Gameplay/Move";

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Min(0f)] private float movementSpeed = 4f;

        private Rigidbody2D playerRigidbody;
        private InputAction moveAction;
        private Vector2 movementDirection;
        private Vector2 lastMoveDirection;
        private bool movementLocked;

        public Vector2 CurrentMoveDirection => movementDirection;
        public Vector2 LastMoveDirection => lastMoveDirection;

        private void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody2D>();

            if (inputActions == null)
            {
                Debug.LogError("PlayerController requires an Input Action Asset.", this);
                enabled = false;
                return;
            }

            moveAction = inputActions.FindAction(MoveActionPath);

            if (moveAction == null)
            {
                Debug.LogError($"Input action '{MoveActionPath}' was not found.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            moveAction?.Enable();
        }

        private void Update()
        {
            movementDirection = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

            if (movementDirection.sqrMagnitude > 0f)
            {
                lastMoveDirection = movementDirection.normalized;
            }
        }

        private void FixedUpdate()
        {
            if (movementLocked)
            {
                return;
            }

            playerRigidbody.linearVelocity = movementDirection * movementSpeed;
        }

        internal void SetMovementLocked(bool locked)
        {
            movementLocked = locked;

            if (locked)
            {
                playerRigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            movementDirection = Vector2.zero;
            movementLocked = false;

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}
