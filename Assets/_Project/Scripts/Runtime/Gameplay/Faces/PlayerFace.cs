using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GGJ.Gameplay.Faces
{
    public class PlayerFace : MonoBehaviour
    {
        [SerializeField] private ComputeShader transferCompute;
        [SerializeField] private int resolution = 1024;

        [SerializeField] private int cellResolution = 32;

        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Texture2D debugTex;
        
        private RenderTexture _faceTexture;

        private FaceCell[,] _faceCells;

        private Dictionary<Expression, int> _currentExpressionCount = new();

        private float _deviation;
        MaterialPropertyBlock _propertyBlock;

        public static Action<PlayerFace> OnUpdateFaceValues;

        private void OnEnable()
        {
            _faceTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _faceTexture.name = "GrabberTexture";
            _faceTexture.enableRandomWrite = true;
            _faceTexture.Create();

            _faceCells = new FaceCell[cellResolution, cellResolution];
            debugTex = new Texture2D(cellResolution, cellResolution, TextureFormat.RGBA32, false, true);

            _propertyBlock ??= new();
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture("_MainTex", _faceTexture);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnDisable()
        {
            _faceTexture.Release();
            _faceTexture = null;

            CoreUtils.Destroy(debugTex);
            debugTex = null;
            
            _faceCells = null;
        }


        public void ApplyFacePart(RenderTexture partTexture, Vector2 uv, GrabbedFacePart facePart)
        {
            ApplyToCells(partTexture.height, uv, facePart);
            ApplyToTexture(partTexture, uv, facePart);
        }

        private void ApplyToTexture(RenderTexture partTexture, Vector2 uv, GrabbedFacePart facePart)
        {
            GrabTextureIntoRenderTexture.StackTextureToFace(transferCompute, uv, facePart, partTexture, _faceTexture);
        }

        private void ApplyToCells(int res, Vector2 uv, GrabbedFacePart facePart)
        {
            float deviation = Vector2.Distance(uv, facePart.UV);

            for (int x = 0; x < cellResolution; x++)
            {
                for (int y = 0; y < cellResolution; y++)
                {
                    Vector2 cellUv = (new Vector2(x + 0.5f, y + 0.5f)) / cellResolution;

                    cellUv -= facePart.UV;
                    
                    Vector2 cell = Vector2Int.FloorToInt((cellUv) * res);

                    if (cell.magnitude < facePart.Radius)
                    {
                        _faceCells[x, y] = new(facePart.Expression, deviation);
                    }
                }
            }
            EvaluateExpressions();
        }

        private void LogAllExpressions()
        {
            Dictionary<Expression, int> expressionLog = new();
            
            for (int x = 0; x < cellResolution; x++)
            {
                for (int y = 0; y < cellResolution; y++)
                {
                    FaceCell cell = _faceCells[x, y];

                    Color color = Color.black;
                    int expr = (int)cell.Expression;
                    color = Color.HSVToRGB(expr / 8f, expr > 0 ? 1 : 0, 1);
                    
                    debugTex.SetPixel(x, y, color);
                    
                    if (!expressionLog.TryAdd(cell.Expression, 1))
                        expressionLog[cell.Expression]++;
                }
            }
            
            debugTex.Apply();

            string str = "";
            foreach (KeyValuePair<Expression,int> pair in expressionLog)
            {
                str += ($"{pair.Key}: {pair.Value}\n");
            }
            Debug.Log(str);
        }

        private void EvaluateExpressions()
        {
            _currentExpressionCount.Clear();
            _deviation = 0;
            
            for (int x = 0; x < cellResolution; x++)
            {
                for (int y = 0; y < cellResolution; y++)
                {
                    FaceCell cell = _faceCells[x, y];

                    if (!_currentExpressionCount.TryAdd(cell.Expression, 1))
                        _currentExpressionCount[cell.Expression]++;

                    _deviation += cell.Deviation;
                }
            }

            _deviation /= cellResolution * cellResolution;
            
            OnUpdateFaceValues?.Invoke(this);
        }

        public bool HasExpressionPercentage(Expression expression, float threshold01)
        {
            return GetExpressionPercentage(expression) >= threshold01;
        }
        
        public float GetExpressionPercentage(Expression expression)
        {
            if (expression == Expression.Uncanny)
                return _deviation * 2;
            
            if (!_currentExpressionCount.TryGetValue(expression, out int count))
                return 0;

            float percentage = (float)count / (cellResolution * cellResolution);
            return percentage;
        }

        public bool IsDeviationTooHigh(float threshold01)
        {
            return _deviation > threshold01;
        }

        public void SetShaderHighlight(float radius, Vector2 uv)
        {
            _propertyBlock ??= new();
            
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(PropertyIDs.HighlightUV, uv);
            _propertyBlock.SetFloat(PropertyIDs.HighlightRadius, radius);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }
        
        private static class PropertyIDs
        {
            public static readonly int MainTex = Shader.PropertyToID("_MainTex");
            public static readonly int HighlightUV = Shader.PropertyToID("_HighlightUV");
            public static readonly int HighlightRadius = Shader.PropertyToID("_HighlightRadius");
        }
    }

    public struct FaceCell
    {
        public Expression Expression;
        public float Deviation;

        public FaceCell(Expression expression, float deviation)
        {
            Expression = expression;
            Deviation = deviation;
        }
    }
    
    public struct GrabbedFacePart
    {
        public Expression Expression;
        public Vector2 UV;
        public int Radius;

        public GrabbedFacePart(Expression expression, Vector2 uv, int radius)
        {
            Expression = expression;
            UV = uv;
            Radius = radius;
        }
    }
}
