using UnityEngine;
using System.Collections;
using Settworks.Hexagons;



public class CreateHexBoard : MonoBehaviour {

    HexCoord test1;

    public int columns = 5;
    public int rows = 5;
    public bool debugDisplay = false;
    int total;
    HexCoord[] HexBoard;
    
    public GameObject hexDisplayPrefab;
    public HexController hexController;
    public BoardManager boardManager;

	// Use this for initialization
	void Start () {

        total = 469;            //Hard code the total number of hexes for now. 469 is for hex board with radius of 12
        HexBoard = new HexCoord[total];
        int index = 0;

        int startRow = 0;
        int endRow = rows;

        for (int i = -columns; i < columns+1; i++)
        {
            for (int j = startRow; j < endRow + 1; j++)
            {
                
                HexCoord newHex = new HexCoord(i, j);
                HexBoard[index] = newHex;
                //Debug.Log(HexBoard[index].ToString());
                index++;
            }

            if (i < 0)
                startRow--;
            else
                endRow--;
        }
        //Debug.Log(index);

        
        InitializeHexes ();
        
	}
	

	void InitializeHexes () {

        GameObject empt = new GameObject("HexDisplays");

        for (int i = 0; i < HexBoard.Length; i++)
        {
            Vector2 pos = HexBoard[i].Position();
            GameObject disp = (GameObject)Instantiate(hexDisplayPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            HexDisplayControl dispControl = disp.gameObject.GetComponent<HexDisplayControl>();
            
            dispControl.SetText(HexBoard[i].ToString());
            if (debugDisplay)
                dispControl.DebugDisplayOn();
            else
                dispControl.DebugDisplayOff();
            dispControl.Highlight(false);
            disp.transform.parent = empt.transform;
            //yield return new WaitForSeconds(0.02f);
        }

        
        hexController.HexDisplays = empt;              //Store the parent of the hex displays

        InitializeManagers();

        //yield return null;
	}

    void InitializeManagers() {
        //Store hex board in HexController
        for (int i = 0; i < HexBoard.Length; i++)
        {
            hexController.HexBoard[i] = HexBoard[i];
            boardManager.HexBoard[i] = HexBoard[i];
        }
        
    }
}
