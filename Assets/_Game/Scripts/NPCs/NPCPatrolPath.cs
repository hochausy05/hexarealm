using UnityEngine;

namespace HexaRealm.NPC
{
    /// <summary>
    /// Scene-authored ordered route used by ambient village NPCs.
    /// </summary>
    public sealed class NPCPatrolPath : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;

        public int WaypointCount => waypoints == null ? 0 : waypoints.Length;

        public Transform GetWaypoint(int index)
        {
            if (index < 0 || index >= WaypointCount)
            {
                return null;
            }

            return waypoints[index];
        }

        private void OnDrawGizmos()
        {
            if (WaypointCount == 0)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.82f, 0.2f, 0.9f);
            Transform previous = null;

            for (int i = 0; i < WaypointCount; i++)
            {
                Transform waypoint = GetWaypoint(i);
                if (waypoint == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(waypoint.position, 0.12f);
                if (previous != null)
                {
                    Gizmos.DrawLine(previous.position, waypoint.position);
                }

                previous = waypoint;
            }

            Transform first = GetWaypoint(0);
            if (WaypointCount > 1 && previous != null && first != null)
            {
                Gizmos.DrawLine(previous.position, first.position);
            }
        }
    }
}
