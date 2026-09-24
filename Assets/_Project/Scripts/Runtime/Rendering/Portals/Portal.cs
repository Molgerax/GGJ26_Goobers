using System;
using System.Collections.Generic;
using GGJ.Gameplay.Movement;
using GGJ.Mapping.BrushEntities;
using GGJ.Utility;
using GGJ.Utility.Extensions;
using UnityEngine;

namespace GGJ.Rendering.Portals
{
    [ExecuteInEditMode]
    public class Portal : MonoBehaviour, IComparable, ITeleportDestination
    {
        public static readonly List<Portal> ActivePortals = new();
        
        [SerializeField] private Vector2 size;
        
        [SerializeField] private Portal otherPortal;
        [SerializeField, Range(0, 10)] private int iteration = 0;

        [SerializeField] private TriggerSeamlessTeleport otherPortalTremble;

        [SerializeField] private float portalDepth = 1f;
        
        public void Setup(TriggerSeamlessTeleport target, Vector2 scale, float depth)
        {
            size = scale;
            otherPortalTremble = target;
            portalDepth = depth;
        }
        
        public float PortalDepth => portalDepth;
        
        private Camera _mainCamera;

        public Vector2 Size => size;
        public Portal OtherPortal => otherPortal ? otherPortal : (otherPortalTremble ? otherPortalTremble.portal : null);

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

        
        private List<Transform> _currentTeleportables = new();
        

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out ITeleportable teleportable))
                return;
            _currentTeleportables.Add(other.transform);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out ITeleportable teleportable))
                return;
            _currentTeleportables.Remove(other.transform);
        }

        private void LateUpdate()
        {
            if (!OtherPortal)
                return;
            
            for (var i = _currentTeleportables.Count - 1; i >= 0; i--)
            {
                var currentTeleportable = _currentTeleportables[i];
                if (transform.IsBehind(currentTeleportable.position))
                {
                    currentTeleportable.TryGetComponent(out ITeleportable t);
                    t.Teleport(OtherPortal, GetTeleportData());
                    _currentTeleportables.RemoveAt(i);
                }
            }
        }

        private TeleportData GetTeleportData()
        {
            Transform t = transform;
            Quaternion localRotation = Quaternion.LookRotation(-t.forward, t.up);
            Vector3 position = t.position;

            TeleportData teleportData = new TeleportData()
            {
                RelativePosition = position,
                RelativeRotation = localRotation,
            };

            return teleportData;
        }

        public bool UseRelativeRotation => true;
        public bool UseRelativePosition => true;
        public Pose Transform => transform.ToPose();

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

            if (!OtherPortal)
                return;

            Gizmos.color = new(1, 0, 0, 0.25f);
            bool overlap = CameraUtility.ScreenBoundsOverlap(bounds, OtherPortal.Bounds, _mainCamera);

            if (overlap)
                Gizmos.color = new(0, 1, 0, 0.25f);
            Gizmos.DrawCube(bounds.center, bounds.size);
            
            
            
            float t = iteration / 10f;
            Gizmos.color = Color.HSVToRGB(t, 1, 1);

            Pose camPose = GetCameraPose(this, OtherPortal, _mainCamera.transform.ToPose(), iteration);
            
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
        
        public static void GetDepthBiasPlanes(Pose outPortalPose, Pose portalCameraPose, Camera mainCamera, out float biasFactor, out int biasUnits)
        {
            Plane p = new Plane(outPortalPose.forward, outPortalPose.position);
            Vector4 clipPlaneWorldSpace = new(p.normal.x, p.normal.y, p.normal.z, p.distance);
            Vector4 clipPlaneCameraSpace = 
                Matrix4x4.Transpose(Matrix4x4.Inverse(portalCameraPose.ToViewMatrix())) 
                * clipPlaneWorldSpace;

            Vector3 cNormal = portalCameraPose.ToViewMatrix().MultiplyVector(outPortalPose.forward).normalized;
            Vector3 cPos = portalCameraPose.ToViewMatrix().MultiplyPoint(outPortalPose.position);
            clipPlaneCameraSpace = new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));
            
            // 2. Compute the exact depth bias factors based on the oblique slope
            // N_x / N_z and N_y / N_z define the slope distortions
            float slopeX = clipPlaneCameraSpace.x / clipPlaneCameraSpace.z;
            float slopeY = clipPlaneCameraSpace.y / clipPlaneCameraSpace.z;
            // Calculate the magnitude of the slope offset factor
            biasFactor = Mathf.Sqrt(slopeX * slopeX + slopeY * slopeY);
            // Units offset handles the depth translation shift
            biasUnits = Mathf.RoundToInt(clipPlaneCameraSpace.w / clipPlaneCameraSpace.z * 2.0f);
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
