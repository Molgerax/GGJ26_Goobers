using GGJ.Gameplay.Player;
using GGJ.Mapping.Tremble.Properties;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("button", category:"func")]
    public class FuncButton : TriggerSender
    {
        [SerializeField, NoTremble] private float speed = 5f;
        [SerializeField, NoTremble] private float distance = 16;
        [SerializeField, NoTremble] private float wait = -1;
        
        [SerializeField, NoTremble] private Vector3 moveDirection;
        
        [Tremble("lip")] private float _trembleLip = 0;
        [Tremble("speed")] private float _trembleSpeed = 64;
        [Tremble("angle")] private QuakeAngle _angle;

        [Tremble("wait")] private float _wait = -1;

        private float _timer;
        private float Duration => speed > 0 ? distance / speed : 0;

        private bool _triggered;

        private Vector3 _initPos;
        private Vector3 _targetPos;

        private DoorState _state = DoorState.Idle;
        
        private void Awake()
        {
            _state = DoorState.Idle;
            _initPos = transform.position;
            _targetPos = _initPos + moveDirection * distance;
        }

        private enum DoorState
        {
            Idle = 0,
            Triggered = 1,
            Finished = 2,
            Waiting = 3,
            Retract = 4,
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_state != DoorState.Idle)
                return;
            
            if (other.gameObject.TryGetComponent(out PlayerController player))
            {
                _state = DoorState.Triggered;
            }
        }

        private void Update()
        {
            if (_state == DoorState.Triggered)
                TickPressed(Time.deltaTime);
            else if (_state == DoorState.Retract)
                TickUnPressed(Time.deltaTime);
            else if (_state == DoorState.Waiting)
                TickWait(Time.deltaTime);
        }

        private void TickPressed(float deltaTime)
        {
            _timer += deltaTime;

            if (_timer >= Duration)
            {
                _timer = 0;
                transform.position = _targetPos;
                _state = wait < 0 ? DoorState.Finished : DoorState.Waiting;
                
                SendTrigger();
                
                return;
            }
            
            transform.position = Vector3.Lerp(_initPos, _targetPos, _timer / Duration);
        }
        
        
        private void TickUnPressed(float deltaTime)
        {
            _timer += deltaTime;

            if (_timer >= Duration)
            {
                _timer = 0;
                transform.position = _initPos;
                _state = DoorState.Idle;
                return;
            }
            
            transform.position = Vector3.Lerp(_targetPos, _initPos, _timer / Duration);
        }

        private void TickWait(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= wait)
            {
                _timer = 0;
                _state = DoorState.Retract;
            }
        }
        
        public override void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            base.OnImportFromMapEntity(mapBsp, entity);
            Vector3 direction = _angle;
            
            Bounds bounds = GetComponent<MeshCollider>().bounds;
            Vector3 positiveDirection = new(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
            distance = Vector3.Dot(positiveDirection, bounds.size) - _trembleLip * entity.ImportScale;

            speed = (_trembleSpeed * entity.ImportScale);

            moveDirection = _angle;
            wait = _wait;
        }
    }
}