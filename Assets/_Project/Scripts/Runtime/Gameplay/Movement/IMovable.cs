using UnityEngine;

namespace GGJ.Gameplay.Movement
{
    public interface IMovable
    {
        public void Move(Vector3 displacement);
    }
}