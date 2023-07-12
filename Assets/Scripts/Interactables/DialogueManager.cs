using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

    //where dialogue is displayed
    [SerializeField]
    GameObject dialogueBox;
    //actual text which will be displayed
    [SerializeField]
    Text textBlock;
    //speed at which letters appear in the box
    [SerializeField]
    int letterSpeed;

    //event to subscribe to when showing or hiding dialogue
    public event Action OnShowDialogue;
    public event Action OnHideDialogue;

    //instance of Dialogue class which will contain the lines of dialogue
    Dialogue dialogueObject;

    //helpers
    int messageIndex = 0;
    bool isTyping;

    public static DialogueManager Instance { get; private set; }

    private void Awake() {
        
        Instance = this;
    }

    public void DialogueUpdate() {

        //press E to skip to the next line
        if(Input.GetKeyDown(KeyCode.E) && !isTyping){

            //increase index to get to the next line
            messageIndex++;
            //type line until the last
            if(messageIndex < dialogueObject.Lines.Count){

                StartCoroutine(TypeText(dialogueObject.Lines[messageIndex]));
            }
            //after last line was typed, close dialogue box
            else{

                dialogueBox.SetActive(false);
                messageIndex = 0;
                OnHideDialogue?.Invoke();
            }
        }    
    }

    //function to start showing dialogue when interacting
    public IEnumerator DisplayDialogue(Dialogue dialogueObject){
        
        //smooth it out by waiting for the frame to end
        yield return new WaitForEndOfFrame();
        //invoke event
        OnShowDialogue?.Invoke();
        this.dialogueObject = dialogueObject;
        //show box
        dialogueBox.SetActive(true);
        //start typing
        StartCoroutine(TypeText(dialogueObject.Lines[0]));
    }

    //function to display the actual letters of the text at the given speed
    public IEnumerator TypeText(string line){

        isTyping = true;
        textBlock.text = "";
        //iterate through each letter of line
        foreach(var letter in line.ToCharArray()){

            //add letter to displayed text
            textBlock.text += letter;
            //add at given letter speed by waiting
            yield return new WaitForSeconds(1f / letterSpeed);
        }    
        isTyping = false;    
    }
}
