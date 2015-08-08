using UnityEngine;
using System.Collections;

public class HexDisplayControl : MonoBehaviour {

    public TextMesh coordText;
    public SpriteRenderer centerMark;
    public SpriteRenderer highlight;

	// Use this for initialization
	void Start () {
	
	}
	
	public void SetText (string coord) {
        coordText.text = coord;
	}

    public void DebugDisplayOn()
    {
        centerMark.gameObject.SetActive(true);
        coordText.gameObject.SetActive(true);
    }

    public void DebugDisplayOff()
    {
        centerMark.gameObject.SetActive(false);
        coordText.gameObject.SetActive(false);
    }

    public void Highlight(bool state)
    {
        highlight.gameObject.SetActive(state);
    }

}
