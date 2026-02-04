using TinyGoose.Tremble;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("illusionary", "func", BrushType.Liquid)]
    public class FuncIllusionary : MonoBehaviour, IOnImportFromMapEntity
    {
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            if (gameObject.TryGetComponent(out MeshCollider meshCollider))
                CoreUtils.Destroy(meshCollider);

            if (gameObject.TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
