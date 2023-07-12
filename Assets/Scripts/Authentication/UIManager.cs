using UnityEngine;

public class UIManager : MonoBehaviour {

    //create public instance for any other script to have control of the UI
    public static UIManager instance;

    //all screens in the game
    public GameObject loginUI;
    public GameObject userDataUI;
    public GameObject scoreboardUI;
    public GameObject registerUI;
    public GameObject chatUI;
    public GameObject pastLessonsUI;

    private void Awake(){

        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(this);
    }

    //functions to hide all screens
    public void ClearScreen(){

        loginUI.SetActive(false);
        userDataUI.SetActive(false);
        scoreboardUI.SetActive(false);
        registerUI.SetActive(false);
        chatUI.SetActive(false);
        pastLessonsUI.SetActive(false);
    }

    //functions to show a certain screen
    public void LoginScreen(){

        ClearScreen();
        loginUI.SetActive(true);
    }

    public void UserDataScreen(){

        ClearScreen();
        userDataUI.SetActive(true);
    }

    public void ScoreboardScreen(){

        ClearScreen();
        scoreboardUI.SetActive(true);
    }

    public void RegisterScreen(){

        ClearScreen();
        registerUI.SetActive(true);
    }

    public void ChatScreen(){

        ClearScreen();
        chatUI.SetActive(true);
    }

    public void PastLessonsScreen(){

        ClearScreen();
        pastLessonsUI.SetActive(true);
    }
}