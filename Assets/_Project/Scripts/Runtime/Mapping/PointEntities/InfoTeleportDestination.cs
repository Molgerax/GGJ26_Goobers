using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("teleport_destination", category:"info", prefab:"Info_Arrow", colour: "0 0.5 0.75")]
    public class InfoTeleportDestination : MonoBehaviour, IOnImportFromMapEntity
    {
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            transform.rotation = Quaternion.LookRotation(transform.right, transform.up);
        }
    }
}