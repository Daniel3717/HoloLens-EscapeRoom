using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WrapClues : MonoBehaviour {

    //needs to be placed on the same GameObject as PlaceObjects

    public GameObject[] mClues;
    public int[] mPositions; //see PlaceObjects for convention

    //these 2 will be synced (within max 1 frame) with the PlaceObjects values
    public bool mSuccessful = false;
    public bool mComplete = false;

    public bool mStartFlag = false;

    public GameObject errorPanel; //errorPanel is assigned in StartPlacement

    private PlaceObjects mPO;
    private bool mRunning = false;
    private bool mFinished = false;
    private GameObject[] mWraps;
    private GameObject mAdopt;

    // Use this for initialization
    void Start () {

        //The mesh data is retained in the SpatialUnderstanding GameObject's coordinate system
        //This means that the Placement results will be returned in that coordinate system
        //Thus, we position the clues based on that coordinate system. 
        mAdopt = GameObject.Find("SpatialUnderstanding");

        //this is used for communication between WrapClues and PlaceObjects.
        //this is also one of the reasons why WrapClues needs to be on the same object as PlaceObjects.
        //if you think the condition above is inconvenient, feel free to pass the PlaceObjects to mPO in some other way
        //and also check the selfdestruct function
        mPO = this.gameObject.GetComponent<PlaceObjects>();
	}
	
	// Update is called once per frame
	void Update () {

        if (!mStartFlag) //mStartFlag decides whether this code runs or not
        {
            return;
        }

        if (mFinished) //if we finished our job until this point in time, stop here. no reason to go past this.
            return;

        if (mComplete) //we completed
        {
            if (!mFinished) //if this is the first time here, we also need to unwrap
                unwrapClues();
            mFinished = true;

            if (!mSuccessful) //if placement failed, selfdestruct
            {
                selfdestruct();
            }

            return;
        }

        if (!mRunning) //so we started (with the mStartFlag), but we haven't loaded the data in (since we're not running). let's load.
        {
            wrapAndLoadClues();
            mRunning = true;
        }

        //sync those with the PlaceObjects values until the placement finishes
        mComplete = mPO.mComplete;
        mSuccessful = mPO.mSuccessful;
	}

    //wrappers are used for translation and also to allow the clues to become invisible without disabling their collider
    private void wrapAndLoadClues()
    {
        mWraps = new GameObject[mClues.Length];
        for (int i=0;i<mClues.Length;i++)
        {
            mWraps[i] = new GameObject();

            //initialise the wrapper at the clues position and with rotation zero
            mWraps[i].transform.localEulerAngles = Vector3.zero;
            mWraps[i].transform.position = mClues[i].transform.position;

            //the following will basically copy the collider from the clue to the wrap, while also translating between the Clues module and the ObjectPlacement module
            //Example: the ObjectPlacement module makes it so that the z-axis of a wall clue points outwards from the wall,
            //but the Clues module creates wall clues with their z-axis pointing towards the wall
            //(just standard integration translation)
            BoxCollider lWBC = mWraps[i].AddComponent<BoxCollider>();
            BoxCollider lCBC = mClues[i].GetComponent<BoxCollider>();
            lWBC.center = Vector3.zero;
            if ((mPositions[i] == 1) || (mPositions[i] == 5))
            {
                mClues[i].transform.localEulerAngles = new Vector3(0, 180, 0);
                lWBC.size = lCBC.bounds.size;
            }
            else
            if (mPositions[i] == 2)
            {
                mClues[i].transform.localEulerAngles = new Vector3(-90, 0, 0);
                lWBC.size = new Vector3(2 * lCBC.bounds.extents.x, 2 * lCBC.bounds.extents.y, 2 * lCBC.bounds.extents.z);
            }
            else
            {
                lWBC.size = lCBC.bounds.size;
            }
            
            mWraps[i].transform.parent = mAdopt.transform; //put it in the same coordinate system as the processed mesh

            mClues[i].transform.parent = mWraps[i].transform;//make it so that the clue goes to where the wrap goes
            mClues[i].SetActive(false); //this makes the clue invisible

            //wraps should start in the underworld (i.e. at y position -200f) such that they don't interfere with the canPlace check from PlaceObjects
            mWraps[i].transform.position = new Vector3(0, -200f, 0); 
        }

        mPO.mPublicToPlace = mWraps;
        mPO.mPublicPosition = mPositions;
        mPO.mStartFlag = true;
    }

    //after placement, need to get rid of the wraps and also change the transform.parent of the clue to the Placements object,
    //such that clues get deleted on the scene change or on the selfdestruct call
    private void unwrapClues()
    {
        for (int i=0;i<mWraps.Length;i++)
        {
            mClues[i].SetActive(true); //this makes the clue visible again
            mClues[i].transform.parent = this.gameObject.transform;
            Destroy(mWraps[i]);
        }

        HoloToolkit.Unity.SpatialUnderstandingDllObjectPlacement.Solver_RemoveAllObjects();
    }

    //translation function from one module (AppFlow) to the other (ObjectPlacement)
    public void LoadClues(ClueToPlace[] clues)
    {
        mClues = new GameObject[clues.Length];
        mPositions = new int[clues.Length];
        for (int i=0;i<clues.Length;i++)
        {
            GameObject lGO = clues[i].clue;
            mClues[i] = lGO;
            lGO.transform.parent = this.gameObject.transform;

            string lS = clues[i].placements[0];

            if (lS == "floor")
                mPositions[i] = 0;
            else
            if (lS == "wall")
                mPositions[i] = 1;
            else
            if (lS == "ceiling")
                mPositions[i] = 2;
            else
            if (lS == "air")
                mPositions[i] = 3;
            else
            if (lS == "platform")
                mPositions[i] = 4;
            else
            if (lS == "base")
                mPositions[i] = 5;
            else
                mPositions[i] = 0;
        
        }
    }

    //in case object placement fails, the clues will be scattered all around the place (behaviour of API is quite unpredicatable when it fails)
    //thus, we need to get rid of everything
    public void selfdestruct()
    {
        //do error panel here
        if (errorPanel == null)
        {
            //could not find the Error Panel
        }
        else
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<Text>().text = "Your scanned area is too small. You can either try another room or go back to the Main Menu to rescan your area";
        }
        Destroy(this.gameObject);
    }
}
