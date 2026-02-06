using System.Collections.Generic;
using GGJ.Gameplay.Movement;
using GGJ.Mapping.PointEntities;
using GGJ.Utility.Extensions;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Mapping.BrushEntities
{
    //[PointEntity("platform", category:"func", colour:"0 0.5 1.0", size: 16)]
    [BrushEntity("platform", category:"func", type: BrushType.Solid)]
    public class TrembleMovingPlatform : MonoBehaviour, ITriggerTarget, IMover, IOnImportFromMapEntity
    {
        [SerializeField, NoTremble] private float speed = 5f;

        [SerializeField, Tremble("target")] private Waypoint waypoint;
        [Tremble("speed")] private float _trembleSpeed = 64;

        [SerializeField] private bool autoStart;

        private HashSet<IMovable> _attachedMovables = new();
        
        private bool _triggered;

        private Vector3 _initPos;
        private Vector3 _targetPos;

        private DoorState _state;

        private Waypoint _nextWaypoint;

        private Vector3 _offset;
        
        private void Awake()
        {
            if (autoStart)
                Trigger();
        }

        private enum DoorState
        {
            Idle = 0,
            Triggered = 1,
            Finished = 2,
            Stopped = 3,
        }

        public void Trigger(TriggerData data = default)
        {
            if (_state == DoorState.Idle)
            {
                _nextWaypoint = waypoint;
                if (!_nextWaypoint)
                {
                    _state = DoorState.Finished;
                    return;
                }

                _offset = _nextWaypoint.transform.position - transform.position;
                _state = DoorState.Triggered;
            }

            if (_state == DoorState.Stopped)
            {
                _state = DoorState.Triggered;
            }
        }

        private void Update()
        {
            if (_state != DoorState.Triggered)
                return;
            
            if (!_nextWaypoint)
                return;

            Vector3 posA = _offset + transform.position;
            Vector3 posB = _nextWaypoint.transform.position;

            Vector3 toNextWaypoint = posB - posA;
            Vector3 movement = toNextWaypoint.normalized * (Time.deltaTime * speed);

            if (movement.magnitude >= toNextWaypoint.magnitude || movement.magnitude == 0f)
            {
                OnReachWaypoint(_nextWaypoint);
                movement = posA - posB;
                posA = posB;
            }
            else
            {
                posA += movement;
            }
            
            transform.position = posA - _offset;
            
            MoveAttachedObjects(movement);
        }

        private void MoveAttachedObjects(Vector3 movement)
        {
            foreach (IMovable movable in _attachedMovables)
            {
                movable.Move(movement);
            }
        }

        private void OnReachWaypoint(Waypoint wp)
        {
            Waypoint newWp = wp.GetNextWaypoint();

            if (_nextWaypoint && _nextWaypoint.IsStop)
                _state = DoorState.Stopped;
            
            _nextWaypoint = newWp;
            if (!_nextWaypoint)
                _state = DoorState.Finished;
        }

        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            speed = (_trembleSpeed * TrembleSyncSettings.Get().ImportScale);

            Rigidbody rb = gameObject.GetOrAddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.isKinematic = true;
            rb.useGravity = false;
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