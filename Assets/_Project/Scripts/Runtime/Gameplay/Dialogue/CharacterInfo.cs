using UnityEngine;

namespace GGJ
{
    [RequireComponent (typeof(AudioSource))]
    public class CharacterInfo : MonoBehaviour
    {
        [SerializeField] private string characterName;
        [SerializeField] private GameObject OneLinerPrefab;
        public DialogueSequence sequence;
        public AudioSource audioSource;
        public int currentIndex = 0;
        private AudioTextMatch[] oneliners;
        public bool isOneliner = false;
        public bool stopInteract = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
           
            audioSource = GetComponent<AudioSource> ();
            if (sequence.is_oneliner)
            {
                isOneliner = true;
                oneliners = sequence.oneliners;
            }
        }
        public void displayOneliner()
        {
            if (OneLinerPrefab != null)
            {
                Debug.Log("random oneLiner");
                AudioTextMatch randomPair = oneliners[Random.Range(0, oneliners.Length)];
                GameObject gameobject = Instantiate(OneLinerPrefab);
                // need to position over head
                gameobject.transform.position = this.gameObject.transform.position;
                gameobject.GetComponent<OneLinerText>().setText(sequence.asset, randomPair.text);
                audioSource.clip = randomPair.audio;
                audioSource.Play();
            }
            
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
