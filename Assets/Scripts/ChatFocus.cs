using UnityEngine;
using System.Collections;

public class ChatFocus : MonoBehaviour {

    public MainCameraControl camControl;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public void OnChatFocus () {
        camControl.DisableCameraControls();
	}

    public void OnChatUnfocus()
    {
        camControl.EnableCameraControls();
    }
}
