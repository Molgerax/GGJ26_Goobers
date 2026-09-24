using UnityEngine;

namespace GGJ.Utility.Extensions
{
    public static class TransformExtensions
    {
        public static void CopyPositionAndRotation(this Transform transform, Transform target)
        {
            transform.SetPositionAndRotation(target.position, target.rotation);
        }

        public static Pose ToLocalPose(this Transform transform)
        {
            transform.GetLocalPositionAndRotation(out var position, out var rotation);
            return new Pose(position, rotation);
        }

        public static Pose ToPose(this Transform transform)
        {
            transform.GetPositionAndRotation(out var position, out var rotation);
            return new Pose(position, rotation);
        }

        public static Matrix4x4 ToMatrix(this Pose pose)
        {
            return Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
        }
        
        public static Matrix4x4 ToViewMatrix(this Pose pose)
        {
            return Matrix4x4.Scale(new Vector3(1, 1, -1)) * pose.ToMatrix().inverse;
        }

        public static Vector3 InverseTransformPoint(this Pose pose, Vector3 point)
        {
            point = point - pose.position;
            return Quaternion.Inverse(pose.rotation) * point;
        }
        
        public static Vector3 TransformPoint(this Pose pose, Vector3 point)
        {
            point = pose.rotation * point;
            return point + pose.position;
        }
        public static Vector3 TransformVector(this Pose pose, Vector3 vector)
        {
            return pose.rotation * vector;
        }


        public static bool IsInFrontOf(this Transform transform, Vector3 point)
        {
            Vector3 diff = point - transform.position;
            return Vector3.Dot(diff, transform.forward) > 0;
        }
        
        public static bool IsBehind(this Transform transform, Vector3 point)
        {
            Vector3 diff = point - transform.position;
            return Vector3.Dot(diff, transform.forward) < 0;
        }
    }
}
