using TinyGoose.Tremble;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("liquid", "func", BrushType.Liquid)]
    public class FuncLiquid : MonoBehaviour, IOnImportFromMapEntity
    {
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            if (gameObject.TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
