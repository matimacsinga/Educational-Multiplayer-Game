using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script attached to object that triggers the quiz to start
public class MinigameInteractableController : MonoBehaviour, Interactable
{
    [SerializeField]
    FirstMinigameSlides slides;

    public void Interact(){

        UIManager.instance.ClearScreen();
        MinigameController.Instance.DisplaySlides((slides));
    }
}
