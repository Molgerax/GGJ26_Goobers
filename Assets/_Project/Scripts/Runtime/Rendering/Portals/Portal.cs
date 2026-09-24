using System;
using System.Collections.Generic;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Rendering.Portals
{
    [ExecuteInEditMode]
    public class Portal : MonoBehaviour, IComparable
    {
        public static readonly List<Portal> ActivePortals = new();
        
        [SerializeField] private Vector2 size;
        
        [SerializeField] private Portal otherPortal;
        [SerializeField, Range(0, 10)] private int iteration = 0;

        public static float PortalDepth => 0.5f;
        
        private Camera _mainCamera;

        public Vector2 Size => size;
        public Portal OtherPortal => otherPortal;

        private float _distanceToCamera;

        public void UpdateDistanceToCamera(Camera cam)
        {
            _distanceToCamera = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
        }

        public Bounds Bounds
        {
            get
            {
                Pose pose = transform.ToPose();
                Vector3 offset = pose.rotation * new Vector3(0, 0, -0.5f * PortalDepth);
                return CameraUtility.GetRotatedBoxBounds(pose.position + offset, pose.rotation, new Vector3(size.x, size.y, PortalDepth));
            }
        }


        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            ActivePortals.Add(this);
        }

        private void OnDisable()
        {
            ActivePortals.Remove(this);
        }

        public bool IsFacingView(Vector3 cameraForward)
        {
            float dot = Vector3.Dot(cameraForward, transform.forward);
            return dot <= 0;
        }
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3( size.x, size.y, 0.01f));
            
            Bounds bounds = Bounds;
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            if (!_mainCamera)
                return;

            if (!otherPortal)
                return;

            Gizmos.color = new(1, 0, 0, 0.25f);
            bool overlap = CameraUtility.ScreenBoundsOverlap(bounds, otherPortal.Bounds, _mainCamera);

            if (overlap)
                Gizmos.color = new(0, 1, 0, 0.25f);
            Gizmos.DrawCube(bounds.center, bounds.size);
            
            
            
            float t = iteration / 10f;
            Gizmos.color = Color.HSVToRGB(t, 1, 1);

            Pose camPose = GetCameraPose(this, otherPortal, _mainCamera.transform.ToPose(), iteration);
            
            Gizmos.matrix = Matrix4x4.TRS(camPose.position, camPose.rotation, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, 1f);
            Gizmos.DrawRay(Vector3.zero, Vector3.forward * 2f);
        }


        public static Pose GetCameraPose(Portal inPortal, Portal outPortal, Pose cameraPose, int iterationID = 0)
        {
            Pose inPose = inPortal.transform.ToPose();
            Pose outPose = outPortal.transform.ToPose();
            
            for (int i = 0; i <= iterationID; i++)
            {
                Vector3 relativePos = inPose.InverseTransformPoint(cameraPose.position);
                relativePos = Quaternion.Euler(0, 180, 0) * relativePos;
                cameraPose.position = outPose.TransformPoint(relativePos);

                Quaternion relativeRot = Quaternion.Inverse(inPose.rotation) * cameraPose.rotation;
                relativeRot = Quaternion.Euler(0, 180, 0) * relativeRot;
                cameraPose.rotation = outPose.rotation * relativeRot;
            }

            return cameraPose;
        }

        public static Matrix4x4 GetProjectionMatrix(Pose outPortalPose, Pose portalCameraPose, Camera mainCamera)
        {
            Plane p = new Plane(outPortalPose.forward, outPortalPose.position);
            Vector4 clipPlaneWorldSpace = new(p.normal.x, p.normal.y, p.normal.z, p.distance);
            Vector4 clipPlaneCameraSpace = 
                Matrix4x4.Transpose(Matrix4x4.Inverse(portalCameraPose.ToViewMatrix())) 
                * clipPlaneWorldSpace;

            return mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }

        public int CompareTo(object obj)
        {
            var a = this;
            var b = obj as Portal;

            if (a._distanceToCamera < b._distanceToCamera)
                return -1;
            if (a._distanceToCamera > b._distanceToCamera)
                return 1;
            return 0;
        }
    }
}
