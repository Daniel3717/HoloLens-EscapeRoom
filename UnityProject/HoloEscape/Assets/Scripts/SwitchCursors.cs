using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCursors : MonoBehaviour
{

    public GameObject NewCursor;

	// Use this for initialization
	void Start ()
	{
	    GameObject cursor = GameObject.Find("Cursor");

        if (cursor.activeSelf)
	    {
            cursor.SetActive(false);
            NewCursor.SetActive(true);
            NewCursor.transform.GetChild(0).gameObject.SetActive(true);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
