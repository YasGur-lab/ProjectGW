using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GW.Control
{
    public class PatrolPath : MonoBehaviour
    {
        private const float m_WaypointGizmoRadius = 0.3f;

        private void OnDrawGizmos()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                int j = GetNextWaypoint(i);

                Gizmos.DrawSphere(GetWaypoint(i), m_WaypointGizmoRadius);
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
            }
        }

        public int GetNextWaypoint(int i)
        {
            if (i + 1 == transform.childCount)
                return 0;
            return i + 1;
        }

        public Vector3 GetWaypoint(int i)
        {
            return transform.GetChild(i).position;
        }
    }
}
