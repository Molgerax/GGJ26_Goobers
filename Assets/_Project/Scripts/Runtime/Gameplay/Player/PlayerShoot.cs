using System;
using GGJ.Gameplay.Faces;
using GGJ.Rendering.Portals;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = GGJ.Inputs.PlayerInput;

namespace GGJ.Gameplay.Player
{
    public class PlayerShoot : MonoBehaviour
    {
        [SerializeField] private int shootDistance = 5;
        [SerializeField] private Transform lookTransform;
        
        private GrabbedFacePart _visualGrabbedFace;

        private void OnEnable()
        {
            PlayerInput.Input.Player.Attack.performed += OnAttack;
        }

        private void OnDisable()
        {
            PlayerInput.Input.Player.Attack.performed -= OnAttack;
        }
        

        private void OnAttack(InputAction.CallbackContext context)
        {
            Shoot();
        }

        private void Shoot()
        {
            Ray ray = new(lookTransform.position, lookTransform.forward);

            if (PortalUtility.Raycast(ray, out var hit, shootDistance))
            {
                
            }
        }

        private void OnDrawGizmos()
        {
            if (!lookTransform)
                return;
            
            
            Ray ray = new(lookTransform.position, lookTransform.forward);
            if (PortalUtility.Raycast(ray, out var hit, shootDistance, triggerInteraction: QueryTriggerInteraction.Collide))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.2f);
            }
        }
    }
}