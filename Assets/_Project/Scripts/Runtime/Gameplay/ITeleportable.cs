using GGJ.Mapping.PointEntities;

namespace GGJ.Gameplay
{
    public interface ITeleportable
    {
        public void Teleport(InfoTeleportDestination destination);
    }
}