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

        public static Vector3 GetPointOnSphere(int i, int count)
        {
            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            float angleIncrement = Mathf.PI * 2 * goldenRatio;

            float t = (float)i / count;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            return new Vector3(x, y, z);
        }
    }
}