using UnityEngine;

namespace GGJ
{
    [RequireComponent (typeof(AudioSource))]
    public class CharacterInfo : MonoBehaviour
    {
        [SerializeField] private string characterName;
        public DialogueSequence sequence;
        public AudioSource audioSource;
        private int currentIndex = 0;
        private AudioTextMatch[] oneliners;
        public bool isOneliner = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (sequence.is_oneliner)
            {
                isOneliner = true;
                oneliners = sequence.oneliners;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public void displayOneliner()
        {
            Debug.Log("random oneLiner");
            // spawn a in world text box over the head with the one liner
            // play audio related to oneliner
        }
        public void SetCurrentIndex(int inputIndex)
        {
            if (!sequence.RepeatFromStart)
            {
                currentIndex = inputIndex;
            }
        }
    }
}
