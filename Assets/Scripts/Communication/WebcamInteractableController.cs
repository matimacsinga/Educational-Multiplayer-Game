using UnityEngine;

public class WebcamInteractableController : MonoBehaviour, Interactable
{
    public void Interact(){

        WebcamController.Instance.ToggleWebcam();
    }
}
