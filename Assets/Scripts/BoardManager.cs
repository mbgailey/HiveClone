using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BoardManager : MonoBehaviour {
    //Keeps track of which piece is at what location
    public LayerMask pieceLayers;

    public enum UnitColor { White, Black };
    public enum UnitType { Queen, Spider, Ant, Beetle, Grasshopper };
    public enum Active { Dugout, Board};

    int totalHexes = 469;
    //Vector2[] coordinateList;   //List of each hex q,r coordinates in order
    public HexCoord[] HexBoard;         //List of hex coord objects in same order
    //UnitColor[] colorList;      //List of piece colors in same order
    //UnitType[] typeList;        //List of piece types in same order
    [HideInInspector]   public List<HexCoord> occupiedList = new List<HexCoord>(); //Generic List to hold all occupied hexes
    Dictionary<HexCoord, UnitColor> colorDict = new Dictionary<HexCoord, UnitColor>(); //Generic Dictionary to hold the piece color for occupied hexes
    Dictionary<HexCoord, UnitType> typeDict = new Dictionary<HexCoord, UnitType>();
    [HideInInspector]   public Dictionary<HexCoord, int> beetleDict = new Dictionary<HexCoord, int>();   //Generic dictionary to hold how many beetles are on top of a given unit
    [HideInInspector]   public List<GameObject> queenList = new List<GameObject>(); //List containing both queen gameobjects
    public List<HexCoord> validMovePieces = new List<HexCoord>();
    public Dictionary<string, HexCoord> boardDict = new Dictionary<string, HexCoord>(); //Generic Dictionary to hold location of all pieecs. This is used for board saving and loading.
    public List<string> boardList = new List<string>();         //Generic List to hold location of all pieecs. This is used for board saving and loading.
    // wq1 = white queen 1
    // bs3 = black spider 3
    public List<UnitType> whitePieceList = new List<UnitType>(); //Generic List to hold all white pieces. Used for registering
    public List<UnitType> blackPieceList = new List<UnitType>(); //Generic List to hold all black pieces. Used for registering
    //HexController hexController;

    public GameObject lastPieceMoved;

	// Use this for initialization
	void Awake () {   
        //coordinateList = new Vector2[totalHexes];
        HexBoard = new HexCoord[totalHexes];
        //colorList = new UnitColor[totalHexes];
        //typeList = new UnitType[totalHexes];
	}


    void Start()
    {
        //hexController = GameObject.FindGameObjectWithTag("GameController").GetComponent<HexController>();
    }
	// Used when a piece first enters the board to populate the type and color lists. Return piece number (if the second white spider then returns 2)
	public void RegisterPiece (HexCoord currentHex, UnitColor color, UnitType type) {
        //int index = GetIndex(currentHex);

        //colorList[index] = color;
        //typeList[index] = type;

        occupiedList.Add(currentHex);
        colorDict[currentHex] = color;
        typeDict[currentHex] = type;
        beetleDict[currentHex] = 0; //No beetles on top

	}

    public int DugoutRegistration(HexCoord currentHex, UnitColor color, UnitType type)
    {
        int num = 1;
        if (color == UnitColor.White)
        {
            foreach (UnitType member in whitePieceList)
            {
                if (member == type)
                {
                    num++;
                }
            }
            whitePieceList.Add(type);
        }
        if (color == UnitColor.Black)
        {
            foreach (UnitType member in blackPieceList)
            {
                if (member == type)
                {
                    num++;
                }
            }
            blackPieceList.Add(type);
        }

        boardList.Add(PieceStringHelper(color, type, num));
        boardDict[PieceStringHelper(color, type, num)] = currentHex;

        return num;
    }

    string PieceStringHelper(UnitColor color, UnitType type, int num)
    {
        string str ="";

        //Start with color
        if (color == UnitColor.White)
        {
            str += "w";
        }
        else
        {
            str += "b";
        }

        //Append unit type
        switch (type)
        {
            case UnitType.Ant:
                str += "a";
                break;
            case UnitType.Beetle:
                str += "b";
                break;
            case UnitType.Grasshopper:
                str += "g";
                break;
            case UnitType.Queen:
                str += "q";
                break;
            case UnitType.Spider:
                str += "s";
                break;
        }
        //Append number
        str += num.ToString();

        return str;
    }

    // Used when a piece moves to update the lists
    public void MovePiece(HexCoord prevHex, HexCoord newHex, UnitColor color, UnitType type, GameObject obj)
    {
        if (type == UnitType.Beetle) //If beetle then we need to update lists differently
        {
            if (occupiedList.Contains(newHex)) //If beetle is moving on top of another piece
            {
                beetleDict[newHex]++;   //Add beetle on top
                CoverPieceBelow(obj.transform.position);
                //Don't add beetle to occupied list since hex is already occupied
            }
            else //If new hex is unoccupied then add beetle to occupied list
            {
                occupiedList.Add(newHex);
                colorDict[newHex] = color;
                typeDict[newHex] = type;
            }
            if (beetleDict[prevHex] > 0)    //If moving a beetle from a stacked position
            {
                beetleDict[prevHex]--;      //Decrement beetle count at previous hex
                UncoverPieceBelow(new Vector3(prevHex.Position().x, prevHex.Position().y, 5.0f)); //Define ray starting point far enough above prev position
            }
            else  //Only remove prevHex from lists if no other piece is at previous position
            {
                occupiedList.Remove(prevHex);
                colorDict.Remove(prevHex);
                typeDict.Remove(prevHex);
            }
        }
        else //If any other move type then change lists and dict as normal;
        {
            occupiedList.Remove(prevHex);
            colorDict.Remove(prevHex);
            typeDict.Remove(prevHex);
            beetleDict.Remove(prevHex);

            occupiedList.Add(newHex);
            colorDict[newHex] = color;
            typeDict[newHex] = type;
            beetleDict[newHex] = 0;
        }

    }

    int GetIndex(HexCoord hex)
    {
        int index = 0;
        foreach (HexCoord hc in HexBoard)
        {
            if (hc == hex)
            {
                break;
            }
            index++;
        }
        return index;
    }


    //Run eligible placement function to get all outer hexes. Then remove any hexes that neighbor an opposing. Input is the color of the piece that's being placed
    public List<HexCoord> EligiblePlacementByColor(UnitColor color)
    {
        List<HexCoord> eligibleList = EligiblePlacement();
        List<HexCoord> eligibleListMod = new List<HexCoord>(eligibleList);
        UnitColor enemyColor;
        if (color == UnitColor.White)
            enemyColor = UnitColor.Black;
        else
            enemyColor = UnitColor.White;
        
        //Iterate through occupied list and find all neighbors. Add them to the possibly eligible list
        foreach (HexCoord hc in eligibleList) 
        {
            foreach (HexCoord adj in hc.Neighbors())    //Check all neighbors of eligible cells 
            {
                if (occupiedList.Contains(adj))         //If a neighbor is occupied
                {
                    if (colorDict[adj] == enemyColor) //Check color of any occupied neighbor. If any are enemy color then hex is not eligible
                    {
                        if(eligibleListMod.Contains(hc))
                            eligibleListMod.Remove(hc);
                        //break;
                    }
                }
                
            }
        }
        //Debug.Log(eligibleList.Count);
        return eligibleListMod;
    }

    //Get all eligible hexes for new piece placement that touch a played piece but don't have a piece on them
    //This is the list of all hexes directly surrounding the pieces on the board
    public List<HexCoord> EligiblePlacement()
    {
        List<HexCoord> eligibleList = new List<HexCoord>(); 
        foreach (HexCoord hc in occupiedList)
        {
            foreach (HexCoord adj in hc.Neighbors())
            {
                if (!occupiedList.Contains(adj) && !eligibleList.Contains(adj)) //Don't add any occupied hexes or duplicates
                {
                    eligibleList.Add(adj);
                }
            }
        }
        return eligibleList;
    }

    //Get all eligible hexes for movement that touch a played piece but don't have a piece on them. Exclude the currently selected piece
    public List<HexCoord> GetOuterHexes(HexCoord start)
    {
        List<HexCoord> eligibleList = new List<HexCoord>();
        List<HexCoord> occupiedListMod = new List<HexCoord>(occupiedList);  //Copy list

        occupiedListMod.Remove(start);                                   //Exclude selected piece from neighbor search
        //Iterate through occupied start and find all neighbors. Add them to the possibly eligible list
        foreach (HexCoord hc in occupiedListMod)
        {
            foreach (HexCoord adj in hc.Neighbors())
            {
                if (!occupiedListMod.Contains(adj) && !eligibleList.Contains(adj)) //Don't add any occupied hexes or duplicates
                {
                    eligibleList.Add(adj);
                }
            }
        }
        //Finally, remove the selected hex from the list because you can't move to the same place that the piece started
        eligibleList.Remove(start);

        return eligibleList;
    }

    //Returns a list of eligible moves for a given piece
    public List<HexCoord> EligibleMovesByPiece(HexCoord start, UnitType type)
    {
        Debug.Log("Eligible Start " + start);
        //Start with outer hexes
        List<HexCoord> eligibleList = GetOuterHexes(start);
        List<HexCoord> eligibleListMod = new List<HexCoord>();    //Make a blank list
        switch (type)
        {
            case UnitType.Ant:
                //Copy eligible list. Ant can move to any outer position. 
                eligibleListMod = new List<HexCoord>(eligibleList);
                RemoveUnreachableHexes(eligibleListMod);                //Next remove unreachable locations.
                ////Currently I don't check to see whether a move would have the ant squeeze through a hole that's too small
                ////A way to implement this would be to check space by space in a move chain. If any space is unreachable then
                ////the chain would stop.
                break;
            
            case UnitType.Queen:
                //Queen can only move one hex away from start. Check for overlap between outer cells and neighbors to start
                foreach (HexCoord nh in start.Neighbors())
                {
                    if (eligibleList.Contains(nh) && !IsMoveGapJumping(start, start, nh))
                        eligibleListMod.Add(nh);
                }
                RemoveUnreachableHexes(eligibleListMod);                //Next remove unreachable locations.
                break;
            
            case UnitType.Beetle:
                //Beetles move one space, but can also move on top of other pieces
                //No restrictions on unreachable hexes
                foreach (HexCoord nh in start.Neighbors())
                {
                    if (beetleDict.ContainsKey(start))
                    {
                        if (beetleDict[start] > 0)      //If this is a beetle moving from on top of the hive, all neighbors will be eligible
                        {
                            eligibleListMod.Add(nh);
                        }

                        else if (eligibleList.Contains(nh) && !IsMoveGapJumping(start, start, nh))  //Add all unnoccupied neighbors that are eligible
                        {
                            eligibleListMod.Add(nh);
                        }
                        else if (occupiedList.Contains(nh))
                        {
                                eligibleListMod.Add(nh);    //Add all occupied neighbors
                        }
                    }
                    //Might not need to repeat these
                    else if (eligibleList.Contains(nh) && !IsMoveGapJumping(start, start, nh))  //Add all unnoccupied neighbors that are eligible
                    {
                        eligibleListMod.Add(nh);

                        if (occupiedList.Contains(nh))
                            eligibleListMod.Add(nh);    //Add all occupied neighbors
                    }
                }
                break;

            case UnitType.Spider:
                //Spiders move exactly 3 spaces following the outer edge of the hive
                //Use outer hex list as a starting point. All outer hexes at exactly 3 spaces from starting point should be eligible
                List<HexCoord> outerHexes = GetOuterHexes(start);

                List<HexCoord> iterList = new List<HexCoord>();
                List<HexCoord> firstSpaces = new List<HexCoord>();
                List<HexCoord> secondSpaces = new List<HexCoord>();
                List<HexCoord> thirdSpaces = new List<HexCoord>();

                for (int i = 1; i < 4; i++)
                {
                    switch (i)
                    {
                        case 1:    //For the first move check neighbors of the start hex
                            iterList.Add(start);
                            break;
                        case 2:     //For the second move check neighbors of each eligible first move space
                            iterList = new List<HexCoord>(firstSpaces);
                            
                            break;
                        case 3:     //For the third move check neighbors of each eligible second move space
                            iterList = new List<HexCoord>(secondSpaces);
                            break;
                    }

                    foreach (HexCoord hex in iterList)
                    {

                        foreach (HexCoord nh in hex.Neighbors())          //First find occupied neighbors
                        {
                            //space must be an outer hex and reachable and the move must not be jumping a gap
                            if (outerHexes.Contains(nh) && !IsHexUnreachable(nh) && !IsMoveGapJumping(start,hex,nh)) 
                            {
                                switch (i)
                                {
                                    case 1:
                                        firstSpaces.Add(nh);
                                        break;
                                    case 2:
                                        secondSpaces.Add(nh);
                                        break;
                                    case 3:
                                        thirdSpaces.Add(nh);
                                        break;
                                }
                            }  
                        }
                    }

                }
                
                eligibleListMod = new List<HexCoord>(thirdSpaces);      //All eligible third spaces are the full eligible list
                //Remove all first spaces and second spaces. This is an attempt to eliminate back tracking.
                //Since backtracking isn't allowed I don't think there's any way that any 
                //first space could also be a valid third space. Same reasoning applies to second spaces
                //although I'm not as sure about the second spaces. Needs testing
                foreach (HexCoord spc in firstSpaces)
                    eligibleListMod.Remove(spc);
                foreach (HexCoord spc in secondSpaces)
                    eligibleListMod.Remove(spc);

                break;

            case UnitType.Grasshopper:
                //Grasshopper jumps in a straight line over other pieces
                //No restrictions on unreachable hexes
                int neighborIndex = 0;
                HexCoord option;
                foreach (HexCoord nh in start.Neighbors())          //First find occupied neighbors
                {
                    if (occupiedList.Contains(nh))                  //If direct neighbor is occupied 
                    {
                        option = nh;                                //Initialize option
                        bool solutionFound = false;                 //Initialize bool
                        while (!solutionFound)                      //Repeat until solution is found
                        {
                            option = option.Neighbor(neighborIndex);    //Option might be next neighbor in same direction

                            if (!occupiedList.Contains(option))     //Check if occupied. 
                            {
                                eligibleListMod.Add(option);        //If not occupied then add to eligible list
                                solutionFound = true;
                            }
                                                                    //If occupied then keep looking in the same direction. Stay in while loop
                        }
                    }
                    neighborIndex++;
                }
                break;

            default:
                eligibleListMod = eligibleList;
                break;

            
        }
       
        return eligibleListMod;
    }

    //Remove unreachable locations that are completely surrounded or only have one open space ///This doesn't remove more complex unreachable areas (like a multiple hex open area that is surrounded)
    void RemoveUnreachableHexes(List<HexCoord> eligibleList)
    {
        List<HexCoord> eligibleListMod = new List<HexCoord>(eligibleList);

        foreach (HexCoord hc in eligibleListMod)
        {
            int neighborCount = CountOccupiedNeighbors(hc);
            
            if (neighborCount >= 5)
            {
                eligibleList.Remove(hc);
            }
        }
        //return eligibleList;
    }

    //Check if a hex is unreachable (completely surrounded or only have one open space next to it) ///This doesn't remove more complex unreachable areas (like a multiple hex open area that is surrounded)
    bool IsHexUnreachable(HexCoord testHex)
    {
        bool unreachable = false;

        int neighborCount = CountOccupiedNeighbors(testHex);
        
        if (neighborCount >= 5)
        {
            unreachable = true;
        }
        
        return unreachable;
    }

    //Checks whether moving a piece from start hex to test hex will be jumping a gap or not. 
    //I.e. Most pieces can't make a move that would have them not be in contact with the hive 
    //during the move
    bool IsMoveGapJumping(HexCoord initial, HexCoord current, HexCoord test)
    {
        bool gapJumped = true;      //Initialize as true. Must be proved to be false
        List<HexCoord> currentNeighbors = new List<HexCoord>();
        List<HexCoord> testNeighbors = new List<HexCoord>();
        List<HexCoord> occupiedListMod = new List<HexCoord>(occupiedList);  //Copy list
        occupiedListMod.Remove(initial);                                   //Exclude piece in question from neighbor search
        
        //Find all neighbor pieces at current location
        foreach (HexCoord nh in current.Neighbors())
        {
            if (occupiedListMod.Contains(nh))   //If neighbor hex is occupied
            {
                currentNeighbors.Add(nh);       //Add to current neighbors list
            }
        }

        //Find all neighbor pieces at current location
        foreach (HexCoord nh in test.Neighbors())
        {
            if (occupiedListMod.Contains(nh))   //If neighbor hex is occupied
            {
                testNeighbors.Add(nh);       //Add to test neighbors list
            }
        }

        //If no neighbors are the same at new location then the piece WOULD be jumping a gap
        foreach (HexCoord hex in testNeighbors)
        {
            if (currentNeighbors.Contains(hex))
            {
                gapJumped = false;
            }
        }

        return gapJumped;
    }

    //Undo last move
    public void UndoLastMove()
    {
        if (lastPieceMoved != null)
        {
            lastPieceMoved.GetComponent<GeneralPieceMovement>().UndoMove();
        }
    }

    //When selecting a piece check to see if a piece has already been moved this turn. If it has, return it to it's starting position
    public void SelectingNewPiece()
    {
        if (lastPieceMoved != null)
        {
            UndoLastMove();
        }
    }

    //Checks whether queen has been placed yet for a given color
    public bool HasQueenBeenPlaced(UnitColor color)
    {
        bool queenPlaced = false;

        foreach (HexCoord hex in occupiedList)
        {
            if (colorDict[hex] == color && typeDict[hex] == UnitType.Queen)
            {
                queenPlaced = true;
            }
        }

        return queenPlaced;
    }

    //Checks whether either queen of certain color is surrounded
    public bool IsQueenSurrounded(UnitColor color)
    {
        bool queenSurrounded = false;
        Debug.Log("Checking for Queen: " + color.ToString());
        foreach (HexCoord hex in occupiedList)
        {
            if (colorDict[hex] == color && typeDict[hex] == UnitType.Queen)
            {
                int neighborCount = CountOccupiedNeighbors(hex);
                Debug.Log("Queen neighbors: " + neighborCount);
                if (neighborCount == 6)
                {
                    queenSurrounded = true;
                }
                break;
            }
        }
        return queenSurrounded;
    }

    int CountOccupiedNeighbors(HexCoord hex)
    {
        int neighborCount = 0;
        foreach (HexCoord nh in hex.Neighbors())
        {
            if (occupiedList.Contains(nh))
            {
                neighborCount++;
            }
        }
        return neighborCount;
    }


    public void DetermineMoveablePieces(UnitColor color)
    {
        if (HasQueenBeenPlaced(color))
        {
            CheckHiveContinuity();      //Populate the validMovePieces list based on maintaining hive continuity
        }
        else
        {
            validMovePieces.Clear();    //If queen hasn't been placed for this color then no pieces can be moved
        }
    }

    //Check hive continuity. Return pieces that can move without disrupting the hive continuity
    public void CheckHiveContinuity()
    {
        validMovePieces.Clear(); //
        foreach (HexCoord testHex in occupiedList)  //For every occupied cell
        {
            //Check each piece is hive continuous without this piece?
            //Choose any piece in the occupied list except for the chosen piece. Find continuous island from random piece
            //Debug.Log("Test Hex" + testHex);
            List<HexCoord> occupiedListMod = new List<HexCoord>(occupiedList);
            occupiedListMod.Remove(testHex);
            List<HexCoord> island = new List<HexCoord>(); // Create a new island list or reset;

            if (occupiedListMod.Count != 0)
            {
                bool solutionFound = false;                   //Initialize bool
                HexCoord hex = occupiedListMod[0];            //Start with first piece in occupied list
                island.Add(hex);                              //Start by adding first hex
                List<HexCoord> hexes = new List<HexCoord>(); // Create a list of hexes to check neighbors of in the while loop
                hexes.Add(hex);
                while (!solutionFound)                        //Repeat until entire island is found
                {
                //Look for occupied neighbors. Add any new ones to the island list. Then check neighbors of occupied neighbors, etc.

                    foreach (HexCoord neighbor in hexes[0].Neighbors()) //Check neighbors of first hex in hexes list
                    {
                        //If direct neighbor is occupied and island list doesn't already have that hex, then add it to the list
                        if (occupiedListMod.Contains(neighbor) && !island.Contains(neighbor))
                        {
                            hexes.Add(neighbor);    //Add this to the list to check in the loop
                            island.Add(neighbor);       //Add occupied neighbors to the island list
                        }
                    }

                    hexes.RemoveAt(0);  //After checking neighbors, remove from list so its not checked again

                    if (hexes.Count == 0) { solutionFound = true; }   //If hexes is empty then we've found a solution.
                }

                //Debug.Log("Island Count: " + island.Count);
                if (island.Count == occupiedList.Count - 1) //Full hive was continuous
                {
                    validMovePieces.Add(testHex);       //Add testHex to valid move pieces list
                }
            }
            
        }
        //Debug.Log("Valid move pieces: " + validMovePieces.Count);
    }

    public void CheckHiveContinuity2()
    {
        validMovePieces.Clear(); //
        
        foreach (HexCoord hex in occupiedList)  //For every occupied cell
        {
            //Debug.Log("Test Hex" + hex);
            //Test whether this piece could be moved by checking hive with out it
            if (beetleDict[hex] == 0)   //Only consider pieces that aren't covered by a beetle
            {
                //Is hive continuous without this piece?
                //Choose any piece in the occupied list except for the chosen piece. Find continuous island from random piece
                List<HexCoord> occupiedListMod = new List<HexCoord>(occupiedList);
                occupiedListMod.Remove(hex);
                List<HexCoord> island = new List<HexCoord>(); // Create a new island list or reset;

                if (occupiedListMod.Count != 0)
                {

                    foreach (HexCoord neighbor in occupiedListMod[0].Neighbors())
                    {
                        if (occupiedListMod.Contains(neighbor))                  //If direct neighbor is occupied 
                        {
                            
                            if (!island.Contains(neighbor))
                            {
                                island.Add(neighbor);                       //Start by adding the neighbor
                            }
                            
                            bool solutionFound = false;                   //Initialize bool

                            while (!solutionFound)                        //Repeat until entire island is found
                            {
                                HexCoord nh = neighbor;                     //Initialize nh variable
                                List<HexCoord> newNeighborList = new List<HexCoord>();
                                foreach (HexCoord secondNeighbor in nh.Neighbors())
                                {
                                    //If direct neighbor is occupied and island list doesn't already have that hex, then add it to the list
                                    if (occupiedListMod.Contains(secondNeighbor) && !island.Contains(secondNeighbor))
                                    {
                                        island.Add(secondNeighbor);       //Add occupied neighbors to the island list
                                        nh = secondNeighbor;        //If a new neighbor was found set nh to new neighbor and repeat loop
                                        newNeighborList.Add(secondNeighbor);
                                    }
                                }

                                if (newNeighborList.Count == 0) { solutionFound = true; }   //If there were no new neighbors then entire connected island has been found
                            }

                           
                            //Check island size. Is it equal to total number of occupied hexes - 1? 
                            //If so, that particular branch meets hive continuity. Add piece to eligible list
                            //If not, then hive would be split. 
                            //This piece is not eligible
                            //Note, only need to check first occupied neighbor that is found. Then break to next piece
                            //break;
                        }

                    }

                    Debug.Log("Island Count: " + island.Count);
                    if (island.Count == occupiedList.Count - 1) //Full hive was continuous
                    {
                        validMovePieces.Add(hex);
                    }

                }

            }
        }

        Debug.Log("Valid move pieces: " + validMovePieces.Count);

    }

    //Sets piece directly below point to covered = true
    void CoverPieceBelow(Vector3 pos)
    {
        Ray ray= new Ray(pos, Vector3.back);    //Ray should go in negative z direction to hit pieces below
        RaycastHit rch;
        
        if (Physics.Raycast(ray, out rch, Mathf.Infinity, pieceLayers))
        {
            rch.transform.GetComponent<SpecificBehavior>().covered = true;
        }     
    }

    //Sets piece directly below point to covered = false
    void UncoverPieceBelow(Vector3 pos)
    {
        Ray ray = new Ray(pos, Vector3.back);
        RaycastHit rch;
        if (Physics.Raycast(ray, out rch, Mathf.Infinity, pieceLayers))
        {
            rch.transform.GetComponent<SpecificBehavior>().covered = false;
        }
    }

    public void InitializeBoard()
    {

    }

    //Format for board properties is list of occupied hexes in this format:
    //HexID
    //Piece color
    //Piece type
    //If beetle stacked. "ST" This can show multiple pieces stacked
    //Beetle color
    //

    public void GetPieceLocations()
    {
        GameObject[] whitePcs = GameObject.FindGameObjectsWithTag("WhitePieces");
        GameObject[] blackPcs = GameObject.FindGameObjectsWithTag("BlackPieces");

        foreach (GameObject pc in whitePcs)
        {
            //pc.GetComponent<>();
        }

    }

    protected internal Hashtable GetBoardAsCustomProperties()
    {
        Hashtable customProps = new Hashtable();
        
                
        return customProps;
    }


    protected internal bool SetBoardByCustomProperties(Hashtable customProps, bool calledByEvent)
    {
        


        return true;
    }
}
