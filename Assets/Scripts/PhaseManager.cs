using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhaseManager : MonoBehaviour {

    public LayerMask pieceLayers;
    public LayerMask MoveLayer;
    public Camera dugoutCamera;

    string eligibleColor;
    public bool selectionMode = true;
    public bool dragMode = false;
    GameObject selectedPiece;
    BoardManager boardManager;
    HexController hexController;

    public Button endTurnButton;
    public Button undoButton;
    public Text statusText;

    int turnNumber = 1;
    bool onlyQueen = false;
    bool onlyPlacementAllowed = true;
    public bool gameover = false;

    private bool myTurn;
    private bool startOfTurn = false;

    //public enum Turn { White, Black };
    public enum Phase { FirstPlacement, SecondPlacement, PlaceOrMove };

    BoardManager.UnitColor whoseTurn;
    Phase phase;

	// Use this for initialization
	void Start () {
        boardManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<BoardManager>();
        hexController = GameObject.FindGameObjectWithTag("GameController").GetComponent<HexController>();
        whoseTurn = BoardManager.UnitColor.White;
        phase = Phase.FirstPlacement;
        eligibleColor = "WhitePiece";
        selectionMode = true;

        //Initialize GUI display
        DisableButtons();
        UpdateStatus("White's Turn");

        if (PhotonNetwork.isMasterClient)   //Master client is white and takes the first turn
        {
            myTurn = true;
        }
        else
            myTurn = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (startOfTurn && Input.anyKeyDown)
        {
            hexController.ClearLastMove();    //Clear all highlights after first action
            startOfTurn = false;
        }

        if (selectionMode && myTurn)
        {
            bool boardSelection = false;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray;
                if (dugoutCamera.pixelRect.Contains(Input.mousePosition))   //If mouse position is in the dugout region
                {
                    ray = dugoutCamera.ScreenPointToRay(Input.mousePosition);
                }
                else //Otherwise mouse is in the board region
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    boardSelection = true;
                }

                RaycastHit rch;

                if (Physics.Raycast(ray, out rch, Mathf.Infinity, pieceLayers))
                {
                    if (rch.transform.gameObject.tag.Equals(eligibleColor))  //Check that it's the eligible color for this turn
                    {
                        //If only queen is eligible then check that unit type is queen
                        if (!onlyQueen || (onlyQueen && rch.transform.gameObject.GetComponent<SpecificBehavior>().type == BoardManager.UnitType.Queen))
                        {
                            //If only placement is allow then make sure that selected piece is in the dugout
                            if (!onlyPlacementAllowed || (onlyPlacementAllowed && rch.transform.gameObject.GetComponent<SpecificBehavior>().active == BoardManager.Active.Dugout))
                            {
                                //Debug.Log(rch.transform.name);
                                selectedPiece = rch.transform.gameObject;
                                
                                if (phase == Phase.FirstPlacement)   //If this is the first placement of the game
                                {
                                    selectedPiece.transform.GetComponent<GeneralPieceMovement>().SelectPiece(1);
                                    selectionMode = false;
                                    dragMode = true;
                                }
                                else if (phase == Phase.SecondPlacement)    //If this is the second placement of the game
                                {
                                    selectedPiece.transform.GetComponent<GeneralPieceMovement>().SelectPiece(2);
                                    selectionMode = false;
                                    dragMode = true;
                                }

                                else if (boardSelection && selectedPiece.transform.GetComponent<SpecificBehavior>().IsPieceMoveable())  //For board selection, only allow piece to be selected if it is not covered and is on moveable list
                                {
                                    selectedPiece.transform.GetComponent<GeneralPieceMovement>().SelectPiece(3);
                                    selectionMode = false;
                                    dragMode = true;
                                }
                                else if (!boardSelection)   //If selecting from dugout, then don't need to check for valid moveable pieces
                                {
                                    selectedPiece.transform.GetComponent<GeneralPieceMovement>().SelectPiece(3);
                                    selectionMode = false;
                                    dragMode = true;
                                } 
                            }
                        }
                    }
                }

            }

        }

        //Follow the mouse while piece is selected
        if (dragMode)
        {
            if (!Input.GetMouseButtonUp(0))
            {

                Ray ray;
                if (dugoutCamera.pixelRect.Contains(Input.mousePosition))
                {
                    ray = dugoutCamera.ScreenPointToRay(Input.mousePosition);
                }
                else
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                }

                RaycastHit rch;

                if (Physics.Raycast(ray, out rch, Mathf.Infinity, MoveLayer))
                {
                    Vector3 loc = rch.point;

                    //Debug.Log(loc);
                    selectedPiece.transform.position = loc;
                }

            }

            //Drop piece
            else
            {
                selectedPiece.transform.GetComponent<GeneralPieceMovement>().UnSelectPiece();
                dragMode = false;
                //Keep selection mode false

            }
        }

	}

    //Once a valid move has been made. Enable the undo and next turn buttons
    public void EnableButtons()
    {
        endTurnButton.interactable = true;
        undoButton.interactable = true;
    }

    public void DisableButtons()
    {
        endTurnButton.interactable = false;
        undoButton.interactable = false;
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


    public void NextTurn()
    {
        Debug.Log("End Turn");

        boardManager.lastPieceMoved.transform.GetComponent<GeneralPieceMovement>().FinalizeMove();
        this.GetComponent<PhotonView>().RPC("NetworkSyncNextTurn", PhotonTargets.All);
        
    }

    //Execute next turn steps for all clients
    [PunRPC]
    public void NetworkSyncNextTurn()
    {
        if (!PhotonNetwork.offlineMode) //For offline mode leave my turn as true always
        {
            if (myTurn)
                myTurn = false;

            else
            {
                myTurn = true;
                startOfTurn = true;
            }
        }

        else //If in offline mode
        {
            startOfTurn = true;
        }
        
        dragMode = false;
        selectionMode = true;

        boardManager.lastPieceMoved = null; //Clear the last piece moved variable
        
        //hexController.ClearHighlights();    //Clear all highlights

        CheckVictoryConditions();

        if (!gameover)
        {

            if (phase == Phase.FirstPlacement || phase == Phase.SecondPlacement)
            {
                phase++;
            }

            if (whoseTurn == BoardManager.UnitColor.White)
            {
                whoseTurn = BoardManager.UnitColor.Black;
                eligibleColor = "BlackPiece";
                UpdateStatus("Black's Turn");
            }
            else
            {
                whoseTurn = BoardManager.UnitColor.White;
                eligibleColor = "WhitePiece";
                UpdateStatus("White's Turn");
                turnNumber++;
            }

            //If it's the fourth turn and the queen has not been placed
            if (turnNumber == 4 && !boardManager.HasQueenBeenPlaced(whoseTurn))
            {
                onlyQueen = true;
                Debug.Log("Queen must be placed this turn");
            }
            else
                onlyQueen = false;

            //Until the queen has been placed, no moves are allowed, only new placements
            if (turnNumber <= 4 && !boardManager.HasQueenBeenPlaced(whoseTurn))
            {
                onlyPlacementAllowed = true;
                Debug.Log("No moves allowed until queen has been placed");
            }
            else
                onlyPlacementAllowed = false;

            boardManager.DetermineMoveablePieces(whoseTurn);

            DisableButtons();
        }
    }

    public void UndoMove()
    {
        Debug.Log("UndoMove");
        
        selectionMode = true;

        boardManager.UndoLastMove();
        DisableButtons();                   //Don't allow buttons to be pressed until a valid move has been made
        boardManager.lastPieceMoved = null; //Clear the last piece moved variable
        hexController.ClearHighlights();    //Clear all highlights
    }

    //Check whether either (or both) queen is surrounded completely
    public void CheckVictoryConditions()
    {
        
        bool whiteSurrounded = boardManager.IsQueenSurrounded(BoardManager.UnitColor.White);
        bool blackSurrounded = boardManager.IsQueenSurrounded(BoardManager.UnitColor.Black);

        if (whiteSurrounded && blackSurrounded)
        {
            Debug.Log("GAMEOVER: Draw");
            gameover = true;
            UpdateStatus("Game Over\nDraw");
        }

        else if (whiteSurrounded)
        {
            Debug.Log("GAMEOVER: Black Wins");
            gameover = true;
            UpdateStatus("Game Over\nBlack Wins");
        }

        else if (blackSurrounded)
        {
            Debug.Log("GAMEOVER: White Wins");
            gameover = true;
            UpdateStatus("Game Over\nWhite Wins");
        }
    }

}
