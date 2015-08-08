using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Settworks.Hexagons;

public class HexController : MonoBehaviour {

    public HexCoord[] HexBoard;
    public GameObject HexDisplays;
    public GameObject outlinePrefab;
    public GameObject outlinePrefabTall;
    public GameObject highlightPrefab;
    GameObject startHighlight;
    GameObject inEligibleHighlight;
    GameObject outlineObject;
    List<GameObject> eligibleHighlights = new List<GameObject>();
    HexCoord nearestHex;
    HexCoord prevHighlightedHex;
    GameObject prevHighlight;
    GameObject prevHexDisp;

    BoardManager boardManager;

	// Use this for initialization
	void Awake () {
        int total = 469;            //Hard code the total number of hexes for now. 469 is for hex board with radius of 12
        HexBoard = new HexCoord[total];
	}

    void Start()
    {
        boardManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<BoardManager>();
    }

    public void HighlightStart(HexCoord startHex)
    {
        if (startHighlight != null)
            Destroy(startHighlight);

        startHighlight = (GameObject)Instantiate(highlightPrefab);
        startHighlight.GetComponent<HighlightController>().StartHex();
        startHighlight.transform.position = startHex.Position();
    }

    public void HighlightEligible(List<HexCoord> eligibleList)
    {
        //First clear any existing highlights
        foreach (GameObject obj in eligibleHighlights)
            Destroy(obj);
        
        if (eligibleHighlights.Count != 0)
            eligibleHighlights.Clear();
        
        foreach (HexCoord hex in eligibleList)
        {
            GameObject highlight = (GameObject)Instantiate(highlightPrefab);
            highlight.GetComponent<HighlightController>().EligibleHex();
            highlight.transform.position = hex.Position();
            if (boardManager.occupiedList.Contains(hex))    //For beetles the eligible hex might be occupied. In that case, raise the highlight
            {
                Vector3 temp = highlight.transform.position;

                int numBeetles = 0;
                if (boardManager.beetleDict.ContainsKey(hex))
                    numBeetles = boardManager.beetleDict[hex];
                
                temp.z = 0.4f * (numBeetles+1);          //Position will scale up if there are multiple beetles
                highlight.transform.position = temp;
            }
            eligibleHighlights.Add(highlight);
        }
        

    }

    public void HighlightNearest(Vector2 pos) 
    {
        nearestHex = HexCoord.AtPosition(pos);
        Vector2 hexPos = nearestHex.Position();

        if (nearestHex != prevHighlightedHex)
        {
            if (prevHighlight != null)
            {
                Destroy(prevHighlight);
            }
            GameObject highlight = (GameObject)Instantiate(highlightPrefab);
            highlight.GetComponent<HighlightController>().IneligibleHex();
            highlight.transform.position = hexPos;
            prevHighlight = highlight;
            prevHighlightedHex = nearestHex;
        }
    
    }

    public void ClearHighlights()
    {
        //Clear eligible highlights
        foreach (GameObject obj in eligibleHighlights)
            Destroy(obj);
        if (eligibleHighlights.Count != 0)
            eligibleHighlights.Clear();

        //Clear start highlight
        if (startHighlight != null)
            Destroy(startHighlight);

        //Clear all outlines
        if (outlineObject != null)
            Destroy(outlineObject);

        //Clear nearest hex highlight
        if (prevHighlight != null)
        {
            Destroy(prevHighlight);
        }
    }

    public void ClearEligibleHighlights()
    {
        //Clear eligible highlights
        foreach (GameObject obj in eligibleHighlights)
            Destroy(obj);
        if (eligibleHighlights.Count != 0)
            eligibleHighlights.Clear();

        //Clear nearest hex highlight
        if (prevHighlight != null)
        {
            Destroy(prevHighlight);
        }
    }

    public void ClearOutlines()
    {
        //Clear all outlines
        if (outlineObject != null)
            Destroy(outlineObject);
    }

    //NOT USED
	public void HighlightNearestDeprecated (Vector2 pos) {

        // Find the hex that overlaps a certain x,y point
        //Debug.Log(pos);
        nearestHex = HexCoord.AtPosition(pos);
        Vector2 hexPos = nearestHex.Position();
        GameObject hexDisp = null;

        //Brute force method to get hex display associated with a certain HexCoord
        int i = 0;
        //int ind = 0;
        float smallestDist = Mathf.Infinity;
        foreach(Transform child in HexDisplays.transform)
        {
            float dist = Vector2.SqrMagnitude(hexPos - (Vector2)child.position);

            if (dist < smallestDist){
                smallestDist = dist;
                //ind = i;
                hexDisp = child.gameObject;
            }
            i++;
        }
        
        //Highlight the hex
        if (hexDisp != null && hexDisp != prevHexDisp) {
            if (prevHexDisp != null)
            {
                prevHexDisp.GetComponent<HexDisplayControl>().Highlight(false);
            }
            hexDisp.GetComponent<HexDisplayControl>().Highlight(true);
            
            prevHexDisp = hexDisp;
        }

	}

    // Outline a single hex
    public void OutlineHex(HexCoord hex)
    {
        outlineObject = (GameObject)Instantiate(outlinePrefab);
        LineRenderer lineRend = outlineObject.GetComponent<LineRenderer>();
        int vertexCount = 7;
        lineRend.SetVertexCount(vertexCount);
        int ind = 0;
        foreach (Vector2 pos in hex.Corners()){
            Vector3 pos3d = new Vector3(pos.x, pos.y, 0.02f);   //Offset in the z direction slightly above board
            lineRend.SetPosition(ind, pos3d);

            if (ind == 0)
                lineRend.SetPosition(vertexCount-1, pos3d); //Also set the end vertex the same as the start point

            ind++;
        }
        
    }

    // Outline a single hex
    public void OutlineHexTall(HexCoord hex)
    {
        outlineObject = (GameObject)Instantiate(outlinePrefabTall);
        outlineObject.transform.position = hex.Position();
       
    }

    // Outline a single hex
    public void ShowLastMove(HexCoord strt, HexCoord curr)
    {

        ClearLastMove();
        outlineObject = (GameObject)Instantiate(outlinePrefabTall);
        outlineObject.transform.position = curr.Position();

        HighlightStart(strt);

    }

    public void ClearLastMove()
    {
        //Clear Outline
        ClearOutlines();

        //Clear start highlight
        if (startHighlight != null)
            Destroy(startHighlight);

    }


    // Find the hex that overlaps a certain x,y point
    public void HighlightHex(HexCoord hex)
    {

        Vector2 hexPos = hex.Position();
        GameObject hexDisp = null;

        //Brute force method to get hex display associated with a certain HexCoord
        int i = 0;
        float smallestDist = Mathf.Infinity;
        foreach (Transform child in HexDisplays.transform)
        {
            float dist = Vector2.SqrMagnitude(hexPos - (Vector2)child.position);

            if (dist < smallestDist)
            {
                smallestDist = dist;
                hexDisp = child.gameObject;
            }
            i++;
        }

        //Highlight the hex
        if (hexDisp != null)
        {
            hexDisp.GetComponent<HexDisplayControl>().Highlight(true);
        }
    }

    public Vector2 FindNearest(Vector2 pos) {
        // Find the hex that overlaps a certain x,y point
        //Debug.Log(pos);
        nearestHex = HexCoord.AtPosition(pos);
        Vector2 hexPos = nearestHex.Position();

        return hexPos;
    }

    public HexCoord GetNearestHex(Vector2 pos)
    {
        nearestHex = HexCoord.AtPosition(pos);
        return nearestHex;
    }

    public IEnumerable GetNeighbors(HexCoord hex)
    {
        // Return the neighbors of a given hexcoord
        //Debug.Log(pos);
        IEnumerable neighbors = hex.Neighbors();

        return neighbors;
    }

}
