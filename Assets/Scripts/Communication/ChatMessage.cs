using UnityEngine;
using TMPro;

public class ChatMessage : MonoBehaviour {
    
    public TMP_Text messageText;

    //setter for the text in a chat message object
    public void NewChatElement (string message){

        messageText.text = message;
    }
}
