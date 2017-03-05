using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPosZ : MonoBehaviour
{

    public Transform CanvasTransform;
    private Vector3 localInitial;
    
	// Use this for initialization
	void Start ()
	{
	    localInitial = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update ()
	{
        Vector3 pos = transform.localPosition;
	    localInitial.y = pos.y;
        transform.localPosition = localInitial;
	    transform.rotation = new Quaternion(0,0,0,0);
	}
}
