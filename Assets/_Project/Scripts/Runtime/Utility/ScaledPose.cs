using UnityEngine;

namespace GGJ.Utility
{
    public struct ScaledPose
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Vector3 forward => rotation * Vector3.forward;

        public static ScaledPose identity => new ScaledPose(Vector3.zero, Quaternion.identity, Vector3.one);
        
        public ScaledPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
        
        public ScaledPose(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = Vector3.one;
        }
    }
}