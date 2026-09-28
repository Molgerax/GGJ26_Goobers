using GGJ.Utility;
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
        public ScaledPose Transform { get; }
    }

    public struct TeleportData
    {
        public Vector3 RelativePosition;
        public Quaternion RelativeRotation;
    }
}