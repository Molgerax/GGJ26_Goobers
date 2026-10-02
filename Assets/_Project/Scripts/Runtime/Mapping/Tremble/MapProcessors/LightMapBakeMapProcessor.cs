using GGJ.Rendering.Portals;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.Tremble.MapProcessors
{
    public class LightMapBakeMapProcessor : MapProcessorBase
    {
        public override void OnProcessingCompleted(GameObject root, MapBsp mapBsp)
        {
#if UNITY_EDITOR
            UnityEditor.Lightmapping.BakeAsync();
#endif
        }
    }
}