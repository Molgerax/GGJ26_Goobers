using UnityEngine;

namespace GGJ.Gameplay.Faces
{
    [CreateAssetMenu(fileName = "FaceTextureCollection", menuName = "Scriptable Objects/FaceTextureCollection", order = 0)]
    public class FaceTextureCollection : ScriptableObject
    {
        [SerializeField] private FaceTexture joy;
        [SerializeField] private FaceTexture sadness;
        [SerializeField] private FaceTexture anger;
        [SerializeField] private FaceTexture fear;
        [SerializeField] private FaceTexture disgust;
        [SerializeField] private FaceTexture surprise;

        public FaceTexture GetFromExpression(Expression expression)
        {
            switch (expression)
            {
                case Expression.Joy:
                    return joy;
                case Expression.Sad:
                    return sadness;
                case Expression.Anger:
                    return anger;
                case Expression.Fear:
                    return fear;
                case Expression.Disgust:
                    return disgust;
                case Expression.Surprise:
                    return surprise;
            }

            return null;
        }
        
        public FaceTexture GetFromExpression(FaceExpression expression)
        {
            switch (expression)
            {
                case FaceExpression.Joy:
                    return joy;
                case FaceExpression.Sad:
                    return sadness;
                case FaceExpression.Anger:
                    return anger;
                case FaceExpression.Fear:
                    return fear;
                case FaceExpression.Disgust:
                    return disgust;
                case FaceExpression.Surprise:
                    return surprise;
            }

            return null;
        }
    }

    public enum FaceExpression
    {
        Joy = 0,
        Sad = 1,
        Anger = 2,
        Fear = 3,
        Disgust = 4,
        Surprise = 5,
    }
}