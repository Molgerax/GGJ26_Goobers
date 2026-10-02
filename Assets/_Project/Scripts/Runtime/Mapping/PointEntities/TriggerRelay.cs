using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.PointEntities
{
    [PointEntity("relay", "trigger")]
    public class TriggerRelay : TriggerSender, ITriggerTarget
    {
        [SerializeField] private float delay = 0f;

        private float _timer;
        private bool _isRunning;
        private bool _triggered;

        private TriggerData _data;
        
        public void Trigger(TriggerData data)
        {
            _isRunning = true;
            _timer = 0f;

            _data = data;
        }

        private void Update()
        {
            if (!_isRunning)
                return;

            _timer += Time.deltaTime;
            
            if (_timer >= delay)
                OnTimerOver();
        }

        public void OnTimerOver()
        {
            _isRunning = false;
            
            SendTrigger(_data);
        }
    }
}
