using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Utility
{
    public static class CameraUtility
    {
        public static bool IsVisibleFromCamera(Bounds bounds, Camera cam)
        {
            GeometryUtility.CalculateFrustumPlanes(cam, FrustumPlanes);
            return GeometryUtility.TestPlanesAABB(FrustumPlanes, bounds);
        }
        
        public static readonly Plane[] FrustumPlanes = new Plane[6];

        
        public static bool ScreenBoundsOverlap (Bounds nearObject, Bounds farObject, Camera camera)
        {
            return ScreenBoundsOverlap(nearObject, Pose.identity, farObject, Pose.identity, camera);
        }
        
        public static bool ScreenBoundsOverlap (Bounds nearLocalBounds, Pose nearPose, Bounds farLocalBounds, Pose farPose, Camera camera) 
        {

            var near = GetScreenRectFromBounds (nearLocalBounds, nearPose, camera);
            var far = GetScreenRectFromBounds (farLocalBounds, farPose, camera);

            // ensure far object is indeed further away than near object
            if (far.max.z > near.min.z) 
            {
                // Doesn't overlap on x axis
                if (far.max.x < near.min.x || far.min.x > near.max.x) 
                {
                    return false;
                }
                // Doesn't overlap on y axis
                if (far.max.y < near.min.y || far.min.y > near.max.y) 
                {
                    return false;
                }
                // Overlaps
                return true;
            }
            return false;
        }
        
        
        public static Bounds GetScreenRectFromBounds (Bounds worldSpaceBounds, Camera camera) 
        {
            return GetScreenRectFromBounds(worldSpaceBounds, Pose.identity, camera);
            
        }
        
        public static Bounds GetScreenRectFromBounds (Bounds localBounds, Pose localToWorld, Camera camera)
        {
            return GetScreenRectFromBounds(localBounds, localToWorld, camera.transform.ToPose(),
                camera.projectionMatrix);
        }
        
        public static Bounds GetScreenRectFromBounds (Bounds localBounds, Pose localToWorld, Pose cameraPose, Matrix4x4 projectionMatrix) 
        {
            Bounds result = new Bounds
            {
                min = Vector3.positiveInfinity,
                max = Vector3.positiveInfinity
            };

            bool anyPointIsInFrontOfCamera = false;

            for (int i = 0; i < 8; i++)
            {
                Vector3 localSpaceCorner = localBounds.center + Vector3.Scale(localBounds.size, Corners[i]);
                Vector3 worldSpaceCorner = localToWorld.TransformPoint(localSpaceCorner);
                Vector3 viewportSpaceCorner = WorldToViewportPoint (cameraPose, projectionMatrix, worldSpaceCorner);

                if (viewportSpaceCorner.z > 0) 
                {
                    anyPointIsInFrontOfCamera = true;
                } 
                else 
                {
                    // If point is behind camera, it gets flipped to the opposite side
                    // So clamp to opposite edge to correct for this
                    viewportSpaceCorner.x = (viewportSpaceCorner.x <= 0.5f) ? 1 : 0;
                    viewportSpaceCorner.y = (viewportSpaceCorner.y <= 0.5f) ? 1 : 0;
                }

                // Update bounds with new corner point
                result.Encapsulate(viewportSpaceCorner);
            }

            // All points are behind camera so just return empty bounds
            if (!anyPointIsInFrontOfCamera) 
                return new Bounds();

            return result;
        }

        public static readonly Vector3[] ScreenBoundsExtents = new Vector3[8];
        
        public static Vector3 WorldToViewportPoint(Pose cameraPose, Matrix4x4 cameraProjection, Vector3 worldPoint)
        {
            return WorldToViewportPoint(Matrix4x4.Scale(new Vector3(1, 1, -1)) * cameraPose.ToMatrix().inverse, cameraProjection, worldPoint);
        }
        
        public static Vector3 WorldToViewportPoint(Matrix4x4 cameraView, Matrix4x4 cameraProjection, Vector3 worldPoint)
        {
            Vector4 viewPos = (cameraView * new Vector4(worldPoint.x, worldPoint.y, worldPoint.z, 1.0f));
            Vector4 clipPos = (cameraProjection * viewPos);
            // -0.5 to 0.5
            Vector2 ndc = new Vector2(clipPos.x, clipPos.y) / clipPos.w;
            // 0 to 1
            Vector2 viewport = (ndc + Vector2.one) / 2.0f;
            return new Vector3(viewport.x, viewport.y, -viewPos.z);
        }
        
        
        
        public static Bounds GetRotatedBoxBounds(Vector3 center, Quaternion rotation, Vector3 size)
        {
            Bounds bounds = new Bounds(center, Vector3.zero);
            
            for (int i = 0; i < 8; i++)
            {
                Vector3 point = Corners[i];
                point.x *= size.x;
                point.y *= size.y;
                point.z *= size.z;
                
                bounds.Encapsulate(rotation * point + center);
            }

            return bounds;
        }
        
        public static readonly Vector3[] Corners = new[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, +0.5f),
            new Vector3(-0.5f, +0.5f, -0.5f),
            new Vector3(-0.5f, +0.5f, +0.5f),
            new Vector3(+0.5f, -0.5f, -0.5f),
            new Vector3(+0.5f, -0.5f, +0.5f),
            new Vector3(+0.5f, +0.5f, -0.5f),
            new Vector3(+0.5f, +0.5f, +0.5f),
        };
    }
}