using TinyGoose.Tremble;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace GGJ.Mapping.Tremble.MapProcessors
{
    public class NavMeshMapProcessor : MapProcessorBase
    {
        public override void OnProcessingCompleted(GameObject root, MapBsp mapBsp)
        {
            Worldspawn spawn = root.GetComponentInChildren<Worldspawn>();
            
            NavMeshSurface nav = spawn.gameObject.AddComponent<NavMeshSurface>();
            nav.collectObjects = CollectObjects.Children;
            nav.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            spawn.gameObject.AddComponent<NavMeshTriggerBake>();
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += DelayedCall;
#endif
        }

        private void DelayedCall()
        {
            var worldSpawn = Object.FindAnyObjectByType<Worldspawn>();
            
            if (!worldSpawn)
                return;
            
            if (worldSpawn.TryGetComponent(out NavMeshSurface surface))
                surface.BuildNavMesh();
        }
    }
}
