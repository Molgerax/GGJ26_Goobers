using GGJ.SceneManagement;
using GGJ.Utility.Extensions;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("scene_loader", "trigger", TrembleColors.TrembleSceneLoader, size:16)]
    public class TrembleSceneLoader : MonoBehaviour, ITriggerTarget, IOnImportFromMapEntity
    {
        [SerializeField, Tremble("scene")] private SceneLoadCollection scene;

        [SerializeField, NoTremble] private SceneLoadPoster sceneLoadPoster;

        private bool _triggered;
        
        public void Trigger()
        {
            if (_triggered)
                return;
            _triggered = true;
            
            sceneLoadPoster.LoadScene();
        }

        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            sceneLoadPoster = gameObject.GetOrAddComponent<SceneLoadPoster>();
            sceneLoadPoster.SetCollection(scene);
            sceneLoadPoster.SetChannel();
        }
    }
}
