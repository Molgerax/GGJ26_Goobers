using GGJ.Gameplay.Faces;
using GGJ.Utility.Extensions;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("once", "trigger", BrushType.Trigger)]
    public class TriggerOnce : TriggerSender
    {
        [SerializeField] private LayerMask layerMask = 64;
        
        [SerializeField] private Expression requireExpression = Expression.Neutral;
        [SerializeField, Range(0, 1)] private float expressionAmount = 0;
        
        private bool _triggered = false;
        
        private void OnTriggerEnter(Collider other)
        {
            if (_triggered)
                return;
            
            if (!layerMask.Contains(other))
                return;

            if (expressionAmount > 0)
            {
                if (other.TryGetComponent(out PlayerFace face))
                {
                    if (!face.HasExpressionPercentage(requireExpression, expressionAmount))
                        return;
                }
            }
            
            _triggered = true;

            SendTrigger();
        }

        public override void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            base.OnImportFromMapEntity(mapBsp, entity);
            layerMask = LayerMask.GetMask("Player");
        }
    }
}
