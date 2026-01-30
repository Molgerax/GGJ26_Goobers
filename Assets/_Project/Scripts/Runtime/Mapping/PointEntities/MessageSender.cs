using System.Collections.Generic;
using System.Linq;
using GGJ.Gameplay.Messages;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("message", "misc", TrembleColors.MessageSender, size:16)]
    public class MessageSender : TriggerSender, ITriggerTarget
    {
        [SerializeField, Tremble] private string message = "A secret has been discovered";
        [SerializeField, Tremble] private float time = -1;

        [SerializeField, NoTremble] private List<Component> targetList;
        
        public void Trigger()
        {
            Message m = new Message(message, time, targetList);
            MessageManager.AddMessage(m);
        }

        public override void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            base.OnImportFromMapEntity(mapBsp, entity);
            targetList = targets?.ToList();
        }
    }
}
