using System.Collections.Generic;
using GGJ.Gameplay;
using GGJ.Gameplay.Movement;
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
                TeleportData teleportData = GetTeleportData();
                teleportable.Teleport(destination, teleportData);
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

        private TeleportData GetTeleportData()
        {
            Transform t = transform;
            Quaternion localRotation = Quaternion.LookRotation(t.right, t.up);
            Vector3 position = t.position;

            TeleportData teleportData = new TeleportData()
            {
                RelativePosition = position,
                RelativeRotation = localRotation,
            };

            return teleportData;
        }
        
        public void Trigger(TriggerData data)
        {
            TeleportData teleportData = GetTeleportData();
            foreach (ITeleportable teleportable in _currentTeleportables)
            {
                teleportable.Teleport(destination, teleportData);
            }
        }
        
        
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            autoTrigger = !entity.HasKey("targetname");
        }
    }
}
