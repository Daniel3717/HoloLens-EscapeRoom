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
	    //transform.parent = CanvasTransform.transform;
	    //pos.z = CanvasTransform.position.z;
	    //pos.x = CanvasTransform.position.x;
	    //transform.position = pos;
	    //transform.position = new Vector3(CanvasTransform.position.x, CanvasTransform.position.y+0.4f, CanvasTransform.position.z);
	    //transform.position = CanvasTransform.position;
	    transform.rotation = new Quaternion(0,0,0,0);
	}
}
