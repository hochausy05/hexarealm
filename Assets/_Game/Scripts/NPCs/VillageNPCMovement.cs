using UnityEngine;

namespace HexaRealm.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class VillageNPCMovement : MonoBehaviour
    {
        private enum MovementState
        {
            Idle,
            Walk
        }

        [SerializeField] private NPCPatrolPath patrolPath;
        [SerializeField, Min(0f)] private float moveSpeed = 1.35f;
        [SerializeField, Min(0f)] private float idleDuration = 1.25f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.08f;
        [SerializeField, Min(0)] private int startingWaypointIndex;

        private Rigidbody2D npcRigidbody;
        private MovementState state;
        private int currentWaypointIndex;
        private float idleTimeRemaining;

        private void Awake()
        {
            npcRigidbody = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            currentWaypointIndex = GetStartingWaypointIndex();
            EnterIdle();
        }

        private void FixedUpdate()
        {
            if (!HasLoopingPath())
            {
                StopMovement();
                return;
            }

            if (state == MovementState.Idle)
            {
                StopMovement();
                idleTimeRemaining -= Time.fixedDeltaTime;
                if (idleTimeRemaining <= 0f)
                {
                    state = MovementState.Walk;
                }

                return;
            }

            Transform target = patrolPath.GetWaypoint(currentWaypointIndex);
            if (target == null)
            {
                EnterIdle();
                return;
            }

            Vector2 offset = (Vector2)target.position - npcRigidbody.position;
            float arrivalDistanceSquared = arrivalDistance * arrivalDistance;
            if (offset.sqrMagnitude <= arrivalDistanceSquared)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolPath.WaypointCount;
                EnterIdle();
                return;
            }

            npcRigidbody.linearVelocity = offset.normalized * moveSpeed;
        }

        private bool HasLoopingPath()
        {
            return patrolPath != null
                && patrolPath.WaypointCount > 1
                && patrolPath.GetWaypoint(currentWaypointIndex) != null;
        }

        private int GetStartingWaypointIndex()
        {
            if (patrolPath == null || patrolPath.WaypointCount == 0)
            {
                return 0;
            }

            return startingWaypointIndex % patrolPath.WaypointCount;
        }

        private void EnterIdle()
        {
            state = MovementState.Idle;
            idleTimeRemaining = idleDuration;
            StopMovement();
        }

        private void StopMovement()
        {
            if (npcRigidbody != null)
            {
                npcRigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void OnDisable()
        {
            StopMovement();
        }
    }
}
