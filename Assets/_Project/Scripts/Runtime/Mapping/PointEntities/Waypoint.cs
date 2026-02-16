using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("waypoint")]
    public class Waypoint : MonoBehaviour, IOnImportFromMapEntity
    {
        [SerializeField, Tremble("target")] private Waypoint[] waypoints;

        [SerializeField, Tremble("stop")] private bool stopPoint = false;

        [SerializeField, NoTremble] private Waypoint previousWaypoint;

        public bool IsStop => stopPoint;
        
        public Vector3 GetTangent(Waypoint waypoint)
        {
            if (!waypoint)
                return transform.forward;
            
            if (waypoint && previousWaypoint)
            {
                return (waypoint.transform.position - previousWaypoint.transform.position).normalized;
            }
            return transform.forward;
        }
        
        public Vector3 GetTangent(int index = 0)
        {
            if (waypoints == null || waypoints.Length == 0)
                return transform.forward;
            
            Waypoint waypoint = waypoints[index % waypoints.Length];
            return GetTangent(waypoint);
        }
        
        public Waypoint GetNextWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0)
                return null;

            int index = Random.Range(0, waypoints.Length);
            return waypoints[index];
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.aquamarine;
            
            Gizmos.DrawWireCube(transform.position, Vector3.one * 4);
            
            if (waypoints == null || waypoints.Length == 0)
                return;

            foreach (Waypoint waypoint in waypoints)
            {
                if (!waypoint)
                    return;

                Vector3 posA = transform.position;
                Vector3 posB = waypoint.transform.position;

                Vector3 diff = posB - posA;
                
                Gizmos.color = Color.aquamarine;
                Vector3 previousPos = posA;
                int resolution = 16;
                for (int i = 1; i < resolution; i++)
                {
                    float t = i / (resolution - 1f);
                    Vector3 pos = Interpolate(this, waypoint, t, out _);
                    Gizmos.DrawLine(previousPos, pos);
                    previousPos = pos;
                }
                
                Gizmos.color = Color.red;
                Gizmos.DrawRay(posA, GetTangent(waypoint) * diff.magnitude * 0.2f);
            }
        }

        public static float GetDistance(Waypoint a, Waypoint b)
        {
            if (!a || !b)
                return 0;
        
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            Vector3 dirA = a.GetTangent(b);
            Vector3 dirB = b.GetTangent();
            
            float sum = 0;
            int resolution = 16;
            Vector3 posPrevious = posA;
            for (int i = 1; i < resolution; i++)
            {
                float t = i / (resolution - 1f);
            
                Vector3 evaluateA = Interpolate(posA, dirA, posB,
                    dirB, t, out _);
                Vector3 evaluateB = Interpolate(posA, dirA, posB,
                    dirB, t, out _);

                Vector3 pos = Vector3.Lerp(evaluateA, evaluateB, t);
                sum += Vector3.Distance(pos, posPrevious);
                posPrevious = pos;
            }
            return sum;
        }
        
        public static Vector3 Interpolate(Waypoint a, Waypoint b, float t, out Vector3 forward)
        {
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            Vector3 dirA = a.GetTangent(b);
            Vector3 dirB = b.GetTangent();

            Vector3 forwardA, forwardB;
            
            Vector3 evaluateA = Interpolate(posA, dirA, posB,
                dirB, t, out forwardA);
            Vector3 evaluateB = Interpolate(posA, dirA, posB,
                dirB, t, out forwardB);

            forward = Vector3.Slerp(forwardA, forwardB, t);
            return Vector3.Lerp(evaluateA, evaluateB, t);
        }

        private static Vector3 Interpolate(Vector3 a, Vector3 dirA, Vector3 b, Vector3 dirB, float t, out Vector3 forward)
        {
            forward = (b - a).normalized;
            
            return Vector3.Lerp(a, b, t);
        }

        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            foreach (Waypoint waypoint in waypoints)
            {
                waypoint.previousWaypoint = this;
            }
        }
    }
}
