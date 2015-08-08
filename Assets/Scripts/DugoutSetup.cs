using UnityEngine;
using System.Collections;

public class DugoutSetup : MonoBehaviour {

    Vector3 dugoutCenterPoint = new Vector3(0f,-149f, 0.4f);
    float distanceBetween = 1.7f;
    float xPos = 0.9f; //Starting x position
    float startingRotation = -30f;
    //White pieces first then black pieces. 
    //Ant, Beetle, Queen, Spider, Grasshopper
    public GameObject[] piecePrefabArray = new GameObject[10];
    public int[] pieceCountArray = new int[5];
    //public bool multiplayer = true;

	// Use this for initialization
	void Start () {

        Debug.Log("PhotonNetwork.player.ID " + PhotonNetwork.player.ID);

        int pieces = piecePrefabArray.Length / 2;
        
        for(int i = 0; i < pieces; i++)
        {
            
            for (int count = 1; count <= pieceCountArray[i]; count++)
            {
                //Place white pieces
                Vector3 whitePos = new Vector3(-xPos, dugoutCenterPoint.y, dugoutCenterPoint.z);
                if (PhotonNetwork.isMasterClient || PhotonNetwork.offlineMode) //Master client creates white pieces. Player ID will be -1 for offline mode
                {
                    NetworkSpawn(piecePrefabArray[i].name, whitePos, Quaternion.Euler(new Vector3(0f, 0f, -startingRotation)));
                }
                //Place black pieces
                Vector3 blackPos = new Vector3(xPos, dugoutCenterPoint.y, dugoutCenterPoint.z);
                if (!PhotonNetwork.isMasterClient || PhotonNetwork.offlineMode) //Other client creates black pieces. Player ID will be -1 for offline mode
                {
                    NetworkSpawn(piecePrefabArray[i + 5].name, blackPos, Quaternion.Euler(new Vector3(0f, 0f, startingRotation)));
                }

                ////Place white pieces
                //Vector3 whitePos = new Vector3(-xPos, dugoutCenterPoint.y, dugoutCenterPoint.z);
                //if (PhotonNetwork.isMasterClient) //Master client creates white pieces. Player ID will be -1 for offline mode
                //{ 
                //    if (!PhotonNetwork.offlineMode) //If multiplayer do a network spawn
                //    {
                //        NetworkSpawn(piecePrefabArray[i].name, whitePos, Quaternion.Euler(new Vector3(0f, 0f, -startingRotation)));
                //    }
                //}
                //if (PhotonNetwork.offlineMode)
                //{
                //    Instantiate(piecePrefabArray[i], whitePos, Quaternion.Euler(new Vector3(0f, 0f, -startingRotation)));
                //}
                   
                ////Place black pieces
                //Vector3 blackPos = new Vector3(xPos, dugoutCenterPoint.y, dugoutCenterPoint.z);
                //if (!PhotonNetwork.isMasterClient) //Other client creates black pieces. Player ID will be -1 for offline mode
                //{
                //    if (!PhotonNetwork.offlineMode) //If multiplayer do a network spawn
                //    {
                //        NetworkSpawn(piecePrefabArray[i + 5].name, blackPos, Quaternion.Euler(new Vector3(0f, 0f, -startingRotation * 2f)));
                //    }
                //}

                //if (PhotonNetwork.offlineMode)
                //{
                //    Instantiate(piecePrefabArray[i + 5], blackPos, Quaternion.Euler(new Vector3(0f, 0f, -startingRotation * 2f)));  //Assumes black pieces are all after white pieces in prfab array
                //}

               

                xPos += distanceBetween;
            }
            
        }
	}
	
	// Used to instantiate network objects
	void NetworkSpawn(string name, Vector3 pos, Quaternion rot)
    {
        PhotonNetwork.Instantiate(name, pos, rot, 0);
        //The last argument is an optional group number, feel free to ignore it for now.
    }
}
