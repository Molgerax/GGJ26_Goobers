using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("point", category:"light")]
    public class TrembleLight : MonoBehaviour, IOnImportFromMapEntity
    {
        [Tremble("strength")] private float _strength = 1f;
        [Tremble("range")] private float _range = 32f;
        [Tremble("color")] private Color _color = Color.white;

        [Tremble, SpawnFlags] private bool _bakedOnly;
        
        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            Light l = gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = _color;
            l.intensity = _strength;
            l.range = _range * mapBsp.ImportScale;
            l.shadows = LightShadows.Soft;
            
#if UNITY_EDITOR
            l.lightmapBakeType = _bakedOnly ? LightmapBakeType.Baked : LightmapBakeType.Mixed;
#endif
        }
    }
}