using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIController : MonoBehaviour {

    public RectTransform tipDisplay;
    public Text tipText;
    public Canvas pauseGUI;
    //public Canvas mainGUI;
    public RectTransform mainGUI;

    bool paused = false;

	// Use this for initialization
	void Start () {
        DisableTip();
        HidePauseMenu();
	}
	
	void DisableTip () 
    {
        tipDisplay.gameObject.SetActive(false);
	}

    void UpdateTip(string txt)
    {
        tipText.text = txt;
        tipDisplay.gameObject.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        pauseGUI.enabled = true;
        //mainGUI.enabled = false;
        mainGUI.gameObject.SetActive(false);
        paused = true;

    }

    public void HidePauseMenu()
    {
        pauseGUI.enabled = false;
        //mainGUI.enabled = true;
        mainGUI.gameObject.SetActive(true);
        paused = false;
    }

    public void TogglePauseMenu()
    {
        if (paused)
        {
            HidePauseMenu();
        }
        else
        {
            ShowPauseMenu();
        }
    }

}
