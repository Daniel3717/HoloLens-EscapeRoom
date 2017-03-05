using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPlacement : MonoBehaviour
{

    public GameObject mErrorPanel;
    
    private WrapClues mWC;
    

	// Use this for initialization
	void Start ()
	{
        mWC = GameObject.Find("Placements").GetComponent<WrapClues>();
	    mWC.errorPanel = this.mErrorPanel;
        mWC.mStartFlag = true;
	    Debug.Log("started Placement");
	}
	
	// Update is called once per frame
	void Update () {

	}
}
