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
        public struct PortalCorners
        {
            public Vector3 BottomLeft;
            public Vector3 BottomRight;
            public Vector3 TopLeft;
            public Vector3 TopRight;
        }

        private static readonly List<Portal> _activePortals = new();
        public static IReadOnlyList<Portal> ActivePortals => _activePortals;

        private static void AddPortal(Portal portal)
        {
            if (!portal._isAdded)
            {
                _activePortals.Add(portal);
                portal._isAdded = true;
            }
            else
            {
                Debug.LogError($"Portal {portal} was already added to {nameof(_activePortals)}!");
            }
        }
        
        private static void RemovePortal(Portal portal)
        {
            if (portal._isAdded)
            {
                _activePortals.Remove(portal);
                portal._isAdded = false;
            }
            else
            {
                Debug.LogError($"Portal {portal} was not part of {nameof(_activePortals)}!");
            }
        }
        
        
        [SerializeField] private Vector2 size;
        
        [SerializeField] private Portal otherPortal;
        [SerializeField, Range(0, 10)] private int iteration = 0;

        [SerializeField] private TriggerSeamlessTeleport otherPortalTremble;

        [SerializeField] private float portalDepth = 1f;

        [SerializeField] private bool passable;
        [SerializeField] private bool mirror;

        [SerializeField] private bool active = true;
        private bool _isAdded;
        
        public bool Active
        {
            get => active && isActiveAndEnabled;
            set
            {
                active = value;
                if (active && isActiveAndEnabled && !_isAdded)
                    AddPortal(this);
                if (!active && isActiveAndEnabled && _isAdded)
                    RemovePortal(this);
            }
        }

        public string ToName()
        {
            return System.Text.RegularExpressions.Regex.Match(this.name, "\'(.*)\'").Value;
        }
        
        public void Setup(TriggerSeamlessTeleport target, Vector2 scale, float depth, bool canPassThrough, bool doesMirror)
        {
            size = scale;
            otherPortalTremble = target;
            portalDepth = depth;
            passable = canPassThrough;
            mirror = doesMirror;
        }
        
        public float PortalDepth => portalDepth;
        
        private Camera _mainCamera;

        public Vector2 Size => size;
        public Portal OtherPortal => otherPortal ? otherPortal : (otherPortalTremble ? otherPortalTremble.portal : null);

        public Vector3 MultiplyVector(Vector3 vector)
        {
            if (!OtherPortal)
                return vector;

            vector = transform.InverseTransformVector(vector);
            vector.z *= -1f;
            if (!OtherPortal.Mirror)
                vector.x *= -1;
            
            return OtherPortal.transform.TransformVector(vector);
        }
        
        public bool Mirror => mirror;

        public bool Passable => passable && Active;
        
        private float _distanceToCamera;


        public PortalCorners GetCorners()
        {
            ScaledPose pose = Transform;
            Vector3 right = pose.rotation * Vector3.right;
            Vector3 up = pose.rotation * Vector3.up;
            var corners = new PortalCorners()
            {
                BottomLeft = pose.position - right * size.x * 0.5f - up * size.y * 0.5f,
                BottomRight = pose.position + right * size.x * 0.5f - up * size.y * 0.5f,
                TopLeft = pose.position - right * size.x * 0.5f + up * size.y * 0.5f,
                TopRight = pose.position + right * size.x * 0.5f + up * size.y * 0.5f,
            };
            return corners;
        }
        

        public void UpdateDistanceToCamera(Camera cam)
        {
            _distanceToCamera = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
        }

        public Bounds Bounds
        {
            get
            {
                ScaledPose pose = transform.ToPose();
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
            if (Active)
                AddPortal(this);
        }

        private void OnDisable()
        {
            if (_isAdded)   
                RemovePortal(this);
        }

        public TeleportData GetTeleportData()
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
        public ScaledPose Transform => transform.ToPose();

        private void OnDrawGizmos()
        {
            return;
            
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

            ScaledPose camPose = GetCameraPose(this, OtherPortal, _mainCamera.transform.ToPose(), iteration);
            
            Gizmos.matrix = Matrix4x4.TRS(camPose.position, camPose.rotation, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, 1f);
            Gizmos.DrawRay(Vector3.zero, Vector3.forward * 2f);
        }


        private void OnDrawGizmosSelected()
        {
            if (PortalRenderFeature.ClippingPlanes == null)
                return;
            
            return;
            
            Gizmos.color = Color.red;
            DrawPlaneIntersections(out var center);
            
            for (int i = 0; i < 6; i++)
            {
                float t = i / 6f;
                var col = Color.HSVToRGB(t, 1, 1);
                col.a = 0.25f;
                Gizmos.color = col;
                
                Plane p = PortalRenderFeature.ClippingPlanes[i];
                DrawPlane(p, center, 50);
            }
        }

        public void DrawPlane(Plane p, Vector3 center, float width, int count = 16)
        {
            Vector3 closest = p.ClosestPointOnPlane(center);
            Vector3 pos = -p.normal * p.distance;
            Gizmos.matrix = Matrix4x4.TRS(pos, Quaternion.LookRotation(p.normal), Vector3.one);

            
            for (int x = 0; x < count; x++)
            {
                float t = x / (count - 1f);
                t = 2 * t - 1;
                Gizmos.DrawLine(
                    new Vector3(-width, t * width, 0),
                    new Vector3(+width, t * width, 0));
                
                Gizmos.DrawLine(
                    new Vector3(t * width, -width, 0),
                    new Vector3(t * width, +width, 0));
            }
        }

        public static Vector3[] CornerList = new Vector3[16];
        
        public void DrawPlaneIntersections(out Vector3 frustumMiddle)
        {
            Gizmos.matrix = Matrix4x4.identity;
            int i = 0;
         
            frustumMiddle = Vector3.zero;
            
            for (int a = 0; a < 6; a++)
            {
                for (int b = 0; b < 6; b++)
                {
                    for (int c = 0; c < 6; c++)
                    {
                        if (a == b || a == c || b == c)
                            continue;
                        
                        if (planesIntersectAtSinglePoint(
                                PortalRenderFeature.ClippingPlanes[a], 
                                PortalRenderFeature.ClippingPlanes[b], 
                                PortalRenderFeature.ClippingPlanes[c], out Vector3 point))
                        {
                            
                            if (i < 16)
                                CornerList[i++] = point;
                            Gizmos.DrawSphere(point, 0.1f);
                        }
                    }
                }
            }

            if (i == 0)
                return;
            
            for (int index = 0; index < i; index++)
            {
                frustumMiddle += CornerList[index];
            }

            frustumMiddle /= i;
            
            for (int j = 0; j < i; j++)
            {
                for (int k = 0; k < i; k++)
                {
                    if (j == k)
                        continue;
                    Gizmos.DrawLine(CornerList[j], CornerList[k]);
                }
            }
        }
        
        private bool planesIntersectAtSinglePoint( Plane p0, Plane p1, Plane p2, out Vector3 intersectionPoint )
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


        public static ScaledPose GetCameraPose(Portal inPortal, Portal outPortal, ScaledPose cameraPose, int iterationID = 0)
        {
            ScaledPose inPose = inPortal.transform.ToPose();
            ScaledPose outPose = outPortal.transform.ToPose();
            
            for (int i = 0; i <= iterationID; i++)
            {
                Vector3 relativePos = inPose.InverseTransformPoint(cameraPose.position);
                relativePos = Quaternion.Euler(0, 180, 0) * relativePos;

                if (outPortal.Mirror)
                    relativePos.x *= -1;
                
                cameraPose.position = outPose.TransformPoint(relativePos);

                Quaternion relativeRot = Quaternion.Inverse(inPose.rotation) * cameraPose.rotation;
                relativeRot = Quaternion.Euler(0, 180, 0) * relativeRot;
                
                if (outPortal.Mirror)
                {
                    relativeRot.y *= -1;
                    relativeRot.z *= -1;
                } 
                
                cameraPose.rotation = outPose.rotation * relativeRot;
            }

            if (outPortal.mirror)
            {
                cameraPose.scale.x *= -1;
            }
            
            return cameraPose;
        }

        public static Matrix4x4 GetProjectionMatrix(ScaledPose outPortalPose, ScaledPose portalCameraPose, Camera mainCamera)
        {
            Plane p = new Plane(outPortalPose.forward, outPortalPose.position);
            Vector4 clipPlaneWorldSpace = new(p.normal.x, p.normal.y, p.normal.z, p.distance);
            Vector4 clipPlaneCameraSpace = 
                Matrix4x4.Transpose(Matrix4x4.Inverse(portalCameraPose.ToViewMatrix())) 
                * clipPlaneWorldSpace;

            return mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        
        public static void GetDepthBiasPlanes(ScaledPose outPortalPose, ScaledPose portalCameraPose, Camera mainCamera, out float biasFactor, out int biasUnits)
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

        public Plane GetPlane()
        {
            return new(transform.forward, transform.position);
        }
    }
}
