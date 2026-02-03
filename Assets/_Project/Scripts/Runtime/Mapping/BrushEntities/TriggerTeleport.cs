using System.Collections.Generic;
using GGJ.Gameplay;
using GGJ.Mapping.PointEntities;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("teleport", "trigger", BrushType.Trigger)]
    public class TriggerTeleport : MonoBehaviour, IOnImportFromMapEntity, ITriggerTarget
    {
        [SerializeField, Tremble("target")] private InfoTeleportDestination destination;

        [SerializeField, NoTremble] private bool autoTrigger = true;

        
        private HashSet<ITeleportable> _currentTeleportables = new();
        

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out ITeleportable teleportable))
                return;
            
            if (autoTrigger)
            {
                teleportable.Teleport(destination);
                return;
            }
            
            _currentTeleportables.Add(teleportable);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out ITeleportable teleportable))
                return;

            
            if (autoTrigger)
                return;
            
            _currentTeleportables.Remove(teleportable);
        }

        
        public void Trigger()
        {
            foreach (ITeleportable teleportable in _currentTeleportables)
            {
                teleportable.Teleport(destination);
            }
        }
        
        
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            autoTrigger = !entity.HasKey("targetname");
        }
    }
}
