using UnityEngine;
using System.Collections;

public class HighlightController : MonoBehaviour {

    public Color start;
    public Color eligible;
    public Color inEligible;

    public SpriteRenderer highlight; 

	// Use this for initialization
	void Start () {

	}
	

	public void StartHex () {
        highlight.color = start;
	}
    public void EligibleHex()
    {
        highlight.color = eligible;
    }
    public void IneligibleHex()
    {
        highlight.color = inEligible;
    }
}
