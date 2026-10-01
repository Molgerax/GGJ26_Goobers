using UnityEngine;

namespace GGJ.Utility.Extensions
{
    public static class PlaneExtensions
    {
        public static Plane TransformRaw(this Plane plane, Matrix4x4 transform)
        {
            Vector4 p = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
            p = transform * p;
            return new Plane(p, p.w);
        }
        
        public static Plane Transform(this Plane plane, Matrix4x4 transform)
        {
            Vector4 p = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
            p = transform.inverse.transpose * p;
            return new Plane(p, p.w);
        }
    }
}