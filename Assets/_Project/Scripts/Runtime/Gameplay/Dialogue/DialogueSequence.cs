using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace GGJ
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "Scriptable Objects/DialogueSequence")]
    
    public class DialogueSequence : ScriptableObject
    {
        public TMP_FontAsset asset;
        public bool is_oneliner;
        public AudioTextMatch[] oneliners;
        public DialogueElement[] elements;
        public bool RepeatFromStart = false;
    }
    [System.Serializable]
    public class DialogueElement
    {
        
        public enum Type{
            Dialogue,
            AnswerWithTimer,
            AnswerNoTimer
        }
        /// <summary>
        /// Select The Type of element
        /// </summary>
        public Type ElementType;
        /// <summary>
        /// repeat the dialoge from the beginning next time you interact
        /// </summary>
        public bool repeatThisDialogue;
        /// <summary>
        ///  if this is dialogue only create one entry
        /// </summary>
        public AudioTextMatch dialogue;
    }
  
    [System.Serializable]
    public class AudioTextMatch
    {
        public string text;
        public AudioClip audio;
    }
}
