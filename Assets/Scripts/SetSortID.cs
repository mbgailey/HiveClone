using UnityEngine;
using System.Collections;

public class SetSortID : MonoBehaviour {

    public int SortID = 0;

	// Use this for initialization
	void Start () {
        this.gameObject.GetComponent<Renderer>().sortingLayerID = SortID;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
