using TinyGoose.Tremble;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("kill", category:"func")]
    public class TrembleKill : MonoBehaviour, ITriggerTarget, IOnImportFromMapEntity
    {
        public void Trigger(TriggerData data)
        {
            CoreUtils.Destroy(gameObject);
        }

        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
        }
    }
}