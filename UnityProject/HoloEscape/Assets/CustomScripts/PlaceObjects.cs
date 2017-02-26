using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

public class PlaceObjects : MonoBehaviour {

    private struct Result
    {
        public int OriginalIndex;
        public Vector3 Position;
        public Quaternion Rotation;

        public Result(int index,Vector3 pos, Quaternion rot)
        {
            OriginalIndex = index;
            Position = pos;
            Rotation = rot;
        }
    }

    public GameObject[] mPublicToPlace;
    public int[] mPublicPosition; 
    //0 means floor. 1 means wall. 2 means ceiling. 3 means air. 4 means platform. 5 means at the base of a wall.
    public int mMaxTries = 40;
    public float mMinHeightWall = 1.1f;
    public float mMaxHeightWall = 7f;
    public bool mSuccessful = false;
    public bool mComplete = false;
    public bool mStartFlag = false;

    private GameObject[] mToPlace; //in case PublicToPlace is shallow copy
    private int[] mPosition; //in case PublicPosition is shallow copy
    private List<int> mLeftToPlace = new List<int>();
    private List<int> mPlaced = new List<int>();
    private int mTimesTried;
    private string mToWrite = "";
    private List<int> mPlatformLeftToPlace = new List<int>();
    private bool mPlaceFWCRComplete = false;
    private bool mPlacePComplete = false;
    private int mMaxPlatLoc = 65536;
    private int mPlaceFWCRThreshold;
    private bool mCollidersEnabled = false;
    private bool mStarted = false;
    private float mYMinRandom;
    private float mYMaxRandom;
    private float mBaseWallAbsError = 0.25f;
    private bool mDirty = false;

    // Use this for initialization
    void Start()
    {

    }


    //beneficial to place the biggest object first
    private void sortBiggestGOFirst()
    {
        //in case shallow copy is given
        mToPlace = new GameObject[mPublicToPlace.Length];
        mPosition = new int[mPublicPosition.Length];
        for (int i = 0; i < mToPlace.Length; i++)
        {
            mToPlace[i] = mPublicToPlace[i];
            mPosition[i] = mPublicPosition[i];
        }

        for (int i=0;i<mToPlace.Length;i++)
            for (int j=i+1;j<mToPlace.Length;j++)
                if (mToPlace[i].GetComponent<Collider>().bounds.size.sqrMagnitude< mToPlace[j].GetComponent<Collider>().bounds.size.sqrMagnitude)
                {
                    GameObject axGO;
                    axGO = mToPlace[j];
                    mToPlace[j] = mToPlace[i];
                    mToPlace[i] = axGO;

                    int axPos;
                    axPos = mPosition[i];
                    mPosition[i] = mPosition[j];
                    mPosition[j] = axPos;
                }

        /*
        for (int i = 0; i < mToPlace.Length; i++)
            mToWrite += "\nIn mToPlace, at index " + i + " we have object " + mToPlace[i].name + " which wants to go at position " + mPosition[i];
        mToWrite += "\nExited sort";
        */
    }

    //if we create a reset feature, it's quite important to have this behaviour accessible from outside void Start()
    private void startIt()
    {
        mToWrite += "\nEntered StartIt and found " + mPublicToPlace.Length + " objects";
        sortBiggestGOFirst();
        for (int i = 0; i < mToPlace.Length; i++)
        {
            if (separatePlat(mPosition[i]))
                mPlatformLeftToPlace.Add(i);
            else
                mLeftToPlace.Add(i);
        }
        
        mPlaceFWCRThreshold = mLeftToPlace.Count;
        mTimesTried = 0;
        mStarted = true;
        SpatialUnderstandingDll.Imports.PlayspaceAlignment lPA = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignment();
        mYMinRandom = (lPA.CeilingYValue - lPA.FloorYValue) / 2 + lPA.FloorYValue;
        mYMaxRandom = lPA.CeilingYValue;
        mToWrite += "\nExited StartIt with " + mLeftToPlace.Count + " FWCR to place and " + mPlatformLeftToPlace.Count + " Plat to place \n and mYMinRandom=" + mYMinRandom + ", mYMaxRandom=" + mYMaxRandom;
    }

    // Update is called once per frame
    void Update()
    {

        if (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Done)
            return;
        if (SpatialUnderstanding.Instance.UnderstandingCustomMesh.IsImportActive == true)
            return;

        if (!mStartFlag) //built-in reset
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

        if (mComplete)
        {
            //not in use
            /*
            if (mCollidersEnabled==true)
            {
                disableColliders();
                for (int i = 0; i < mToPlace.Length; i++)
                {
                    Collider localColl = mToPlace[i].GetComponent<Collider>();
                    if (mPosition[i] == 3)
                        mToWrite += "\n center of " + mToPlace[i].name + " is at " + localColl.bounds.center + " and extents are " + localColl.bounds.extents + '\n';
                }

            }
            */
            //mToWrite="";
            AppState.Instance.CustomText = mToWrite+"\nPlacing FWCR:"+mPlaceFWCRComplete+"\nPlacing P:"+mPlacePComplete+"\nSuccessful:"+mSuccessful;
            return;
        }

        //not in use
        //if (mCollidersEnabled == false)
        //    enableColliders();

        if (!mStarted)
            startIt();

        AppState.Instance.CustomText = mToWrite;
        if (!mPlaceFWCRComplete)
        {
            ContinueStage1();
        }
        else
        if (!mPlacePComplete)
            placePlat();
        else
        {
            mSuccessful = true;
            mComplete = true;
        }
        
    }

    private void enableColliders()
    {
        for (int i = 0; i < mPublicToPlace.Length; i++)
            mPublicToPlace[i].GetComponent<Collider>().enabled = true;
        mCollidersEnabled = true;
    }

    private void disableColliders()
    {
        for (int i = 0; i < mPublicToPlace.Length; i++)
            mPublicToPlace[i].GetComponent<Collider>().enabled = false;
        mCollidersEnabled = false;
    }

    private void ContinueStage1()
    {
        if (mTimesTried < mMaxTries)
        {

            if (mTimesTried == 0)
            {
                sendQuery();
            }

            if (mLeftToPlace.Count > 0)
            {
                if (LevelSolver.Instance.queryStatus.State != LevelSolver.QueryStates.Processing)
                {
                    List<Result> Results = getResults();

                    mLeftToPlace.Clear();
                    for (int i = 0; i < Results.Count; i++)
                    {
                        GameObject nowGO = mToPlace[Results[i].OriginalIndex];
                        nowGO.transform.position = Results[i].Position;
                        nowGO.transform.rotation = Results[i].Rotation;

                        /*
                        //some special situations which arose from some integration
                        if (mPosition[Results[i].OriginalIndex] == 1)
                            nowGO.transform.rotation = Quaternion.LookRotation(-nowGO.transform.forward, nowGO.transform.up);
                        if (mPosition[Results[i].OriginalIndex] == 2)
                            nowGO.transform.rotation = Quaternion.LookRotation(Vector3.up);
                        */


                        if (canPlace(Results[i].OriginalIndex))
                        {
                            mPlaced.Add(Results[i].OriginalIndex);
                        }
                        else
                        {
                            mLeftToPlace.Add(Results[i].OriginalIndex);

                            nowGO.transform.position = new Vector3(0,-200,0); //put it away from sight (in the underworld)
                        }
                    }

                    if (mLeftToPlace.Count + mPlaced.Count != mPlaceFWCRThreshold)
                    {
                        for (int i = 0; i < mToPlace.Length; i++)
                            if ((!mPlaced.Contains(i)) && (!mLeftToPlace.Contains(i))) //lost item. Had no results in placement query. Happens from time to time.
                                    mLeftToPlace.Add(i);
                    }
                    /*
                    if (mLeftToPlace.Count + mPlaced.Count != mPlaceFWCRThreshold)
                    {
                        AppState.Instance.OverrideNormalText = true;
                        mToWrite += "\nMissing the following:";
                        for (int i = 0; i < mToPlace.Length; i++)
                            if ((!mPlaced.Contains(i)) && (!mLeftToPlace.Contains(i)))
                                mToWrite += mToPlace[i].name + " , ";
                        mToWrite += "\nLeftToPlace:" + mLeftToPlace.Count + "  Placed:" + mPlaced.Count;
                        mSuccessful = false;
                        mComplete = true;
                    }
                    */
                    if ((mLeftToPlace.Count > 0) && (mTimesTried < mMaxTries))
                        sendQuery();
                }
            }
            else
            {
                mPlaceFWCRComplete = true;
            }

        }
        else
        {
            AppState.Instance.OverrideNormalText = true;
            mToWrite += "\nRan out of tries";

            mToWrite += "\nLeftToPlace is: ";
            for (int i = 0; i < mLeftToPlace.Count; i++)
                mToWrite += mToPlace[mLeftToPlace[i]].name+" , ";


            mSuccessful = false;
            mComplete = true;
        }
    }

    private bool canPlace(int ToCheck)
    {
        if (mPosition[ToCheck] == 3) //3 means in air. In air needs to be in the upper half. 
        {
            Collider lColl = mToPlace[ToCheck].GetComponent<Collider>();
            float lCollMinY = lColl.bounds.min.y;
            float lCollMaxY = lColl.bounds.max.y;
            if ((lCollMinY<mYMinRandom)||(lCollMaxY>mYMaxRandom))
                return false;
        }

                for (int i=0;i<mPlaced.Count;i++)
        {
            GameObject nowGO = mToPlace[mPlaced[i]];
            if (mToPlace[ToCheck].GetComponent<Collider>().bounds.Intersects(nowGO.GetComponent<Collider>().bounds))
                return false;
        }
        return true;
    }

    private void sendQuery()
    {
        mTimesTried++;
        int nowGOindex;
        List<LevelSolver.PlacementQuery> Queries = new List<LevelSolver.PlacementQuery>();
        for (int i = 0; i < mLeftToPlace.Count; i++)
        {
            nowGOindex = mLeftToPlace[i];
            Queries.Add
                (
                new LevelSolver.PlacementQuery
                    (
                        getDef(mPosition[nowGOindex], mToPlace[nowGOindex].GetComponent<Collider>().bounds.extents),
                        getListRules(mPosition[nowGOindex]),
                        getListConstraints(mPosition[nowGOindex])
                    )
                );
        }
        LevelSolver.Instance.PlaceObjectAsync("CustomQuery" + mTimesTried, Queries, false);
    }

    private List<Result> getResults()
    {
        List<LevelSolver.PlacementResult> BigResults = LevelSolver.Instance.placementResults;
        List<Result> NormalResults = new List<Result>();
        for (int i = 0; i < BigResults.Count; i++)
        {
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult ResultNow = BigResults[i].Result;
            NormalResults.Add(
                new Result(
                    mLeftToPlace[i],
                    ResultNow.Position,
                    Quaternion.LookRotation(ResultNow.Forward, ResultNow.Up)
                    )
                );
        }
        return NormalResults;
    }

    //Default to Floor
    private SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition getDef(int i, Vector3 halfDims)
    {
        if (i == 0)
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfDims);
        if (i == 1)
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnWall(halfDims, mMinHeightWall, mMaxHeightWall);
        if (i == 2)
        {
            /*
            float max2Dims = Mathf.Max(halfDims.x, halfDims.y);
            Vector3 halfDimsCeil = new Vector3(max2Dims, halfDims.z, max2Dims);
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnCeiling(halfDimsCeil);
            */
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnCeiling(halfDims);
        }
        if (i == 3)
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_RandomInAir(halfDims);

        if (i == 5)
        {
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnWall(halfDims, 0, halfDims.y + mBaseWallAbsError);
        }

        return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfDims);
    }

    private List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> getListRules(int i) //not in use yet. I put it here so it's easier to add in case of need.
    {
        return null;
    }

    private List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> getListConstraints(int i) //not in use yet. I put it here so it's easier to add in case of need.
    {
        return null;
    }

    private bool separatePlat(int i)
    {
        if (i == 4)
            return true;
        return false;
    }

    //note: Coroutines/IEnumerators weirdly do not seem to be different from normal function calls in Hololens
    //if any problems arises (i.e. fps spikes when placing), I'll split this one into different parts and do it in update
    private void placePlat()
    {
        AppState.Instance.OverrideNormalText = true;
        mToWrite += "\nProcessing placePlat";

        List<Vector3>[] lPositions = new List<Vector3>[mPlatformLeftToPlace.Count];
        SpatialUnderstandingDllShapes.ShapeResult[] lPlatPos = new SpatialUnderstandingDllShapes.ShapeResult[mMaxPlatLoc];
        System.IntPtr lPlatPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(lPlatPos);

        for (int i=0;i< mPlatformLeftToPlace.Count;i++)
        {
            lPositions[i] = new List<Vector3>();
        }

        for (int lIt = 0; lIt < mPlatformLeftToPlace.Count; lIt++)
        {
            int i = mPlatformLeftToPlace[lIt];
            Collider lColl = mToPlace[i].GetComponent<Collider>();
            float lMax = Mathf.Max(lColl.bounds.extents.x, Mathf.Max(lColl.bounds.extents.y, lColl.bounds.extents.z));
            mToWrite += "\nEntering Shape Query for " + i + " with lMax="+lMax;
            int lPlatCount = SpatialUnderstandingDllShapes.QueryShape_FindPositionsOnShape(
                                "Platform", 1.1f*lMax,
                                lPlatPos.Length, lPlatPtr);
            mToWrite += "\nExited Shape Query for " + i + " and found " + lPlatCount + " positions";

            for (int lPlatIt = 0; lPlatIt < lPlatCount; lPlatIt++)
            {
                lPositions[lIt].Add(lPlatPos[lPlatIt].position);
            }

            mToWrite += "\nFound " + lPositions[lIt].Count + " places for the object " + lIt;
            if (lPositions[lIt].Count == 0)
                lIt = mPlatformLeftToPlace.Count + 2;
        }

        int lItObj;
        for (lItObj=0;lItObj< mPlatformLeftToPlace.Count;lItObj++)
        {
            int iObj = mPlatformLeftToPlace[lItObj];
            int lItPos=2;
            bool lPlaced = false;
            while ((lPositions[lItObj].Count!=0)&&(!lPlaced))
            {
                lItPos = Random.Range(0, lPositions[lItObj].Count - 1);
                mToPlace[iObj].transform.position = lPositions[lItObj][lItPos] + mToPlace[iObj].GetComponent<Collider>().bounds.extents.y * Vector3.up;
                if (canPlace(iObj))
                {
                    mPlaced.Add(iObj);
                    lPlaced = true;
                }
                else
                    lPositions[lItObj].RemoveAt(lItPos);
            }
            if (!lPlaced)
            {
                mToWrite += "\nCould not place object " + iObj;
                lItObj = mPlatformLeftToPlace.Count + 2;
                mToPlace[iObj].transform.position = new Vector3(0, -1000, 0);
            }
            else
                mToWrite += "\nPlaced object " + iObj;
        }

        if (lItObj == mPlatformLeftToPlace.Count)
        {
            mPlacePComplete = true;
        }
        else
        {
            mPlacePComplete = false;
            mSuccessful = false;
            mComplete = true;
        }
        //yield return null;
    }

    public void resetAll()
    {
        mSuccessful = false;
        mComplete = false;
        mPlaceFWCRComplete = false;
        mPlacePComplete = false;
        mCollidersEnabled = false;
        mStarted = false;
        List<int> mLeftToPlace = new List<int>();
        List<int> mPlaced = new List<int>();
        mToWrite = "";
        List<int> mPlatformLeftToPlace = new List<int>();
    }
}
