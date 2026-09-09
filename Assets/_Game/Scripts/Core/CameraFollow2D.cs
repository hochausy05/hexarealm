using UnityEngine;

namespace HexaRealm.Core
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset;
        [SerializeField, Min(0f)] private float smoothTime = 0.12f;

        private Vector2 followVelocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector2 targetPosition = (Vector2)target.position + offset;
            Vector2 nextPosition = smoothTime > 0f
                ? Vector2.SmoothDamp(transform.position, targetPosition, ref followVelocity, smoothTime)
                : targetPosition;

            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
        }
    }
}
