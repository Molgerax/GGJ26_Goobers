
using GGJ.Gameplay.Player;
using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
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
                ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward.normalized * raydistance);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.CompareTag("Character"))
                    {
                        Debug.Log("Hit A Character");
                        CharacterInfo info = hit.transform.GetComponent<CharacterInfo>();
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
                    // play the audiofile
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
                    // display text as answer
                    // exit after completion
                    StartCoroutine(waitandExit(sequence.elements[index].dialogue.audio.length));
                    
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
            currentCharacterInfo.SetCurrentIndex(currentIndex);
            is_inDialogue = false;
            playerMovement.ToggleMovementForDialogue(false);
            DialogueUI.enabled = false;
        }
    }
}
