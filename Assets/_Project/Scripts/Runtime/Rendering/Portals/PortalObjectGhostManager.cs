using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace GGJ.Rendering.Portals
{
    public class PortalObjectGhostManager : MonoBehaviour
    {
        public static PortalObjectGhostManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<PortalObjectGhostManager>();
                if (_instance == null)
                {
                    _instance =
                        new GameObject(nameof(PortalObjectGhostManager)).AddComponent<PortalObjectGhostManager>();
                }

                return _instance;
            }
        }

        private static PortalObjectGhostManager _instance;

        public static IObjectPool<PortalObjectGhost> Pool => Instance._pool;
        
        private IObjectPool<PortalObjectGhost> _pool = new UnityEngine.Pool.ObjectPool<PortalObjectGhost>(Create, OnGet, OnRelease, OnDestroyObject);
        
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
            go.transform.parent = Instance.transform;
        }
        
        private static void OnDestroyObject(PortalObjectGhost go)
        {
            CoreUtils.Destroy(go);
        }
    }
}