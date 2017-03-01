using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPlacement : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    GameObject.Find("Placements").GetComponent<WrapClues>().mStartFlag = true;
	    Debug.Log("started Placement");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
