using GGJ.Gameplay.Faces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ.UI
{
    public class ExpressionFillbar : MonoBehaviour
    {
        [SerializeField] private Expression expression;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image fillbar;

        private void OnEnable()
        {
            nameText.text = expression.ToString();
            nameText.color = GetColorFromExpression(expression);
            PlayerFace.OnUpdateFaceValues += SetExpression;
        }

        private void OnDisable()
        {
            PlayerFace.OnUpdateFaceValues -= SetExpression;
        }

        
        public void SetExpression(PlayerFace face)
        {
            float percentage = face.GetExpressionPercentage(expression);

            if (fillbar)
            {
                fillbar.fillAmount = Mathf.Clamp01(percentage);
                fillbar.color = GetColorFromExpression(expression);
            }
        }



        private Color GetColorFromExpression(Expression expression)
        {
            switch (expression)
            {
                case Expression.Anger:
                    return new Color(0.8f, 0f, 0f);
                case Expression.Joy:
                    return new Color(0.8f, 0.8f, 0);
                case Expression.Sad:
                    return new Color(0, 0, 0.8f);
                case Expression.Disgust:
                    return new Color(0, 0.8f, 0);
                case Expression.Fear:
                    return new Color(0.8f, 0f, 0.8f);
                case Expression.Surprise:
                    return new Color(0, 0.8f, 0.8f);
                case Expression.Uncanny:
                    return new Color(0.3f, 0.4f, 0.2f);
            }
            
            return Color.black;
        }
    }
}