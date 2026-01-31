using UnityEngine;

namespace GGJ.Gameplay.Faces
{
    [CreateAssetMenu(fileName = "FaceTexture", menuName = "Scriptable Objects/FaceTexture", order = 0)]
    public class FaceTexture : ScriptableObject
    {
        [SerializeField] private Texture2D texture;
        [SerializeField] private Expression expression;

        public Texture2D Texture => texture;
        public Expression Expression => expression;
    }
}