using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnyInfo : MonoBehaviour
{

    public string Someinfo;

	// Use this for initialization
	void Start () {
        // AnyInfo is used to transfer infor across scenes
        // To initialise do:
        // AnyInfo Info = GameObject.Find("InfoObject").GetComponent<AnyInfo>();
        
        // To change Someinfo do:
        // Info.Someinfo =1;

        // To retrieve Someinfo do:
        // string getinfo = Info.Someinfo;
    }
	
}
