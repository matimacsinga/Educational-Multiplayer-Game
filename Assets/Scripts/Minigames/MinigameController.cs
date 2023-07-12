using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Database;

public class MinigameController : MonoBehaviour{

    [SerializeField]
    private NetworkManagerUI networkManagerUI;

    [SerializeField]
    private TMP_Text fiftyHintText;

    [SerializeField]
    private GameObject cheatsButtons;

    [SerializeField]
    private TMP_Text skipCheatsText;
    [SerializeField]
    private TMP_Text fiftyCheatsText;

    //variables to store cheats and lessons info from the database
    private int skipCheatsNb;
    private int fiftyCheatsNb;
    private int lessonsCompleted;

    //bool to keep track if the player used the 50/50 on a slide (since you can only use it once)
    private bool usedFiftyThisSlide = false;

    [SerializeField]
    GameObject box;

    FirstMinigameSlides slides;

    int firstMinigameSlideIndex = 0;
    int firstMinigameQuestions = 3;

    //list of correct answers
    private KeyCode[] firstMinigameCorrectAnswers = {KeyCode.A, KeyCode.C, KeyCode.B};

    //list of wrong answers with each list of answers being at the index representing the question number
    private List<List<KeyCode>> firstMinigameWrongAswers = new List<List<KeyCode>>{ 
                                                                                    new List<KeyCode>{KeyCode.B, KeyCode.C, KeyCode.D}, 
                                                                                    new List<KeyCode>{KeyCode.B, KeyCode.A, KeyCode.D},
                                                                                    new List<KeyCode>{KeyCode.C, KeyCode.A, KeyCode.D}};
    
    //converts keycode pressed keys to strings of the keys to help with the 50/50 cheat
    private Dictionary<KeyCode, string> keyToString = new Dictionary<KeyCode, string>{ {KeyCode.A, "A"},
                                                                                        {KeyCode.B, "B"},
                                                                                        {KeyCode.C, "C"},
                                                                                        {KeyCode.D, "D"}};

    //events to subscribe to when starting or ending the minigame
    public event Action OnShowFirstMinigame;
    public event Action OnHideFirstMinigame;

    public static MinigameController Instance { get; private set; }

    private void Awake() {
        
        Instance = this; 
    }    

    public void FirstMinigameUpdate() {

        //iterate through each question
        if(firstMinigameSlideIndex < firstMinigameQuestions){

            //if player answers correctly
            if(Input.GetKeyDown(firstMinigameCorrectAnswers[firstMinigameSlideIndex])){
                
                //go to next question
                firstMinigameSlideIndex++;
                //if last question answered correctly
                if(firstMinigameSlideIndex == firstMinigameQuestions){

                    cheatsButtons.SetActive(false);
                    if(lessonsCompleted < 1)
                        StartCoroutine(IncreaseLessonsCompleted());
                }

                //display next question slide    
                slides.Slides[firstMinigameSlideIndex-1].SetActive(false);
                slides.Slides[firstMinigameSlideIndex].SetActive(true);  
                //reset cheat helper variables 
                usedFiftyThisSlide = false; 
                fiftyHintText.text = "";
            }

            else{

                //iterate through wrong answers for that question
                foreach(KeyCode wrongAnswer in firstMinigameWrongAswers[firstMinigameSlideIndex]){
                    //check if user pressed a wrong answer key
                    if(Input.GetKeyDown(wrongAnswer)){

                        //reset helpers and move to "failed quiz" slide
                        StartCoroutine(IncrementDatabaseValue("averagetries"));
                        cheatsButtons.SetActive(false);
                        int previousIndex = firstMinigameSlideIndex;
                        firstMinigameSlideIndex = 4;
                        fiftyHintText.text = "";
                        usedFiftyThisSlide = false;
                        slides.Slides[previousIndex].SetActive(false);
                        slides.Slides[firstMinigameSlideIndex].SetActive(true);
                        break;
                    }
                }      
            }        
        }
        else{
            //if users presses E to navigate out of "fail quiz" or "passed quiz" screen
            if(Input.GetKeyDown(KeyCode.E) && firstMinigameSlideIndex >= firstMinigameQuestions){

                slides.Slides[firstMinigameSlideIndex].SetActive(false);
                box.SetActive(false);
                firstMinigameSlideIndex = 0;
                OnHideFirstMinigame?.Invoke();
            }
        }
    }

    public void DisplaySlides(FirstMinigameSlides slides) {
        
        OnShowFirstMinigame?.Invoke();
        this.slides = slides;
        box.SetActive(true);
        slides.Slides[0].SetActive(true);
        cheatsButtons.SetActive(true);
        StartCoroutine(GetCheatsFromDB());
    }

    //called when user presses the skip question button
    public void SkipCheatButton(){

        StartCoroutine(UseSkipCheat());
    }

    //called when user presses the 50/50 button
    public void FiftyCheatButton(){

        if(!usedFiftyThisSlide)
            StartCoroutine(UseFiftyCheat());
        else
            Debug.Log("Already used 50/50 on this question");
    }

    //coroutine to get number of cheats the user has from the database
    private IEnumerator GetCheatsFromDB(){

        var DBTask = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null){

            Debug.Log("User has no data");
        }
        else{
            
            DataSnapshot snapshot = DBTask.Result;
            
            string skipString = snapshot.Child("skipquestion").Value.ToString();
            skipCheatsText.text = skipString;
            skipCheatsNb = Int32.Parse(skipString);
            
            string fiftyString = snapshot.Child("fiftyfifty").Value.ToString();
            fiftyCheatsText.text = fiftyString;
            fiftyCheatsNb = Int32.Parse(fiftyString);

            string lessonsString = snapshot.Child("lessons").Value.ToString();
            lessonsCompleted = Int32.Parse(lessonsString);
        }        
    }

    //coroutine to increase completed lessons after passin the quiz
    private IEnumerator IncreaseLessonsCompleted(){

        lessonsCompleted++;
        var DBTask = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).Child("lessons").SetValueAsync(lessonsCompleted);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else{

            
        }    
    }

    //actual coroutine for using the 50/50 cheat
    private IEnumerator UseFiftyCheat(){

        var DBTask = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null){

            Debug.Log("User has no data");
        }
        else{

            DataSnapshot snapshot = DBTask.Result;
            string cheatsNb = snapshot.Child("fiftyfifty").Value.ToString();
            fiftyCheatsNb = Int32.Parse(cheatsNb);

            if(fiftyCheatsNb > 0){

                fiftyCheatsNb--;
                usedFiftyThisSlide = true;
                StartCoroutine(SetValueInDatabase("fiftyfifty", fiftyCheatsNb));
                StartCoroutine(IncrementDatabaseValue("averagecheats"));
                int eliminatedCounter = 0;
                string[] answersToEliminate = new string[2];

                foreach(KeyCode keyCode in keyToString.Keys){

                    if(keyCode != firstMinigameCorrectAnswers[firstMinigameSlideIndex]){
                        
                        answersToEliminate[eliminatedCounter] = keyToString[keyCode];
                        eliminatedCounter++;

                        if(eliminatedCounter == 2)
                            break;
                    }
                }

                fiftyHintText.text = "The answers " + answersToEliminate[0] + " and " + answersToEliminate[1] + " are incorrect";
            }
        }    
    }
    //actual coroutine for using the skip question cheat
    private IEnumerator UseSkipCheat(){

        var DBTask = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null){

            Debug.Log("User has no data");
        }
        else{

            DataSnapshot snapshot = DBTask.Result;
            string cheatsNb = snapshot.Child("skipquestion").Value.ToString();
            skipCheatsNb = Int32.Parse(cheatsNb);

            if(skipCheatsNb > 0){

                skipCheatsNb--;
                StartCoroutine(SetValueInDatabase("skipquestion", skipCheatsNb));
                StartCoroutine(IncrementDatabaseValue("averagecheats"));
                firstMinigameSlideIndex++;
                slides.Slides[firstMinigameSlideIndex-1].SetActive(false);
                slides.Slides[firstMinigameSlideIndex].SetActive(true);
                usedFiftyThisSlide = false;
                fiftyHintText.text = "";
            }
        }     
    }

    private IEnumerator SetValueInDatabase(string field, int value){

        var DBTask = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).Child(field).SetValueAsync(value);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else{
            if(field == "skipquestion")
                skipCheatsText.text = skipCheatsNb.ToString();
            if(field == "fiftyfifty")
                skipCheatsText.text = fiftyCheatsNb.ToString();
        }    
    }

    private IEnumerator IncrementDatabaseValue(string field){

        var DBTask = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).Child(field).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else{

            DataSnapshot snapshot = DBTask.Result;
            int averageValue = Int32.Parse(snapshot.Value.ToString());
            averageValue++;
            var IncreaseTask = networkManagerUI.databaseReference.Child("users").Child(networkManagerUI.user.UserId).Child(field).SetValueAsync(averageValue);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            if (DBTask.Exception != null){

                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else{
                
                    
            }        
        }    
    }
}
