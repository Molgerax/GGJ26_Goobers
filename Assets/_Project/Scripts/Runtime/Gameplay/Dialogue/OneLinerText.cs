using System.Collections;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace GGJ
{
    public class OneLinerText : MonoBehaviour
    {
        private float timer = 2.0f;
        private TMP_Text UITextMeshPro;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            UITextMeshPro = GetComponentInChildren<TMP_Text>();
            StartCoroutine(deleteAfterTime(timer));
        }

        public void setText(TMP_FontAsset fontAsset,string text)
        {
            UITextMeshPro.font = fontAsset;
            UITextMeshPro.text = text;
        }
        
        IEnumerator deleteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(this.gameObject);
        }
    }
}
