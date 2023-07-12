using UnityEngine;
using UnityEngine.UI;

public class WebcamController : MonoBehaviour
{

    WebCamTexture texture;

    WebCamTexture otherTexture;

    public RawImage display;

    public RawImage otherDisplay;

    [SerializeField]
    GameObject webcamWindow;

    [SerializeField]
    GameObject webcamFrame;

    [SerializeField]
    GameObject otherWebcamWindow;

    [SerializeField]
    GameObject otherWebcamFrame;

    public WebCamDevice webcam;

    public WebCamDevice secondWebcam;

    public static WebcamController Instance { get; private set; }

    private void Awake() {
        
        Instance = this;
        webcam = WebCamTexture.devices[0];
        secondWebcam = WebCamTexture.devices[1];

    }
    
    public void ToggleWebcam(){

        if(texture){

            webcamWindow.SetActive(false);
            webcamFrame.SetActive(false);
            display.texture = null;
            texture.Stop();
            texture = null;
        }
        else{

            webcamWindow.SetActive(true);
            webcamFrame.SetActive(true);
            WebCamDevice currentWebcam = webcam;
            texture = new WebCamTexture(currentWebcam.name);
            display.texture = texture;
            texture.Play();
        }  
    }

    public void ToggleOtherWebcam(GameObject otherPlayer){

        if(otherTexture){

            otherWebcamWindow.SetActive(false);
            otherWebcamFrame.SetActive(false);
            otherDisplay.texture = null;
            otherTexture.Stop();
            otherTexture = null;
        }
        else{

            otherWebcamWindow.SetActive(true);
            otherWebcamFrame.SetActive(true);
            WebCamDevice currentWebcam = otherPlayer.GetComponent<WebcamController>().secondWebcam;
            otherTexture = new WebCamTexture(currentWebcam.name);
            otherDisplay.texture = otherTexture;
            otherTexture.Play();
        }    
    }
}
