using UltEvents;
using UnityEngine;

namespace GGJ.Gameplay.Player
{
    public class PlayerAnimationCallbacks : MonoBehaviour
    {
        [SerializeField] private UltEvent onGrab;
        [SerializeField] private UltEvent onApply;


        public void GrabEvent()
        {
            onGrab?.Invoke();
        }

        public void ApplyEvent()
        {
            onApply?.Invoke();
        }
    }
}