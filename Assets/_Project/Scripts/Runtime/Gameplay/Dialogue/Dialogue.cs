
using GGJ.Gameplay.Player;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using PlayerInput = GGJ.Inputs.PlayerInput;
namespace GGJ
{
    public class Dialogue : MonoBehaviour
    {
        [SerializeField] private Canvas DialogueUI;
        [SerializeField] private TMPro.TextMeshProUGUI UITextMeshPro;
        [SerializeField] private float TextSpeed = 0.1f;
        [SerializeField] private float raydistance = 1;
        private Ray ray;
        private bool is_inDialogue = false;
        // Information recieved from Character
        private DialogueSequence sequence;
        private PlayerMovement playerMovement;
        private bool is_PayingText = false;
        private int currentIndex;
        private Coroutine currentWritingRoutine;
        private CharacterInfo currentCharacterInfo;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
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
        public void DecisionMade(int value)
        {
            // use value to check answer
            // if current element has timer start timer and exit
            // if it doenst just exit dialogue.
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
                    
                    currentWritingRoutine = StartCoroutine(writeText(sequence.elements[index].dialogue.text));
                    // play the audiofile
                }
                if (sequence.elements[index].ElementType == DialogueElement.Type.Decision)
                {
                    // Display your answer 
                }
            }
            else
            {
                ExitDialogue();
            }

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
            // showUI
            //DialogueUI.enabled = false;

            // show dialogue gox
        }

        private void ExitDialogue()
        {

            currentCharacterInfo.SetCurrentIndex(currentIndex);
            is_inDialogue = false;
            playerMovement.ToggleMovementForDialogue(false);
        }
    }
}
