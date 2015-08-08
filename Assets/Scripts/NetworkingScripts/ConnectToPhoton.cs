using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class ConnectToPhoton : MonoBehaviour {

    public GameSettings gameSettings;

    //GUI Variables
    public GUISkin guiSkin;
    private Rect windowRect = new Rect(0, 0, 400, 380);
    private Vector2 scrollPosition = Vector2.zero;

    public List<string> testRoomList;

    RoomOptions roomOptions;
    private string username = "";
    private bool connecting = false;
    bool joined = false;

    ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
 
    void Start()
    {
        
        
        // load the last username the player entered
        username = PlayerPrefs.GetString("Username", "");
        
        roomOptions = new RoomOptions() { isVisible = true, maxPlayers = 2 };
        
        windowRect.x = (Screen.width - windowRect.width) / 2;
        windowRect.y = (Screen.height - windowRect.height) / 2;

        testRoomList = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            testRoomList.Add("RandomName" + Random.Range(0, 999).ToString());
        }
        
    }

    


    void OnJoinedLobby()
    {
        // we joined Photon and are ready to get a list of rooms
        joined = true;
        connecting = false;
    }

    void OnFailedToConnectToPhoton( DisconnectCause cause )
    {
        // some error occurred, 'cause' is an enumeration of the error that happened
    }

    void OnGUI()
    {
        GUI.skin = guiSkin;

        if (!joined)// && string.IsNullOrEmpty(error))
        {
            windowRect = GUI.Window(0, windowRect, DrawConnectScreen, "Hive Clone");

        }

        else if (connecting)// && string.IsNullOrEmpty(error))
        {
            windowRect = GUI.Window(0, windowRect, DrawLoadScreen, "Multiplayer Lobby");

        }
        else if (joined)
        {
            // we're connected to Photon, so now we can draw the lobby screen
            windowRect = GUI.Window(0, windowRect, DrawLobby, "Multiplayer Lobby");
            //DrawLobby(1);
        }
    }


    void DrawConnectScreen(int windowID)
    {
        // let the user enter their username
        GUI.Label(new Rect(20, 30, 200, 20), "Username" );
        username = GUI.TextField(new Rect(20, 50, 200, 30), username, 20);

        if (GUI.Button(new Rect(10, 100, 400, 40), "Play Online"))
        {
            gameSettings.offlineMode = false;
            Connect();
        }

        if (GUI.Button(new Rect(10, 160, 400, 40), "Play Local"))
        {
            //Start local game
            gameSettings.offlineMode = true;
            StartLocal();
        }
    
    }


    void DrawLoadScreen(int windowID)
    {
        //Message
        GUI.Label(new Rect(20, 115, 100, 20), "Loading...");

    }



    void DrawLobby(int windowID)
    {
        //GUI.Label(new Rect(200, 25, 100, 30), "Rooms");
        GUI.Box(new Rect(10, 20, 380, 350),"");
        //GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

        scrollPosition = GUI.BeginScrollView(new Rect(20, 40, 330, 190), scrollPosition, new Rect(0, 0, 300, 850), false, true);
        
        // no rooms available
        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            // display a message informing the user that there aren't any rooms available to join
            //Message
            GUI.Label(new Rect(20, 50, 300, 20), "No Rooms Available");
            

            //Test scroll bar positioning
            //int i = 0;
            //foreach (string room in testRoomList)
            //{
                
            //    float yPos = 10 + 40 * i;
            //    if (GUI.Button(new Rect(20, yPos, 100, 20), "Join"))
            //    {
            //        Debug.Log("Button pressed: " + i);
            //        PhotonNetwork.JoinRoom(room);
            //    }
            //    GUI.Label(new Rect(120, yPos, 200, 40), room);
            //    //GUI.Button(new Rect(240, yPos, 100, 20), "Join");
            //    i++;
            //}
        }
        // draw each room in a scroll view
        else
        {
            int i = 0;
            foreach (RoomInfo room in PhotonNetwork.GetRoomList())
            {
                float yPos = 10 + 40 * i;
                // draw the room info to the GUI
                if (GUI.Button(new Rect(20, yPos, 100, 20), "Join"))
                {
                    Debug.Log("Button pressed: " + i);
                    PhotonNetwork.JoinRoom(room.name);
                }
                GUI.Label(new Rect(120, yPos, 200, 40), room.name);
                i++;

            }

        }

        GUI.EndScrollView();

        //if (PhotonNetwork.room == null)
        //{
        if (GUI.Button(new Rect(20, 300, 200, 40), "Create New Room"))
        {
            CreateRoom();
        }
        //}
    }

    void Connect()
    {
        // remember username for next time
        PlayerPrefs.SetString("Username", username);

        // set username, connect to photon
        PhotonNetwork.playerName = username;

        // connect to Photon
        PhotonNetwork.ConnectUsingSettings("v1.0");
        PhotonNetwork.automaticallySyncScene = true;    // ensure that all players play on the same map
        connecting = true;
    }

    void StartLocal()
    {
        // remember username for next time
        PlayerPrefs.SetString("Username", username);

        // set username, connect to photon
        PhotonNetwork.playerName = username;
        PhotonNetwork.LoadLevel("SimpleBoard2");    //Load the board in new room
    }

    void CreateRoom()
    {
        // create a room with a random name
        string room = "HiveGame" + Random.Range(0, 999).ToString();
        PhotonNetwork.CreateRoom(room, roomOptions, TypedLobby.Default);
    }

    void OnPhotonCreateRoomFailed()
    {
        Debug.Log("Error: Room Create Failed");
    }

    void OnCreatedRoom()
    {
        Debug.Log("Room Created Successfully: " + PhotonNetwork.room.name);
        //Set player color
        if (PhotonNetwork.playerList.GetLength(0) == 1) //If this player is first to join make them white
        {
            PhotonNetwork.player.SetTeam(PunTeams.Team.red);   //Red = white
        }
        else if (PhotonNetwork.playerList.GetLength(0) == 2)    //If this player is second to join make them black
        {
            PhotonNetwork.player.SetTeam(PunTeams.Team.blue);   //Blue = black
        }
        

        PhotonNetwork.LoadLevel("SimpleBoard2");    //Load the board in new room
    }

    void OnPhotonJoinRoomFailed()
    {
        Debug.Log("Error: Room Join Failed");
    }

    void OnJoinedRoom()
    {
        Debug.Log("Room Joined: " + PhotonNetwork.room.name);
        //Do stuff here. Load the game etc.
    }


}
