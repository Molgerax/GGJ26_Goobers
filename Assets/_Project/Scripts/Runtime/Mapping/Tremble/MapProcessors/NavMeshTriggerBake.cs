using Unity.AI.Navigation;
using UnityEngine;

namespace GGJ.Mapping.Tremble.MapProcessors
{
    [ExecuteInEditMode]
    public class NavMeshTriggerBake : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Awake()
        {
            if (gameObject.TryGetComponent(out NavMeshSurface surface))
                surface.BuildNavMesh();
        }
#endif
    }
}