using UnityEngine;
using System.Collections;
using Settworks.Hexagons;
using System.Collections.Generic;

public class GeneralPieceMovement : MonoBehaviour {

    public LayerMask MoveLayer;
    //float layerHt1 = 0.375f;
    float layerHt2 = 1.375f;
    HexController hexController;
    BoardManager boardManager;
    PhaseManager phaseManager;

    HexCoord currentHex;
    HexCoord startHex;

    public SpecificBehavior specificBehavior;

    //PieceSelection pieceSelection;

    bool selected = false;

	// Use this for initialization
	void Start () {
        
        hexController = GameObject.FindGameObjectWithTag("GameController").GetComponent<HexController>();
        boardManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<BoardManager>();
        phaseManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<PhaseManager>();
        specificBehavior = this.transform.GetComponent<SpecificBehavior>();
        currentHex = hexController.GetNearestHex(this.transform.position);
        
        //If already on the board, snap to nearest hex. Only use this during development
        //if (specificBehavior.active == BoardManager.Active.Board)
        
        SnapToNearest();
        this.transform.GetComponent<Rigidbody>().isKinematic = false;
	}

    void Update() {
        if (selected)
        {
            //Debug.Log("Piece Pos: " + this.transform.position);
            HighlightNearestHex();

        }
    
    }

    public void SelectPiece(int placementType)
    {
        Debug.Log("Piece Selected: " + this.transform.name);
        selected = true;
        
        startHex = currentHex;
        hexController.HighlightStart(currentHex);

        boardManager.SelectingNewPiece();
        
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, layerHt2);  //Move up to second layer
        this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        if (placementType == 1)
        {
            specificBehavior.GetEligibleFirstPlacement();
        }
        else if (placementType == 2)
        {
            specificBehavior.GetEligibleSecondPlacement();
        }
        else
        {
            specificBehavior.GetEligible();
        }

    }

    public void UnSelectPiece()
    {

        selected = false;
        this.transform.GetComponent<Rigidbody>().isKinematic = false;
        this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        //Snap to nearest eligible spot and drop to board. Clear eligible highlihgts
        SnapToNearestEligible(specificBehavior.eligibleList);
        hexController.ClearEligibleHighlights();

        //If new position is actually new 
        if (currentHex != startHex)// || specificBehavior.active == BoardManager.Active.Dugout)
        {
            hexController.OutlineHexTall(currentHex);   //Outline the current hex to show which piece moved
            //boardManager.MovePiece(startHex, currentHex, specificBehavior.color, specificBehavior.type, this.gameObject);
            boardManager.lastPieceMoved = this.gameObject;
            phaseManager.EnableButtons();
        }
        //If piece is placed back in the same location as it started
        else if (currentHex == startHex)
        {
            phaseManager.UndoMove();    //Let phase manager know that nothing happened. UndoMove essentially resets the phase manager
            
        }

        
    }

    //Highlight nearest hex
    void HighlightNearestHex()
    {
        hexController.HighlightNearest((Vector2)this.transform.position);
        
    }



    //Snap to the nearest eligible hex or the starting hex, whichever is closest
    void SnapToNearestEligible(List<HexCoord> eligibleList)
    {
        //Debug.Log(eligibleList.Count);
        //Brute force method to get nearest eligible hex
        float smallestDist = Mathf.Infinity;
        HexCoord closestHex = startHex;     //Default to the starting hex
        List<HexCoord> eligibleListMod = new List<HexCoord>(eligibleList);
        eligibleListMod.Add(startHex);      //Include the starting hex

        foreach (HexCoord hex in eligibleListMod)
        {
            float dist = Vector2.SqrMagnitude(hex.Position() - (Vector2)this.transform.position);

            if (dist < smallestDist){
                smallestDist = dist;
                closestHex = hex;   
            }
        }

        Vector2 snapPos = closestHex.Position();

        this.transform.position = snapPos;                      //Snap to x,y position first
        //this.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));    //Snap to default rotation

        //For beetles the eligible hex might be occupied. In that case, fix the z distance and don't allow gravity
        if (specificBehavior.type == BoardManager.UnitType.Beetle && boardManager.occupiedList.Contains(closestHex) && closestHex != startHex)
        {
            Vector3 temp = this.transform.position;
            int numBeetles = boardManager.beetleDict[closestHex];
            temp.z = 0.2f + 0.4f * (numBeetles+1);          //Position will scale up if there are multiple beetles
            this.transform.position = temp;
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        }
        
        currentHex = hexController.GetNearestHex(this.transform.position);

    }

    //Moves piece to target hex. Used for undoing moves and moving piece back to start position. Checks for beetle stacking at target hex
    void SnapToHex(HexCoord hex)
    {
        Vector3 snapPos = hex.Position();
        snapPos.z = 3.0f;                                       //Move up high first to avoid collisions with other pieces
        this.transform.position = snapPos;                      //Snap to x,y position of target hex
        //this.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));    //Snap to default rotation

        //For beetles the target hex might be occupied. In that case, fix the z distance and don't allow gravity. Also make sure it's a beetle that's coming from the board. If coming from the dugout then treat as a normal piece
        if (specificBehavior.type == BoardManager.UnitType.Beetle && specificBehavior.active == BoardManager.Active.Board)
        {
            if (boardManager.beetleDict[hex] > 0) //Check if there was a stacked beetle at target hex
            {
                Vector3 temp = this.transform.position;
                int numBeetles = boardManager.beetleDict[hex];
                temp.z = 0.2f + 0.4f * (numBeetles);          //Position will scale up if there are multiple beetles
                this.transform.position = temp;
                this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            else 
            {
                this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        
            }
        }
        // If piece is not a beetle then treat it normally
        else
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        }

        currentHex = hexController.GetNearestHex(this.transform.position);
    }


    //Used for initializing board
    void SnapToNearest()
    {
        Vector2 snapPos = hexController.FindNearest((Vector2)this.transform.position);
        this.transform.position = snapPos;                      //Snap to x,y position first
        //this.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));    //Snap to default rotation

        this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        currentHex = hexController.GetNearestHex(this.transform.position);
    }

    public void UndoMove()
    {
        hexController.ClearOutlines();
        //this.transform.position = (Vector2)startHex.Position();

        SnapToHex(startHex);

        //boardManager.MovePiece(currentHex, startHex, specificBehavior.color, specificBehavior.type, this.gameObject);
    }

    
    public void FinalizeMove_solo()
    {
        hexController.ClearOutlines();
        //If moving from the dugout
        if (specificBehavior.active != BoardManager.Active.Board)  
        {
            specificBehavior.NewPiecePlacement();    //Register piece properly as it's first placement
        }
        //If moving on the board 
        else                                             
        {
            boardManager.MovePiece(startHex, currentHex, specificBehavior.color, specificBehavior.type, this.gameObject);    //Make the move official in the board manager
        }
    }

    //[PunRPC]
    public void FinalizeMove()
    {
        bool placement = false;
        if(specificBehavior.active != BoardManager.Active.Board)
        {
            placement = true;
        }

        //PUN can't serialize HexCoords by default. Need to translate to something else or setup a serialize/deserialize method (see PUN documentation - search serialize)


        this.GetComponent<PhotonView>().RPC("NetworkSyncMove", PhotonTargets.All, this.transform.position, placement, startHex.Position());
                
    }

    [PunRPC]
    void NetworkSyncMove(Vector3 pos, bool placement, Vector2 strt)
    {
        //Translate start and current positions back to HexCoords
        HexCoord strtHex = hexController.GetNearestHex(strt);
        HexCoord currHex = hexController.GetNearestHex(pos);
       
        Debug.Log("Is Mine?: " + this.GetComponent<PhotonView>().isMine);
        if (this.GetComponent<PhotonView>().isMine)
        {
            //Take normal actions, piece has already been moved on this client
            hexController.ClearOutlines();
        }

        else
        {
            this.transform.position = pos;  //Move to new position
        }

        //If moving from the dugout
        if (placement)
        {
            specificBehavior.NewPiecePlacement();    //Register piece properly as it's first placement    

        }
        //If moving on the board 
        else
        {
            boardManager.MovePiece(strtHex, currHex, specificBehavior.color, specificBehavior.type, this.gameObject);    //Make the move official in the board manager
        }

        hexController.ShowLastMove(strtHex, currHex);
        
    }







    //NOT USED
    void DropToBoard()
    {
        Vector2 snapPos = hexController.FindNearest((Vector2)this.transform.position);
        this.transform.position = snapPos;                      //Snap to x,y position first
        this.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));    //Snap to default rotation

        currentHex = hexController.GetNearestHex(this.transform.position);  //Set currentHex
        hexController.ClearHighlights();
        this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;

    }

}
