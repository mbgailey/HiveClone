using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
//using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class SaveGameInfo
{
    public int MyPlayerId;
    public string RoomName;
    public string DisplayName;
    public bool MyTurn;
    public Dictionary<string, object> AvailableProperties;

    public string ToStringFull()
    {
        return string.Format("\"{0}\"[{1}] {2} ({3})", RoomName, MyPlayerId, MyTurn, SupportClass.DictionaryToString(AvailableProperties));
    }
}


public class HiveBoardClient : MonoBehaviour
{

    public BoardManager board;
    public const string PropTurn = "turn";
    public const string PropNames = "names";

    private const byte MaxPlayers = 2;
     
    public int TurnNumber = 1;
    
    public int PlayerIdToMakeThisTurn;  // who's turn this is. when "done", set the other player's actorNumber and save

    public bool IsMyTurn
    {
        get
        {
            //Debug.Log(PlayerIdToMakeThisTurn + "'s turn. You are: " + this.LocalPlayer.ID); 
            return this.PlayerIdToMakeThisTurn == PhotonNetwork.player.ID;
        }
    }

    public List<SaveGameInfo> SavedGames = new List<SaveGameInfo>();

    

	// Use this for initialization
	void Start () {
	
	}

    
    public void SaveBoardToProperties()
    {
        Hashtable boardProps = board.GetBoardAsCustomProperties();
        boardProps.Add("pt", this.PlayerIdToMakeThisTurn);  // "pt" is for "player turn" and contains the ID/actorNumber of the player who's turn it is
        boardProps.Add("t#", this.TurnNumber);

        //boardProps.Add(GetPlayerPointsPropKey(this.LocalPlayer.ID), this.MyPoints); // we always only save "our" points. this will not affect the opponent's score.
        
        //Debug.Log(string.Format("saved board to room-props {0}", SupportClass.DictionaryToString(boardProps)));
        PhotonNetwork.room.SetCustomProperties(boardProps);
    }

    //public void SavePlayersInProps()
    //{
    //    if (this.CurrentRoom == null || this.CurrentRoom.CustomProperties == null || this.CurrentRoom.CustomProperties.ContainsKey(PropNames))
    //    {
    //        Debug.Log("Skipped saving names. They are already saved.");
    //        return;
    //    }

    //    Debug.Log("Saving names.");
    //    Hashtable boardProps = new Hashtable();
    //    boardProps[PropNames] = string.Format("{0};{1}", this.LocalPlayer.NickName, this.Opponent.NickName);
    //    this.OpSetCustomPropertiesOfRoom(boardProps, false);
    //}

    //public void LoadBoardFromProperties(bool calledByEvent)
    //{
    //    //board.InitializeBoard();

    //    Hashtable roomProps = this.CurrentRoom.CustomProperties;
    //    Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));

    //    if (roomProps.Count == 0)
    //    {
    //        // we are in a fresh room with no saved board.
    //        board.InitializeBoard();
    //        board.RandomBoard();
    //        this.SaveBoardToProperties();
    //    }


    //    // we are in a game that has props (a board). read those (as update or as init, depending on calledByEvent)
    //    bool success = board.SetBoardByCustomProperties(roomProps, calledByEvent);
    //    if (!success)
    //    {
    //        Debug.LogError("Not loaded board from props?");
    //    }


    //    // we set properties "pt" (player turn) and "t#" (turn number). those props might have changed
    //    // it's easier to use a variable in gui, so read the latter property now
    //    if (this.CurrentRoom.CustomProperties.ContainsKey("t#"))
    //    {
    //        this.TurnNumber = (int)this.CurrentRoom.CustomProperties["t#"];
    //    }
    //    else
    //    {
    //        this.TurnNumber = 1;
    //    }

    //    if (this.CurrentRoom.CustomProperties.ContainsKey("pt"))
    //    {
    //        this.PlayerIdToMakeThisTurn = (int)this.CurrentRoom.CustomProperties["pt"];
    //        //Debug.Log("This turn was played by player.ID: " + this.PlayerIdToMakeThisTurn);
    //    }
    //    else
    //    {
    //        this.PlayerIdToMakeThisTurn = 0;
    //    }

    //    // if the game didn't save a player's turn yet (it is 0): use master
    //    if (this.PlayerIdToMakeThisTurn == 0)
    //    {
    //        this.PlayerIdToMakeThisTurn = this.CurrentRoom.MasterClientId;
    //    }

    //    this.MyPoints = GetPlayerPointsFromProps(this.LocalPlayer);
    //    this.OthersPoints = GetPlayerPointsFromProps(this.Opponent);
    //}
}
