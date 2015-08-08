using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using System.Collections.Generic;
//using ExitGames.Client.Photon.LoadBalancing;

public class NetworkingController : MonoBehaviour {

    public Text statusText;
    public PhaseManager phaseManager;

    public GameSettings gameSettings;


    //CHAT SETTINGS
    // keep up to this many messages, after which older messages start to be deleted
    public int MaxMessages = 100;
    public InputField inputField;
    public Text chatText;
    public Scrollbar chatScrollBar;
    private List<string> chatMessages = new List<string>();

    private string typedMessage = "";

    void Awake() 
    {
        if (gameSettings.offlineMode)
        {
            Debug.Log("setting Photon to offline mode");
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom("null room");
        }
        chatText.supportRichText = true;
    }

	// Use this for initialization
	void Start () {

        if (PhotonNetwork.room != null)
        {
            string message = "<i>"+ PhotonNetwork.player.name + " joined room " + PhotonNetwork.room.name + "</i>";
            //UpdateStatus(message);
            this.GetComponent<PhotonView>().RPC("SendChat", PhotonTargets.All, message);        //Use this instead once chat is set up
        }
        else
            DisableStatus();

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            typedMessage = "<b>" + PhotonNetwork.player.name + ": </b>" + inputField.text;
            this.GetComponent<PhotonView>().RPC("SendChat", PhotonTargets.All, typedMessage);
            inputField.text = "";
        }
	}

    public void DisableStatus()
    {
        statusText.enabled = false;
    }

    public void UpdateStatus(string str)
    {
        statusText.enabled = true;
        statusText.text = str;
    }


    //Send text to all clients and display in network status area
    [PunRPC]
    public void SendChat(string message)
    {
        Vector2 chatTextSize = chatText.rectTransform.sizeDelta;
        // store the received message
        chatMessages.Add(message);

        // enforce maximum stored messages
        if (chatMessages.Count > MaxMessages)
        {
            chatMessages.RemoveAt(0);
        }

        //Increase text box size for each message
        chatTextSize.y = (float)chatMessages.Count * 15;
        chatText.rectTransform.sizeDelta = chatTextSize;
        

        chatText.text = "";
        foreach (string msg in chatMessages)
        {
            chatText.text =  chatText.text + "> " + msg + "\n";
        }

        // set scroll value to zero to snap to latest
        chatScrollBar.value = 0f;
    }

    //On quitting, save board, disconnect from server, notify all clients that player has disconnected
    public void OnApplicationQuit()
    {
        //Save board
        
        //Disconnect from server
        PhotonNetwork.Disconnect();


    }

    public void ReturnToLobby()
    {
        //Save board

        //Disconnect from server
        //PhotonNetwork.Disconnect();

        //Leave room. Return to lobby
        PhotonNetwork.LeaveRoom();
    }

    public void AbandonGame()
    {
        //Don't save board
        //Discard any saved board

        //Notify all clients that player has disconnected
        string message = "Player " + PhotonNetwork.player.name + " has forfeited the game";
        this.GetComponent<PhotonView>().RPC("SendChat", PhotonTargets.All, message); 

        //Disconnect from server
        PhotonNetwork.Disconnect();
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        //Notify all clients that player has disconnected
        string message = "Player " + player.name + " has left game";
        this.GetComponent<PhotonView>().RPC("SendChat", PhotonTargets.All, message); 
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        //Doesn't work yet
        
        //if (PhotonNetwork.playerList.GetLength(0) == 2) //If both players are connected
        //{
        //    string message = "<i>";
        //    foreach (PhotonPlayer pl in PhotonNetwork.playerList)
        //    {
        //        string playerColor = "";
        //        if (pl.GetTeam() == PunTeams.Team.red)
        //        {
        //            playerColor = "White";
        //        }
        //        else if (pl.GetTeam() == PunTeams.Team.blue)
        //        {
        //            playerColor = "Black";
        //        }

        //        message = message + pl.name + " is " + playerColor + "\n";
        //    }

        //    message = message + "</i>";
        //    this.GetComponent<PhotonView>().RPC("SendChat", PhotonTargets.All, message);
        //}
    }

    //public void SaveBoardAsProperty()
    //{
    //    CubeBoard board = GameObject.FindObjectOfType(typeof(CubeBoard)) as CubeBoard;
    //    this.turnNumber = this.turnNumber + 1;
    //    this.lastTurnPlayer = this.LocalPlayer.ID;

    //    Hashtable boardProps = board.GetBoardAsCustomProperties();
    //    boardProps.Add("lt", this.lastTurnPlayer);  // "lt" is for "last turn" and contains the ID/actorNumber of the player who did the last one
    //    boardProps.Add("t#", this.turnNumber);

    //    this.OpSetCustomPropertiesOfRoom(boardProps, false);

    //    Debug.Log("saved board to props " + SupportClass.DictionaryToString(boardProps));
    //}

}
