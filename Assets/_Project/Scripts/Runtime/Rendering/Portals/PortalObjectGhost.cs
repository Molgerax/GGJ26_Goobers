using System;
using GGJ.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace GGJ.Rendering.Portals
{
    public class PortalObjectGhost : MonoBehaviour
    {
        public static IObjectPool<PortalObjectGhost> RendererPool = new ObjectPool<PortalObjectGhost>(Create, OnGet, OnRelease, OnDestroyObject);
        
        private static PortalObjectGhost Create()
        {
            GameObject go = new GameObject("PortalObjectGhost");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.SetActive(false);
            var ghost = go.AddComponent<PortalObjectGhost>();
            ghost.Initialize();
            return ghost;
        }
        
        private static void OnGet(PortalObjectGhost go)
        {
            go.SetActive(true);
        }
        
        private static void OnRelease(PortalObjectGhost go)
        {
            go.SetActive(false);
        }
        
        private static void OnDestroyObject(PortalObjectGhost go)
        {
            Destroy(go);
        }


        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propertyBlock;

        public void Initialize()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _propertyBlock = new();
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        

        public void SetMeshAndMaterials(MeshFilter filter, MeshRenderer meshRenderer)
        {
            gameObject.layer = filter.gameObject.layer;
            
            _filter.sharedMesh = filter.sharedMesh;
            _renderer.sharedMaterials = meshRenderer.sharedMaterials;

            transform.parent = filter.transform.parent;
            
            _propertyBlock.Clear();
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetTransform(ScaledPose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            transform.localScale = pose.scale;
        }
        
        private void Update()
        {
            RendererPool.Release(this);
        }
    }
}