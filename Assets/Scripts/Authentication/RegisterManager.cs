using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour {

    [SerializeField]
    private Button createAccountButton;

    [SerializeField]
    private TMP_InputField usernameField;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private TMP_InputField displayNameField;

    void Awake(){
        
        createAccountButton.onClick.AddListener(() => {
            //function called when pressing the register button
            StartCoroutine(NetworkManagerUI.Instance.Register(usernameField.text, passwordField.text, displayNameField.text));
            //update values
            
        });

    }    
}
