using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

//variable to maintaing game states
public enum State { Login, Roaming, Dialogue, Minigame};

public class PlayerController : NetworkBehaviour {

    public float movementSpeed;
    private Vector2 playerInput;

    [SerializeField]
    private Animator playerAnimator;

    public LayerMask solidObjectLayer;
    public LayerMask interactableLayer;

    private Camera myCamera;

    PlayerController playerController; 

    GameObject player; 

    State state;
    
    private TMP_InputField messageInput;

    [SerializeField]
    private GameObject messageElement;

    private string username;

    private bool hostSelfCall = false;

    //network shared chat message offset in the chat box
    private NetworkVariable<int> offset = new NetworkVariable<int>(default,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    private const int initialOffset = 0;

    private void Awake() {
        //initialize player animator
        playerAnimator = GetComponent<Animator>();     
    }

    private void OnOffsetChanged(int previous, int current){
        //callback triggered when changing a network variable, in this case the offset
        Debug.Log($"Detected NetworkVariable Change: Previous: {previous} | Current: {current}");
    }

    //callback when a player, which is an object on the network, is spawned
    public override void OnNetworkSpawn(){

        //assign own camera to player
        myCamera = GetComponentInChildren<Camera>();
        if(!IsOwner)
            myCamera.enabled = false;

        //necessary to track offset variable value
        if(IsHost){

            offset.Value = initialOffset;
        }
        else{

            offset.OnValueChanged += OnOffsetChanged;
        }    

        //owner means that the current spawned player is the one using the script
        //since all players use the same script
        if(IsOwner){

            //assign its own chat input, since that is not shared, but only the message box itself
            messageInput = GameObject.Find("TextChatCanvas/WritingField").GetComponent<TMP_InputField>();

            username = NetworkManagerUI.Instance.user.DisplayName;

            //first state of the game is the normal roaming state
            state = State.Roaming;
            
            //subscribe to all state specific events
            DialogueManager.Instance.OnShowDialogue += () => {

                UIManager.instance.ClearScreen();
                state = State.Dialogue;
            }; 
            DialogueManager.Instance.OnHideDialogue += () => {

                if(state == State.Dialogue){

                UIManager.instance.ChatScreen();
                state = State.Roaming;
            }
            };

            MinigameController.Instance.OnShowFirstMinigame += () => {

                UIManager.instance.ClearScreen();
                state = State.Minigame;
            };
            MinigameController.Instance.OnHideFirstMinigame += () => {

                if(state == State.Minigame){

                    UIManager.instance.ChatScreen();
                    state = State.Roaming;
                }
                    
            };
        }
    }

    private void Update() {

        //game state cycle
        switch(state){

            case State.Roaming:
                //run the script only from the player it is attached to
                if(IsOwner)
                    PlayerUpdate();
                break;

            case State.Dialogue:
                DialogueManager.Instance.DialogueUpdate();
                break;

            case State.Minigame:
                MinigameController.Instance.FirstMinigameUpdate();
                break;
        }   
    }

    public void PlayerUpdate(){

        myCamera = GetComponentInChildren<Camera>();
        myCamera.enabled = true;

        //gets values of player axis position
        playerInput.x = Input.GetAxisRaw("Horizontal");
        playerInput.y = Input.GetAxisRaw("Vertical"); 

        //no diagonal moving
        if(playerInput.x != 0)
            playerInput.y = 0;

        //if player has velocity a.k.a. is moving
        if(playerInput != Vector2.zero && !messageInput.isFocused){

            //play correct animation for the player
            playerAnimator.SetFloat("xDirection", playerInput.x);
            playerAnimator.SetFloat("yDirection", playerInput.y); 

            //move player by adding input values to his position
            Vector3 targetPosition = transform.position;
            targetPosition.x += playerInput.x;
            targetPosition.y += playerInput.y;

            //check if player collides with anything, so he only walks on walkable surfaces
            if(IsWalkable(targetPosition)){

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
                playerAnimator.SetBool("isMoving", true);
            }
            else{

                playerAnimator.SetBool("isMoving", false);
            }
                
        }
        //play idle animation if player is not moving
        else
            playerAnimator.SetBool("isMoving", false);

        //press E key to interact
        if(Input.GetKeyDown(KeyCode.E))
            InteractWithObject();

        //press Return (Enter) key to open the chat
        if (Input.GetKeyDown(KeyCode.Return)) {

                //if the player has written something in the input field
                if (messageInput.text.Length > 0) {
                    //send the message and deactivate the input field
                    SendChatMessage(); 
                    messageInput.text = "";
                    messageInput.DeactivateInputField(clearSelection: true);
                } else {
                    //stop player movement while typing
                    playerAnimator.SetBool("isMoving", false);
                    messageInput.Select();
                    messageInput.ActivateInputField();
                }
        }
    }

    //checks if player can walk on object by checking its layer
    private bool IsWalkable(Vector3 targetPosition){

        if(Physics2D.OverlapCircle(targetPosition, 0.1f, solidObjectLayer | interactableLayer))
            return false;
        return true;
    }

    //main physics in the player interacting
    private void InteractWithObject(){

        //get direction of player
        Vector3 direction = new Vector3(playerAnimator.GetFloat("xDirection"), playerAnimator.GetFloat("yDirection"));
        //check actual position of interaction
        Vector3 interactPosition = transform.position + direction;
        //check if player hit something by drawing a circle in the position where he interacted
        Collider2D collider = Physics2D.OverlapCircle(interactPosition, 0.2f, interactableLayer);    
        if(collider){
            //calls specific interaction for object that player collided with
            if(collider.gameObject.GetComponent<NetworkBehaviour>())
                if(!collider.gameObject.GetComponent<NetworkBehaviour>().IsOwner){

                    Debug.Log(username+" interacted with another player");
                    WebcamController.Instance.ToggleWebcam();
                    WebcamController.Instance.ToggleOtherWebcam(collider.gameObject);

                }
            collider.GetComponent<Interactable>().Interact();
        }
        else{

            Debug.Log("Nothing in the way");
        }
    }

    public void SendChatMessage(){
        
        //only call the function from the player the script is attached to
        if(IsOwner){

            //add username in front of message
            string message = username + ": " + messageInput.text;
            //add message to own player's chat
            AddMessageToChat(message);

            //differentiate between host and client, because of this architecture
            if(IsHost){

                //helper because host is considered a client as well and he should not send himself the message twice
                hostSelfCall = true;
                //call function to add message to all other clients
                SendChatMessageClientRpc(message);
                offset.Value -= 30;
            }
            else{
                
                //send message to all others
                SendChatMessageServerRpc(message);
                //change offset from server since its a network variable and it can only be changed from there
                ChangeOffsetServerRpc(offset.Value);
            }
        }     
    }  

    private void AddMessageToChat(string message){

        if(IsHost){
            //check if the server called itself
            if(hostSelfCall)
                hostSelfCall = false;
            else{

                Debug.Log("Add Message for server");
                //find chat content on the screen
                GameObject chatContent = GameObject.Find("TextChatCanvas/ScrollView/Viewport/Content");
                //instantiate message element
                GameObject chatMessageElement = Instantiate(messageElement, GameObject.Find("TextChatCanvas/ScrollView").GetComponent<ScrollRect>().content);
                //put message in the right position
                chatMessageElement.GetComponent<RectTransform>().SetLocalPositionAndRotation(new Vector3(20,-1*(30*chatContent.transform.childCount),0), new Quaternion(0,0,0,0));
                //set values of element
                chatMessageElement.GetComponent<ChatMessage>().NewChatElement(message);        
            }
        }
        else{

            Debug.Log("Add Message for client");
            GameObject chatContent = GameObject.Find("TextChatCanvas/ScrollView/Viewport/Content");
            GameObject chatMessageElement = Instantiate(messageElement, GameObject.Find("TextChatCanvas/ScrollView").GetComponent<ScrollRect>().content);
            chatMessageElement.GetComponent<RectTransform>().SetLocalPositionAndRotation(new Vector3(20,-1*(30*chatContent.transform.childCount),0), new Quaternion(0,0,0,0));
            ChangeOffsetServerRpc(offset.Value);
            chatMessageElement.GetComponent<ChatMessage>().NewChatElement(message);    
        }
    }

    //methods with this decorator will be called from the client but executed on the server (or host in this case)
    [ServerRpc]
    private void SendChatMessageServerRpc(string message){

        Debug.Log("Server Rpc was called sucessfully");
        AddMessageToChat(message); 
    } 

    //methods with this decorator will be called from the server but executed on the client
    [ClientRpc]
    private void SendChatMessageClientRpc(string message){

        Debug.Log("Client Rpc was called sucessfully");
        AddMessageToChat(message); 
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeOffsetServerRpc(int offsetValue){

        offsetValue -= 30;
    }
}
