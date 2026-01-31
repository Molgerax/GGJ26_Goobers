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
        [SerializeField] private PlayerFace face;
        [SerializeField] private MeshRenderer faceTest;

        [SerializeField] private RenderTexture texture;

        

        private GrabbedFacePart _currentGrabbedFace;
        

        private void OnEnable()
        {
            PlayerInput.Input.Player.Attack.performed += OnAttack;

            texture = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            texture.name = "GrabberTexture";
            texture.enableRandomWrite = true;
            texture.Create();
            
            GrabTextureIntoRenderTexture.ClearTexture(transferCompute, texture);
        }

        private void OnDisable()
        {
            PlayerInput.Input.Player.Attack.performed -= OnAttack;
            texture.Release();
            texture = null;
        }
        
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (_currentGrabbedFace.Radius == 0)
                GrabFace();
            else
                ApplyFace();
        }

        private void GrabFace()
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

                    _currentGrabbedFace = new GrabbedFacePart(face.FaceTexture.Expression, uv, grabRadius);
                }
            }
        }

        private void ApplyFace()
        {
            Vector2 uv = _currentGrabbedFace.UV;
            face.ApplyFacePart(texture, uv, _currentGrabbedFace);
            
            _currentGrabbedFace = default;
            GrabTextureIntoRenderTexture.ClearTexture(transferCompute, texture);
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