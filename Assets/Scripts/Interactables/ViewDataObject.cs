using UnityEngine;
using System.Linq;
using TMPro;
using System.Collections;
using Firebase.Database;
using System;

public class ViewDataObject : MonoBehaviour, Interactable{

    [SerializeField]
    private NetworkManagerUI networkManagerUI;

    public GameObject scoreElement;
    public Transform scoreboardContent;

    [SerializeField]
    private TMP_Text usernameField;
    [SerializeField]
    private TMP_Text lessonsCompletedField;
    [SerializeField]
    private TMP_Text averageTriesField;
    [SerializeField]
    private TMP_Text averageCheatsField;

    public void Interact(){
        
        //set username to display name from database
        usernameField.text = networkManagerUI.user.DisplayName;
        //start loading data
        StartCoroutine(LoadUserData());
        //show user data screen
        UIManager.instance.UserDataScreen();
    }

    private IEnumerator LoadUserData(){

        //get current logged in user data from database
        var query = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => query.IsCompleted);
        if (query.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {query.Exception}");
        }
        else if (query.Result.Value == null){

            //if user does not have any data yet
            lessonsCompletedField.text = "0";
            averageTriesField.text = "0";
            averageCheatsField.text = "0";
        }
        else{

            //set UI fields to database values
            DataSnapshot snapshot = query.Result;
            lessonsCompletedField.text = snapshot.Child("lessons").Value.ToString();
            if(lessonsCompletedField.text != "0"){

                averageTriesField.text = Math.Round(Decimal.Divide(Int32.Parse(snapshot.Child("averagetries").Value.ToString()), Int32.Parse(snapshot.Child("lessons").Value.ToString())), 2).ToString();
                averageCheatsField.text = Math.Round(Decimal.Divide(Int32.Parse(snapshot.Child("averagecheats").Value.ToString()), Int32.Parse(snapshot.Child("lessons").Value.ToString())), 2).ToString();
            }
            else{

                averageTriesField.text = "0";
                averageCheatsField.text = "0";
            }
        }    
    }

    //function called when pressing the scoreboard button
    public void ScoreboardButton(){

        StartCoroutine(LoadScoreboardData());
    }

    private IEnumerator LoadScoreboardData(){
        
        //get all users from database ordered by number of lessons completed
        var query = networkManagerUI.databaseReference.Child("users").OrderByChild("lessons").GetValueAsync();
        yield return new WaitUntil(predicate: () => query.IsCompleted);
        if (query.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {query.Exception}");
        }
        else{
            DataSnapshot snapshot = query.Result;
            //clear scoreboard
            foreach (Transform oldEntry in scoreboardContent.transform){

                Destroy(oldEntry.gameObject);
            }
            //screen position offset for placing entries on the scoreboard
            int positionOffset = -100;
            //iterate through user list
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>()){

                //set scoreboard entry values to values from database
                string username = childSnapshot.Child("username").Value.ToString();
                int lessons = int.Parse(childSnapshot.Child("lessons").Value.ToString());
                int cheats = int.Parse(childSnapshot.Child("averagecheats").Value.ToString());
                int tries = int.Parse(childSnapshot.Child("averagetries").Value.ToString());
                //create scoreboard element for user
                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                //place element on scoreboard
                scoreboardElement.GetComponent<RectTransform>().SetLocalPositionAndRotation(new Vector3(30,positionOffset,0), new Quaternion(0,0,0,0));
                positionOffset = positionOffset - 50;
                //set element values
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, lessons, cheats, tries);
            }
            //after scoreboard is created, show scoreboard screen
            UIManager.instance.ScoreboardScreen();
        }
    }

    //function called when pressing the back button
    public void ScoreboardBackButton(){
        
        UIManager.instance.ClearScreen();
    }

    //function called when pressing the past lessons button
    public void PastLessonsButton(){

        UIManager.instance.PastLessonsScreen();
    }
}
