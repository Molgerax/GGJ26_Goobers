using System;
using GGJ.Gameplay.Faces;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = GGJ.Inputs.PlayerInput;

namespace GGJ.Gameplay.Player
{
    public class PlayerGrabFace : MonoBehaviour
    {
        [SerializeField] private ComputeShader transferCompute;
        
        [SerializeField] private int grabDistance = 5;
        [SerializeField] private int grabRadius = 32;

        [SerializeField] private Transform lookTransform;

        [SerializeField] private MeshRenderer faceTest;

        [SerializeField] private RenderTexture texture;


        private void OnEnable()
        {
            PlayerInput.Input.Player.Attack.performed += OnAttack;

            texture = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            texture.name = "GrabberTexture";
            texture.enableRandomWrite = true;
            texture.Create();
        }

        private void OnDisable()
        {
            PlayerInput.Input.Player.Attack.performed -= OnAttack;
            texture.Release();
            texture = null;
        }
        
        private void OnAttack(InputAction.CallbackContext context)
        {
            Ray ray = new(lookTransform.position, lookTransform.forward);
            ray.origin = ray.GetPoint(0.5f);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, grabDistance, Int32.MaxValue,
                QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.transform.TryGetComponent(out Face face))
                {
                    Vector2 uv = hitInfo.textureCoord;
                    GrabTextureIntoRenderTexture.TransferTexture(transferCompute, uv, grabRadius, face.FaceTexture.Texture, texture);
                    face.RemoveFromFace(uv, grabRadius);
                    
                    SetFaceOffOnMesh(faceTest);
                }
            }
        }


        MaterialPropertyBlock _propBlock;
        private void SetFaceOffOnMesh(MeshRenderer meshR)
        {
            _propBlock ??= new();
            
            meshR.GetPropertyBlock(_propBlock);
            _propBlock.SetTexture("_MainTex", texture);
            meshR.SetPropertyBlock(_propBlock);
        }
    }
}