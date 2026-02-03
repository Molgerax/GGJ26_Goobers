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

        [SerializeField] private LayerMask alienFaceMask;

        [SerializeField] private Animator animator;
        
        private GrabbedFacePart _currentGrabbedFace;
        
        private Camera _camera;

        private GrabbedFacePart _visualGrabbedFace;

        private Face _cachedGrabbedFace;
        
        private void OnEnable()
        {
            PlayerInput.Input.Player.Attack.performed += OnAttack;

            texture = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            texture.name = "GrabberTexture";
            texture.enableRandomWrite = true;
            texture.Create();
            
            GrabTextureIntoRenderTexture.ClearTexture(transferCompute, texture);

            _camera = Camera.main;
        }

        private void OnDisable()
        {
            PlayerInput.Input.Player.Attack.performed -= OnAttack;
            texture.Release();
            texture = null;
        }

        private void Update()
        {
            if (_currentGrabbedFace.Radius != 0)
                SetHighlight();
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
                if (hitInfo.transform.TryGetComponent(out Face grabbedFace))
                {
                    Vector2 uv = hitInfo.textureCoord;
                    GrabTextureIntoRenderTexture.ClearTexture(transferCompute, texture);
                    GrabTextureIntoRenderTexture.TransferTexture(transferCompute, uv, grabRadius, grabbedFace.FaceTexture.Texture, texture);

                    _cachedGrabbedFace = grabbedFace;
                    //grabbedFace.RemoveFromFace(uv, grabRadius);
                    
                    SetFaceOffOnMesh(faceTest);

                    _currentGrabbedFace = new GrabbedFacePart(grabbedFace.FaceTexture.Expression, uv, grabRadius);
                    _visualGrabbedFace = _currentGrabbedFace;
                    
                    if (animator)
                        animator.SetTrigger("Grab");
                    
                    PlayerInput.SetCursorLocked(false);
                    PlayerInput.SetMoveInputs(false);
                }
            }
        }

        private void ApplyFace()
        {
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hitInfo, grabDistance, alienFaceMask,
                QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider is MeshCollider)
                {
                    Vector2 uv = hitInfo.textureCoord;

                    ApplyFaceToFace(uv);
                }
            }
        }

        private void ApplyFaceToFace(Vector2 uv)
        {
            face.ApplyFacePart(texture, uv, _currentGrabbedFace);
            
            _currentGrabbedFace = default;

            if (animator)
                animator.SetTrigger("GrabApply");
            
            face.SetShaderHighlight(0, Vector2.zero);
            PlayerInput.SetCursorLocked(true);
            PlayerInput.SetMoveInputs(true);
        }

        public void RemoveFromFaceDelayed()
        {
            if (!_cachedGrabbedFace || _visualGrabbedFace.Radius == 0)
                return;
            
            _cachedGrabbedFace.RemoveFromFace(_visualGrabbedFace.UV, _visualGrabbedFace.Radius);
            _visualGrabbedFace = default;
        }

        private void SetHighlight()
        {
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000, alienFaceMask,
                QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider is MeshCollider)
                {
                       Vector2 uv = hitInfo.textureCoord;
                       face.SetShaderHighlight(grabRadius, uv);
                }
                else
                { 
                    face.SetShaderHighlight(0, Vector2.zero);   
                }
            }
            else
            {
                face.SetShaderHighlight(0, Vector2.zero);
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