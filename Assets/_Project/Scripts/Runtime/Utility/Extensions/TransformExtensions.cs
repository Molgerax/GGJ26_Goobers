using UnityEngine;

namespace GGJ.Utility.Extensions
{
    public static class TransformExtensions
    {
        public static void CopyPositionAndRotation(this Transform transform, Transform target)
        {
            transform.SetPositionAndRotation(target.position, target.rotation);
        }
    }
}
