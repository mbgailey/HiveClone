using UnityEngine;
using System.Collections;

public class GameSettings : MonoBehaviour {

    public bool offlineMode = true;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

	// Use this for initialization
	void Start () {
	
	}
	
}
