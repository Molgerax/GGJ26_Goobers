using UnityEngine;

namespace GGJ.Gameplay.Movement
{
    public interface IMover
    {
        public bool AddMovable(IMovable movable);
        public bool RemoveMovable(IMovable movable);
    }
}