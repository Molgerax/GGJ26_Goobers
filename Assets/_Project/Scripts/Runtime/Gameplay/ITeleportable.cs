using GGJ.Mapping.PointEntities;
using UnityEngine;

namespace GGJ.Gameplay
{
    public interface ITeleportable
    {
        public void Teleport(InfoTeleportDestination destination, TeleportData data);
    }

    public struct TeleportData
    {
        public Vector3 RelativePosition;
        public Quaternion RelativeRotation;
    }
}