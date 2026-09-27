using GGJ.Rendering.Portals;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.Tremble.MapProcessors
{
    public class LightEnforcerMapProcessor : MapProcessorBase
    {
        public override void OnProcessingCompleted(GameObject root, MapBsp mapBsp)
        {
            var allLights = root.GetComponentsInChildren<Light>();
            foreach (Light light in allLights)
            {
                light.gameObject.AddComponent<LightEnforcer>();
            }
        }
    }
}