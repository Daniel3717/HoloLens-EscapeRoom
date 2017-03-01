using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WrapClues : MonoBehaviour {

    //needs to be placed on the same GameObject as PlaceObjects

    public GameObject[] mClues;
    public int[] mPositions;
    public bool mSuccessful = false;
    public bool mComplete = false;
    public bool mStartFlag = false;

    private PlaceObjects mPO;
    private bool mRunning = false;
    private bool mFinished = false;
    private GameObject[] mWraps;
    private bool mDirty = false;

    // Use this for initialization
    void Start () {

        mPO = this.gameObject.GetComponent<PlaceObjects>();
	}
	
	// Update is called once per frame
	void Update () {

        if (!mStartFlag) //built in reset
        {
            if (mDirty)
            {
                resetAll();

                mDirty = false;
            }
            return;
        }
        else
            mDirty = true;

        if (mFinished)
            return;

        if (mComplete)
        {
            if (!mFinished)
                unwrapClues();
            mFinished = true;

            if (!mSuccessful)
            {
                Debug.Log("Not successful");
                selfdestruct();
            }
            else
                Debug.Log("Successful");
            return;
        }

        if (!mRunning)
        {
            wrapAndLoadClues();
            mRunning = true;
        }

        mComplete = mPO.mComplete;
        mSuccessful = mPO.mSuccessful;
	}

    private void wrapAndLoadClues()
    {
        mWraps = new GameObject[mClues.Length];
        for (int i=0;i<mClues.Length;i++)
        {
            mWraps[i] = new GameObject();
            mWraps[i].transform.localEulerAngles = Vector3.zero;
            mClues[i].transform.localEulerAngles = Vector3.zero;
            mWraps[i].transform.position = mClues[i].transform.position;
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
            mWraps[i].transform.parent = mClues[i].transform.parent;
            mClues[i].transform.parent = mWraps[i].transform;
            mClues[i].SetActive(false);
            mWraps[i].transform.position = new Vector3(0, -200f, 0);
        }

        mPO.mPublicToPlace = mWraps;
        mPO.mPublicPosition = mPositions;
        mPO.mStartFlag = true;
    }

    private void unwrapClues()
    {
        Debug.Log("Unwrapping Clues");
        for (int i=0;i<mWraps.Length;i++)
        {
            mClues[i].SetActive(true);
            mClues[i].transform.parent = mWraps[i].transform.parent;
            Destroy(mWraps[i]);
            Debug.Log(mClues[i].name + " is at " + mClues[i].transform.position + " with rotation "+mClues[i].transform.rotation);
        }
    }

    public void resetAll()
    {
        mSuccessful = false;
        mComplete = false;
        mRunning = false;
        mFinished = false;
    }

    public void LoadClues(ClueToPlace[] clues)
    {
        Debug.Log("Called LoadClues");
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
                mPositions[i] = 0;
        }
    }

    public void selfdestruct()
    {
        //do error panel here
        GameObject errorPanel = GameObject.Find("Error Panel");
        if (errorPanel == null)
        {
            Debug.Log("Could not find Error Panel");
        }
        else
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<Text>().text = "Your Room is too small";
        }
        Destroy(this.gameObject);
    }
}
