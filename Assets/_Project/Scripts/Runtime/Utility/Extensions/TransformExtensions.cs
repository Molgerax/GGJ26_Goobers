using UnityEngine;

namespace GGJ.Utility.Extensions
{
    public static class TransformExtensions
    {
        public static void CopyPositionAndRotation(this Transform transform, Transform target)
        {
            transform.SetPositionAndRotation(target.position, target.rotation);
        }

        public static ScaledPose ToLocalPose(this Transform transform)
        {
            transform.GetLocalPositionAndRotation(out var position, out var rotation);
            return new ScaledPose(position, rotation, transform.localScale);
        }

        public static ScaledPose ToPose(this Transform transform)
        {
            transform.GetPositionAndRotation(out var position, out var rotation);
            return new ScaledPose(position, rotation);
        }

        public static Matrix4x4 ToMatrix(this ScaledPose pose)
        {
            return Matrix4x4.TRS(pose.position, pose.rotation, pose.scale);
        }
        
        public static Matrix4x4 ToViewMatrix(this ScaledPose pose)
        {
            return Matrix4x4.Scale(new Vector3(1, 1, -1)) * pose.ToMatrix().inverse;
        }

        public static Vector3 InverseTransformPoint(this ScaledPose pose, Vector3 point)
        {
            point = point - pose.position;
            return Quaternion.Inverse(pose.rotation) * point;
        }
        
        public static Vector3 TransformPoint(this ScaledPose pose, Vector3 point)
        {
            point = pose.rotation * point;
            return point + pose.position;
        }
        public static Vector3 TransformVector(this ScaledPose pose, Vector3 vector)
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
