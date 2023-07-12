using UnityEngine;

//script that controls the flow of the past lessons UI
public class PastLessonsViewer : MonoBehaviour{

    [SerializeField]
    private GameObject welcomeText;

    [SerializeField]
    private GameObject lessonButton;

    [SerializeField]
    private GameObject lessonText;

    [SerializeField]
    private GameObject backButton;

    [SerializeField]
    private GameObject closeButton;

    //show the lesson when pressing the lesson button
    public void LessonButton(){

        welcomeText.SetActive(false);
        lessonButton.SetActive(false);
        closeButton.SetActive(false);
        lessonText.SetActive(true);
        backButton.SetActive(true);
    }

    //go back to main UI when pressing the back button from an opened lesson
    public void BackButton(){

        welcomeText.SetActive(true);
        lessonButton.SetActive(true);
        closeButton.SetActive(true);
        lessonText.SetActive(false);
        backButton.SetActive(false);    
    }

    //clear UI when pressing the close button
    public void CloseButton(){

        UIManager.instance.ClearScreen();
    }
}
