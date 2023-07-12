using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using TMPro;


public class NetworkManagerUI : MonoBehaviour {

    //firebase variables
    public FirebaseAuth auth;    
    public FirebaseUser user;
    public DatabaseReference databaseReference;

    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button clientButton;

    [SerializeField]
    private Button registerButton;

    [SerializeField]
    private TMP_InputField email;

    [SerializeField]
    private TMP_InputField password;

    //event to subscribe to when user logs in
    public event Action OnLogin;

    //create instance to access easily from other objects
    public static NetworkManagerUI Instance { get; private set; }

    private void Awake(){

        InitializeFirebase();

        Instance = this;

        //add functions to buttons for host login, client login, and register
        hostButton.onClick.AddListener(() => {
            
            StartCoroutine(Login(email.text, password.text, true));
        });
        clientButton.onClick.AddListener(() => {
            
            StartCoroutine(Login(email.text, password.text, false));
        });
        registerButton.onClick.AddListener(() => {
            
            RegisterButton();
        });
    }

    private void InitializeFirebase(){
        //get values from firebase api
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private IEnumerator Login(string email, string password, bool isHost){

        //call firebase function for signing in
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
        if (LoginTask.Exception != null){
            
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
        }
        else{

            //get user variable to use for the rest of the session when querying the database
            user = LoginTask.Result;
            //instantiate the chat
            UIManager.instance.ChatScreen();
            //start network instances of the user for host or client
            if(isHost)
                NetworkManager.Singleton.StartHost();
            else
                NetworkManager.Singleton.StartClient();
            //invoke login event
            OnLogin?.Invoke();
        }
    } 

    public void RegisterButton(){

        UIManager.instance.RegisterScreen();
    }

    public IEnumerator Register(string email, string password, string username){
        
        //sign in same as login
        var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);
        if (RegisterTask.Exception != null){

            Debug.Log("Invalid Credentials");    
        }
        else{

            user = RegisterTask.Result;
            if (user != null){

                //creates user in the database
                UserProfile profile = new UserProfile{DisplayName = username};
                var ProfileTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);
                if (ProfileTask.Exception != null){

                    Debug.Log("Couldn't create profile");    
                }
                else{

                    var SetUsernameTask = databaseReference.Child("users").Child(user.UserId).Child("username").SetValueAsync(username);
                    yield return new WaitUntil(predicate: () => SetUsernameTask.IsCompleted);
                    if (SetUsernameTask.Exception != null){

                        Debug.LogWarning(message: $"Failed to register task with {SetUsernameTask.Exception}");
                    }
                    else{
                        
                        var InitializeLessonsTask = databaseReference.Child("users").Child(user.UserId).Child("lessons").SetValueAsync(0);
                        yield return new WaitUntil(predicate: () => InitializeLessonsTask.IsCompleted);
                        if (InitializeLessonsTask.Exception != null){

                            Debug.LogWarning(message: $"Failed to register task with {InitializeLessonsTask.Exception}");
                        }
                        else{

                            var InitializeCheatsTask = databaseReference.Child("users").Child(user.UserId).Child("averagecheats").SetValueAsync(0);
                            yield return new WaitUntil(predicate: () => InitializeCheatsTask.IsCompleted);
                            if (InitializeCheatsTask.Exception != null){

                                Debug.LogWarning(message: $"Failed to register task with {InitializeCheatsTask.Exception}");
                            }
                            else{

                                var InitializeTriesTask = databaseReference.Child("users").Child(user.UserId).Child("averagetries").SetValueAsync(0);
                                yield return new WaitUntil(predicate: () => InitializeTriesTask.IsCompleted);
                                if (InitializeTriesTask.Exception != null){

                                    Debug.LogWarning(message: $"Failed to register task with {InitializeTriesTask.Exception}");
                                }
                                else{
                                    
                                    var InitializeSkipTask = databaseReference.Child("users").Child(user.UserId).Child("skipquestion").SetValueAsync(0);
                                    yield return new WaitUntil(predicate: () => InitializeSkipTask.IsCompleted);
                                    if (InitializeSkipTask.Exception != null){

                                        Debug.LogWarning(message: $"Failed to register task with {InitializeSkipTask.Exception}");
                                    }
                                    else{
                                        
                                        var InitializeFiftyFiftyTask = databaseReference.Child("users").Child(user.UserId).Child("fiftyfifty").SetValueAsync(0);
                                        yield return new WaitUntil(predicate: () => InitializeFiftyFiftyTask.IsCompleted);
                                        if (InitializeFiftyFiftyTask.Exception != null){

                                            Debug.LogWarning(message: $"Failed to register task with {InitializeFiftyFiftyTask.Exception}");
                                        }
                                        else{
                                            
                                            UIManager.instance.LoginScreen();    
                                        }
                                    }
                                }    
                            }    
                        }
                    }
                }
            }
        }    
    }

    public IEnumerator SetUsername(string username){

        //set the username of the current user in the realtime database
        Debug.Log(databaseReference);
        Debug.Log(user);
        Debug.Log(user.UserId);
        Debug.Log(username);
        var SetUsernameTask = databaseReference.Child("users").Child(user.UserId).Child("username").SetValueAsync(username);
        yield return new WaitUntil(predicate: () => SetUsernameTask.IsCompleted);
        if (SetUsernameTask.Exception != null){

            Debug.LogWarning(message: $"Failed to register task with {SetUsernameTask.Exception}");
        }
        else{
        
        }
    }
}
