using GGJ.Gameplay.Movement;
using GGJ.Utility.Extensions;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("teleport_destination", category:"info", prefab:"Info_Arrow", colour: "0 0.5 0.75")]
    public class InfoTeleportDestination : MonoBehaviour, IOnImportFromMapEntity, ITeleportDestination
    {
        [SerializeField, Tremble("Relative Rotation"), SpawnFlags]
        private bool useRelativeRotation = false;
        
        
        [SerializeField, Tremble("Relative Position"), SpawnFlags]
        private bool useRelativePosition = false;

        public bool UseRelativeRotation => useRelativeRotation;
        public bool UseRelativePosition => useRelativePosition;
        public Pose Transform => transform.ToPose();

        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            transform.rotation = Quaternion.LookRotation(transform.right, transform.up);
        }
    }
}