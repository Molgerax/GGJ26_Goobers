using System.Collections.Generic;
using GGJ.Gameplay.Movement;
using GGJ.Mapping.Tremble.Properties;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.BrushEntities
{
    [BrushEntity("door", category:"func")]
    public class FuncDoor : MonoBehaviour, ITriggerTarget, IMover, IOnImportFromMapEntity
    {
        [SerializeField, NoTremble] private float speed = 5f;
        [SerializeField, NoTremble] private float distance = 16;
        [SerializeField, NoTremble] private bool toggle;
        [SerializeField, NoTremble] private float wait;
        
        [SerializeField, NoTremble] private Vector3 moveDirection;
        
        [Tremble("lip")] private float _trembleLip = 0;
        [Tremble("speed")] private float _trembleSpeed = 64;
        [Tremble("angle")] private QuakeAngle _angle;
        [Tremble("wait")] private float _wait = 3;

        [Tremble("toggle"), SpawnFlags()] private bool _toggle;
        
        private HashSet<IMovable> _attachedMovables = new();
        
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
            Retract = 3,
            Waiting = 4,
        }

        public void Trigger(TriggerData data)
        {
            if (_state == DoorState.Idle)
            {
                _timer = 0;
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

            Vector3 startPos = transform.position;
            
            if (_timer >= Duration)
            {
                _timer = 0;
                transform.position = _targetPos;
                _state = wait < 0 ? DoorState.Finished : DoorState.Waiting;
            }
            else
            {
                transform.position = Vector3.Lerp(_initPos, _targetPos, _timer / Duration);
            }
            
            MoveAttachedObjects(transform.position - startPos);
        }
        
        
        private void TickUnPressed(float deltaTime)
        {
            _timer += deltaTime;
            
            Vector3 startPos = transform.position;

            if (_timer >= Duration)
            {
                _timer = 0;
                transform.position = _initPos;
                _state = DoorState.Idle;
            }
            else
            {
                transform.position = Vector3.Lerp(_targetPos, _initPos, _timer / Duration);
            }
            
            MoveAttachedObjects(transform.position - startPos);
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
        

        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            Vector3 direction = _angle;
            
            Bounds bounds = GetComponent<MeshCollider>().bounds;
            Vector3 positiveDirection = new(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
            distance = Vector3.Dot(positiveDirection, bounds.size) - _trembleLip * entity.ImportScale;

            speed = (_trembleSpeed * entity.ImportScale);

            moveDirection = _angle;
            wait = _wait;
        }
        
        private void MoveAttachedObjects(Vector3 movement)
        {
            foreach (IMovable movable in _attachedMovables)
            {
                movable.Move(movement);
            }
        }
        
        public bool AddMovable(IMovable movable)
        {
            return _attachedMovables.Add(movable);
        }
        
        public bool RemoveMovable(IMovable movable)
        {
            return _attachedMovables.Remove(movable);
        }
    }
}