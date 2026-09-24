using GGJ.Mapping.PointEntities;
using UnityEngine;

namespace GGJ.Gameplay.Movement
{
    public interface ITeleportable
    {
        public void Teleport(ITeleportDestination destination, TeleportData data);
    }

    public interface ITeleportDestination
    {
        public bool UseRelativeRotation { get; }
        public bool UseRelativePosition { get; }
        public Pose Transform { get; }
    }

    public struct TeleportData
    {
        public Vector3 RelativePosition;
        public Quaternion RelativeRotation;
    }
}