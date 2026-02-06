using UnityEngine;

namespace GGJ.Utility
{
    public static class VectorMath
    {
        public static float GetDotProduct(Vector3 vector, Vector3 direction)
        {
            return Vector3.Dot(vector, direction.normalized);
        }
        
        public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction)
        {
            return vector - ExtractDotVector(vector, direction);
        }
        
        public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
        {
            direction.Normalize();
            return direction * Vector3.Dot(vector, direction);
        }

        public static Vector3 SetMagnitudeOfDirection(Vector3 vector3, Vector3 direction, float magnitude)
        {
            direction.Normalize();
            return RemoveDotVector(vector3, direction) + direction * magnitude;
        }
        
        public static Vector3 ProjectToPlaneAndScale(Vector3 vector, Vector3 normal)
        {
            float magnitude = vector.magnitude;
            return Vector3.ProjectOnPlane(vector, normal.normalized).normalized * magnitude;
        }
    }
}