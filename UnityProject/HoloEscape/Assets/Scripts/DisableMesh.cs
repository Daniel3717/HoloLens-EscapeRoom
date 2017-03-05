using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class DisableMesh : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
