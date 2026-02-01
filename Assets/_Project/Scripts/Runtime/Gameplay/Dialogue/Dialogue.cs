
using GGJ.Gameplay.Faces;
using GGJ.Gameplay.Player;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = GGJ.Inputs.PlayerInput;
namespace GGJ
{
    [RequireComponent(typeof(AudioSource))]
    public class Dialogue : MonoBehaviour
    {
        [SerializeField] private Canvas DialogueUI;
        [SerializeField] private TMPro.TextMeshProUGUI UITextMeshPro;
        [SerializeField] private float TextSpeed = 0.1f;
        [SerializeField] private float raydistance = 1;
        [SerializeField] private TMP_FontAsset AlienFont;
        [SerializeField] private PlayerFace face;
        [SerializeField] private LayerMask layerMask;
        
        private AudioSource AlienAudioSource;
        private Ray ray;
        private bool is_inDialogue = false;
        // Information recieved from Character
        private DialogueSequence sequence;
        private PlayerMovement playerMovement;
        private bool is_PayingText = false;
        private int currentIndex;
        private Coroutine currentWritingRoutine;
        private CharacterInfo currentCharacterInfo;
        private bool endWithTimer= false;
        private bool isPositiveReaction = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            face = GetComponent<PlayerFace>();
            AlienAudioSource = GetComponent<AudioSource>();
            playerMovement = this.gameObject.GetComponent<PlayerMovement>();
        }
        private void OnEnable()
        {
            PlayerInput.Input.Player.Interact.performed += hasInteracted;
        }
        private void OnDisable()
        {
            PlayerInput.Input.Player.Interact.performed -= hasInteracted;
        }
        
        private void hasInteracted(InputAction.CallbackContext context)
        {
            
            Debug.Log("Has Pressed");
            if (!is_inDialogue) 
            {
                // do a raycast 
                ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, raydistance, layerMask, QueryTriggerInteraction.Collide))
                {
                    if (hit.transform.TryGetComponent(out CharacterInfo info))
                    {
                        if (!info.stopInteract)
                        {
                            if (info.isOneliner) // if it is a oneliner Trigger oneliner
                            {
                                info.displayOneliner();
                            }
                            else   // if it has dialogue 
                            {
                                currentCharacterInfo = info;
                                sequence = currentCharacterInfo.sequence;
                                currentIndex = info.currentIndex;
                                EnterDialogue();
                            }
                        }
                    }
                }
            }
            else
            {
                // if in dialogue use Interaction to progress
                ContinueDialogue();
                Debug.Log("continueDialogue");
                // if dialogue is exited close window and resume movement abillty
            }


        }
     
        private void ContinueDialogue()
        {
            if (sequence.elements[currentIndex].ElementType == DialogueElement.Type.Dialogue)
            {
                if (is_PayingText)
                {
                    if(currentWritingRoutine != null)
                    {
                        StopCoroutine(currentWritingRoutine);
                    }
                    // finish text
                    
                    UITextMeshPro.text = sequence.elements[currentIndex].dialogue.text;
                    is_PayingText = false;
                }
                else
                {
                    LoadElement(currentIndex+1);
                }
            }
            if (sequence.elements[currentIndex].ElementType == DialogueElement.Type.Reaction)
            {
                if (isPositiveReaction)
                {
                    if (is_PayingText)
                    {
                        if (currentWritingRoutine != null)
                        {
                            StopCoroutine(currentWritingRoutine);
                        }
                        // finish text

                        UITextMeshPro.text = sequence.elements[currentIndex].positive.text;
                        is_PayingText = false;
                    }
                    else
                    {
                        LoadElement(currentIndex + 1);
                    }
                }
                else //negative
                {
                    if (is_PayingText)
                    {
                        if (currentWritingRoutine != null)
                        {
                            StopCoroutine(currentWritingRoutine);
                        }
                        // finish text

                        UITextMeshPro.text = sequence.elements[currentIndex].negative.text;
                        is_PayingText = false;
                    }
                    else
                    {
                        ExitDialogue();
                    }
                }
                
            }
            else
            {
                
            }
        }
        private void LoadElement(int index)
        {
            if (index < sequence.elements.Length)
            {
                currentIndex = index;
                if (sequence.elements[index].ElementType == DialogueElement.Type.Dialogue)
                {
                    UITextMeshPro.font = sequence.asset;
                    currentWritingRoutine = StartCoroutine(writeText(sequence.elements[index].dialogue.text));
                    currentCharacterInfo.audioSource.clip = sequence.elements[index].dialogue.audio;
                    currentCharacterInfo.audioSource.Play();
                }
                if (sequence.elements[index].ElementType == DialogueElement.Type.AnswerWithTimer)
                {
                    UITextMeshPro.font = AlienFont;
                    UITextMeshPro.text = sequence.elements[currentIndex].dialogue.text;
                    AlienAudioSource.clip = sequence.elements[index].dialogue.audio;
                    AlienAudioSource.Play();
                    endWithTimer = true;
                    StartCoroutine(waitandExit(sequence.elements[index].dialogue.audio.length));
                }
                if (sequence.elements[index].ElementType == DialogueElement.Type.AnswerNoTimer)
                {
                    UITextMeshPro.font = AlienFont;
                    UITextMeshPro.text = sequence.elements[currentIndex].dialogue.text;
                    AlienAudioSource.clip = sequence.elements[index].dialogue.audio;
                    AlienAudioSource.Play();
                    StartCoroutine(waitandExit(sequence.elements[index].dialogue.audio.length));
                }
                if (sequence.elements[index].ElementType == DialogueElement.Type.Reaction)
                { 
                    if (face.HasExpressionPercentage(sequence.elements[index].requiredExpression, sequence.elements[index].procentage)) // positive 
                    {
                        UITextMeshPro.font = sequence.asset;
                        currentWritingRoutine = StartCoroutine(writeText(sequence.elements[index].positive.text));
                        currentCharacterInfo.audioSource.clip = sequence.elements[index].positive.audio;
                        currentCharacterInfo.audioSource.Play();
                    }
                    else // negative
                    {
                        UITextMeshPro.font = sequence.asset;
                        currentWritingRoutine = StartCoroutine(writeText(sequence.elements[index].negative.text));
                        currentCharacterInfo.audioSource.clip = sequence.elements[index].negative.audio;
                        currentCharacterInfo.audioSource.Play();
                        StartCoroutine(waitandExit(sequence.elements[index].negative.audio.length));
                        currentCharacterInfo.stopInteract = true;
                    }
                }
            }
            else
            {
                ExitDialogue();
            }

        }
        IEnumerator waitandExit(float time)
        {
            yield return new WaitForSeconds(time);
            ExitDialogue();
        }
        IEnumerator writeText(string text)
        {
            
            is_PayingText = true;
            UITextMeshPro.text = "";
            foreach (char c in text) 
            {
                UITextMeshPro.text += c;
                yield return new WaitForSeconds(TextSpeed);
            }
            is_PayingText = false;
        }
        private void EnterDialogue()
        {
            Debug.Log("enterDialogue");
            is_inDialogue = true;
            // disable character controller
            playerMovement.ToggleMovementForDialogue(true);
            // enable UI
            LoadElement(currentIndex);
            // Open Menu
            DialogueUI.enabled = true;
        }

        private void ExitDialogue()
        {
            if (endWithTimer)
            {
                // start timer
                endWithTimer = false;
            }
            currentCharacterInfo.SetCurrentIndex(currentIndex+1);
            is_inDialogue = false;
            playerMovement.ToggleMovementForDialogue(false);
            DialogueUI.enabled = false;
        }
    }
}
