using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

public class SpecificBehavior : MonoBehaviour
{
    public BoardManager.UnitColor color = BoardManager.UnitColor.White;
    public BoardManager.UnitType type = BoardManager.UnitType.Queen;
    public BoardManager.Active active = BoardManager.Active.Dugout;
    
    BoardManager boardManager;
    HexController hexController;
    public HexCoord currentHex;
    public List<HexCoord> eligibleList = new List<HexCoord>();

    public bool covered;

	// Use this for initialization
	void Start () {
        hexController = GameObject.FindGameObjectWithTag("GameController").GetComponent<HexController>();
        boardManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<BoardManager>();

        covered = false;

        //If already on the board, register piece. Only use this during development
        if (active == BoardManager.Active.Board)
            NewPiecePlacement();
	}

    //Used after any piece is placed on the board to register it properly
    public void NewPiecePlacement()
    {
        GetCurrent();
        boardManager.RegisterPiece(currentHex, color, type);
        active = BoardManager.Active.Board;
    }

	// Determine eligible hex cells for movement or placement. Used if this isn't the first or second placement of the game
	public void GetEligible () {
        GetCurrent();
        eligibleList.Clear();
        if (active == BoardManager.Active.Dugout)   //If moving from the dugout
        {
            eligibleList = boardManager.EligiblePlacementByColor(color);
            
        }
        else                                        //If moving on the board
        {
            eligibleList = boardManager.EligibleMovesByPiece(currentHex, type);
        }
        //foreach (HexCoord hex in eligibleList) 
            //hexController.OutlineHex(hex);
        
        hexController.HighlightEligible(eligibleList);

	}

    //Used for the very first placement of the game. Only hex 0,0 is eligible;
    public void GetEligibleFirstPlacement()
    {
        eligibleList.Clear();
        HexCoord originHex = HexCoord.AtPosition(new Vector2(0, 0));
        eligibleList.Add(originHex);   //Add hexcoord 0,0
        hexController.HighlightEligible(eligibleList);
    }

    //Used for the second placement of the game. Hexes neighboring 0,0 are eligible;
    public void GetEligibleSecondPlacement()
    {
        eligibleList.Clear();
        HexCoord originHex = HexCoord.AtPosition(new Vector2(0, 0));
        foreach (HexCoord nh in originHex.Neighbors())
        {
            eligibleList.Add(nh);   //Add all neighbors
        }  
        
        hexController.HighlightEligible(eligibleList);
    }

    void GetCurrent()
    {
        currentHex = hexController.GetNearestHex((Vector2)this.transform.position);
        //Debug.Log("CurrentHex: " + currentHex);
    }

    //Check if piece is moveable (not covered and on the valid pieces list)
    public bool IsPieceMoveable()
    {
        bool moveable = false;
        GetCurrent();   //Get current hex
        
        if(type == BoardManager.UnitType.Beetle && !covered && boardManager.beetleDict[currentHex] > 0)    //If piece is a beetle then we have to consider that it might be moving from a covering position
        {
            moveable = true;
        }
        else if (!covered && boardManager.validMovePieces.Contains(currentHex)) //Otherwise, if not covered and on the moveable list then piece is valid
        {
            moveable = true;
        }
        return moveable;
    }
}
