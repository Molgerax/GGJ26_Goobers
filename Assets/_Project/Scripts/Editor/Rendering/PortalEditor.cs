using System;
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
    }
}
