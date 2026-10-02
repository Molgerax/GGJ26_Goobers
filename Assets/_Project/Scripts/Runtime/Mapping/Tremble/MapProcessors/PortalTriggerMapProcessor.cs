using GGJ.Mapping.BrushEntities;
using GGJ.Rendering.Portals;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.Tremble.MapProcessors
{
    public class PortalTriggerMapProcessor : MapProcessorBase
    {
        public override void OnProcessingCompleted(GameObject root, MapBsp mapBsp)
        {
            var triggerSenders = root.GetComponentsInChildren<TriggerSender>();

            foreach (TriggerSender sender in triggerSenders)
            {
                if (!sender || sender.Targets == null)
                    continue;
                
                foreach (Component target in sender.Targets)
                {
                    if (target.TryGetComponent(out TriggerSeamlessTeleport teleport))
                    {
                        teleport.Active = false;
                    }
                }
            }
        }
    }
}