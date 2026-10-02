using GGJ.Utility;
using TinyGoose.Tremble;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Mapping.BrushEntities
{
    [DefaultExecutionOrder(500)]
    [BrushEntity("looker", "trigger", BrushType.Trigger)]
    public class TriggerLooker : TriggerSender
    {
        [SerializeField] private Bounds bounds;
        private Camera _camera;

        [SerializeField, NoTremble] private bool _activated;

        public bool Activated
        {
            get => _activated;
            set
            {
                if (_activated == value)
                    return;

                _activated = value;
                SendTrigger(new TriggerData(_activated));
            }
        }
        
        
        private void Awake()
        {
            _camera = Camera.main;
        }


        private void LateUpdate()
        {
            if (!_camera)
                return;
            
            Activated = CameraUtility.TryGetScreenRectFromBounds(bounds, ScaledPose.identity,
                _camera.worldToCameraMatrix, _camera.projectionMatrix, out var newScreenBounds);
        }


        public override void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            base.OnImportFromMapEntity(mapBsp, entity);
            MeshCollider mc = GetComponent<MeshCollider>();
            bounds = mc.bounds;
            CoreUtils.Destroy(mc);
        }
    }
}
