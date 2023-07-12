using UnityEngine;

//each NPC that communicates with the player through dialogue will have this script attached
public class NPCController : MonoBehaviour, Interactable{

    [SerializeField]
    Dialogue dialogue;

    public void Interact(){

        StartCoroutine(DialogueManager.Instance.DisplayDialogue(dialogue));
    }    
}
