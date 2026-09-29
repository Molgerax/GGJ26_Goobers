using System;
using System.Collections.Generic;
using System.Linq;
using GGJ.Rendering.Portals;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace GGJ.Editor.Rendering
{
    [CustomEditor(typeof(Portal))]
    public class PortalEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }


        private void DuringSceneGUI(SceneView sceneView)
        {
            Portal p = target as Portal;
            
            
            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                Camera cam = Handles.currentCamera;

                BeginDefaultDraw();

                DrawPlaneIntersections(out _);
                for (var i = 0; i < PortalRenderFeature.ClippingPlanes.Length; i++)
                {
                    Color cyan = Color.HSVToRGB(1, 0, i / 5f);
                    cyan.a = 0.5f;

                    Handles.color = cyan;
                    var plane = PortalRenderFeature.ClippingPlanes[i];
                    DrawFrustumPlane(plane);
                }

                EndDefaultDraw();
                
                return;
                
                ScaledPose camPose = cam.transform.ToPose();
                Matrix4x4 mat =  (cam.projectionMatrix * camPose.ToViewMatrix()).inverse;
                Handles.matrix = mat;

                if (!CameraUtility.TryGetScreenRectFromBounds(p.Bounds, ScaledPose.identity, camPose.ToViewMatrix(), cam.projectionMatrix, out var screenBounds))
                    return;

                Handles.color = Color.green;
                Handles.DrawWireCube(screenBounds.center, screenBounds.size);

                DrawPortalsRecursive(p, camPose, screenBounds, cam.projectionMatrix, 5, 0);
            }
        }

        private Matrix4x4 _cachedMatrix;

        private void BeginDefaultDraw()
        {
            _cachedMatrix = Handles.matrix;
            Handles.matrix = Matrix4x4.identity;
        }

        private void EndDefaultDraw()
        {
            Handles.matrix = _cachedMatrix;
        }
        
        
        
        private void DrawPortalsRecursive(Portal p, ScaledPose camPose, Bounds screenBounds, Matrix4x4 projMat,
            int maxRecursionLevel, int recursionLevel)
        {
            if (!p.OtherPortal)
                return;
            
            ScaledPose newCamPose = Portal.GetCameraPose(p, p.OtherPortal, camPose);
            
            foreach (Portal activePortal in Portal.ActivePortals)
            {
                if (recursionLevel >= maxRecursionLevel)
                    return;
                
                if (!activePortal.OtherPortal)
                    continue;
                
                if (activePortal == p.OtherPortal)
                    continue;
                
                if (!activePortal.transform.IsInFrontOf(newCamPose.position))
                    continue;
                
                if(!CameraUtility.TryGetScreenRectFromBounds(activePortal.Bounds, ScaledPose.identity, newCamPose.ToViewMatrix(), projMat, out var newScreenBounds))
                    continue;


                Color col = Color.red;
                col.a = (maxRecursionLevel - recursionLevel) / (float)maxRecursionLevel;
                Handles.color = col;
                Handles.DrawWireCube(newScreenBounds.center, newScreenBounds.size); 
                
                
                col = Color.yellow;
                col.a = (maxRecursionLevel - recursionLevel) / (float)maxRecursionLevel;
                Handles.color = col;
                if (CameraUtility.ScreenBoundsOverlap(out Bounds summedBounds, screenBounds, newScreenBounds))
                {
                    Handles.DrawWireCube(summedBounds.center, summedBounds.size);
                    
                    DrawPortalsRecursive(activePortal, newCamPose, summedBounds, projMat, maxRecursionLevel, recursionLevel + 1);
                }
            }
        }
        

        static Vector3 ScreenToWorld(float x, float y, float z, Camera camera) {
            return camera.ScreenToWorldPoint(new Vector3(x, camera.pixelHeight - y, z));
        }


        static void DrawScreenRect(Bounds bounds, Camera camera)
        {
            Vector3 min = bounds.min.With(y: camera.pixelHeight - bounds.min.y);
            Vector3 max = bounds.max.With(y: camera.pixelHeight - bounds.max.y);

            min.z = (max.z + min.z) / 2;
            max.z = (max.z + min.z) / 2;

            Vector2 center = (max + min) / 2;
            Vector2 size = (max - min);
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);
            
            Rect rect = new Rect(center, size);
            
            Handles.BeginGUI();
            GUI.Box(rect, GUIContent.none);
            Handles.EndGUI();
        }
        
        static Rect ScreenBoundsToScreenRect(Bounds bounds, Camera camera)
        {
            Vector3 min = ScreenToWorld(bounds.min.x, bounds.min.y, bounds.min.z, camera);
            Vector3 max = ScreenToWorld(bounds.max.x, bounds.max.y, bounds.max.z, camera);
            Vector2 center = (max + min) / 2;
            Vector2 size = (max - min);
            return new Rect(center, size);
        }
        
        
        public static void DrawPlane(Plane p, Vector3 center, float width, int count = 16)
        {
            Vector3 closest = p.ClosestPointOnPlane(center);
            Vector3 pos = -p.normal * p.distance;
            Handles.matrix = Matrix4x4.TRS(pos, Quaternion.LookRotation(p.normal), Vector3.one);

            
            for (int x = 0; x < count; x++)
            {
                float t = x / (count - 1f);
                t = 2 * t - 1;
                Handles.DrawLine(
                    new Vector3(-width, t * width, 0),
                    new Vector3(+width, t * width, 0));
                
                Handles.DrawLine(
                    new Vector3(t * width, -width, 0),
                    new Vector3(t * width, +width, 0));
            }
        }

        public static Vector3[] CachedPoints = new Vector3[4];
        
        public static void DrawFrustumPlane(Plane p)
        {
            int pointIndex = 0;
            float epsilon = 0.001f;
            for (int i = 0; i < CornerListLength; i++)
            {
                var corner = CornerList[i];

                if (p.GetDistanceToPoint(corner) < epsilon)
                    CachedPoints[pointIndex++] = corner;
                
                if (pointIndex >= 4)
                    break;
            }
            if (pointIndex == 0)
                return;
            
            SortArrayByWinding(p.normal, ref CachedPoints, pointIndex);
            
            
            Handles.DrawAAConvexPolygon(CachedPoints);
        }

        private static void SortArrayByWinding(Vector3 normal, ref Vector3[] array, int size)
        {
            size = Mathf.Min(array.Length, size);
            
            Vector3 center = Vector3.zero;
            for (int i = 0; i < size; i++)
                center += array[i];
            center /= size;

            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, normal);
            if (right.sqrMagnitude == 0)
            {
                right = Vector3.right;
            }
            up = Vector3.Cross(normal, right).normalized;
            right = right.normalized;


            var list = new List<Vector3>();
            for (int i = 0; i < size; i++)
            {
                list.Add(array[i]);
            }
            
            var l = list.OrderBy(x => Math.Atan2(Vector3.Dot(x - center, right), Vector3.Dot(x - center, up)));

            int it = 0;
            foreach (var vector3 in l)
            {
                array[it++] = vector3;
            }
        }
        
        public static Vector3[] CornerList = new Vector3[16];
        public static int CornerListLength = 0;
        
        public static void DrawPlaneIntersections(out Vector3 frustumMiddle)
        {
            CornerListLength = 0;
         
            frustumMiddle = Vector3.zero;
            
            for (int a = 0; a < 6; a++)
            {
                for (int b = 0; b < 6; b++)
                {
                    for (int c = 0; c < 6; c++)
                    {
                        if (a == b || a == c || b == c)
                            continue;
                        
                        if (PlanesIntersectAtSinglePoint(
                                PortalRenderFeature.ClippingPlanes[a], 
                                PortalRenderFeature.ClippingPlanes[b], 
                                PortalRenderFeature.ClippingPlanes[c], out Vector3 point))
                        {
                            if (!IsPointInsideFrustum(point))
                                continue;
                                
                            if (CornerListLength < 16)
                                CornerList[CornerListLength++] = point;
                            //Handles.SphereHandleCap(-1, point, 0.1f);
                        }
                    }
                }
            }

            if (CornerListLength == 0)
                return;
            
            return;
            
            for (int index = 0; index < CornerListLength; index++)
            {
                frustumMiddle += CornerList[index];
            }

            frustumMiddle /= CornerListLength;
            
            for (int j = 0; j < CornerListLength; j++)
            {
                for (int k = 0; k < CornerListLength; k++)
                {
                    if (j == k)
                        continue;
                    Handles.DrawLine(CornerList[j], CornerList[k]);
                }
            }
        }

        private static bool IsPointInsideFrustum(Vector3 point)
        {
            bool output = true;

            for (int i = 0; i < 6; i++)
            {
                Plane p = PortalRenderFeature.ClippingPlanes[i];
                if (p.GetDistanceToPoint(point) < -0.001f)
                    output = false;
            }

            return output;
        }
        
        private static bool PlanesIntersectAtSinglePoint( Plane p0, Plane p1, Plane p2, out Vector3 intersectionPoint )
        {
            const float EPSILON = 1e-4f;

            var det = Vector3.Dot( Vector3.Cross( p0.normal, p1.normal ), p2.normal );
            if( det < EPSILON )
            {
                intersectionPoint = Vector3.zero;
                return false;
            }

            intersectionPoint = 
                ( -( p0.distance * Vector3.Cross( p1.normal, p2.normal ) ) -
                  ( p1.distance * Vector3.Cross( p2.normal, p0.normal ) ) -
                  ( p2.distance * Vector3.Cross( p0.normal, p1.normal ) ) ) / det;

            return true;
        }
    }
}
