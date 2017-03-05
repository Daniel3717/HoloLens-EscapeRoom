using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

public class PlaceObjects : MonoBehaviour {

    //wanted to return a lot of things in getResults, so I made this struct
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

    //int convention: 0 means floor. 1 means wall. 2 means ceiling. 3 means air. 4 means platform. 5 means at the base of a wall.
    //Note: base of a wall was not included in the project, but is enabled. See getDef for how it works.
    public int[] mPublicPosition;

    //number of tries before giving up on FWCR placements (Floor, Wall, Ceiling and Air (query is named RandomInAir, hence the R))
    public int mMaxTries = 40; 

    //these influence Wall type placements. Usually, when placing an object on the Wall, you would like it to not be close to the floor. Exceptions apply.
    public float mMinHeightWall = 1.1f;
    public float mMaxHeightWall = 7f;

    //flags so that other scripts can see the current status
    public bool mSuccessful = false;
    public bool mComplete = false;
    public bool mStartFlag = false;

    private GameObject[] mToPlace; //in case PublicToPlace is shallow copy
    private int[] mPosition; //in case PublicPosition is shallow copy

    private int mTimesTried;

    private List<int> mLeftToPlace = new List<int>();
    private List<int> mPlaced = new List<int>();
    private List<int> mPlatformLeftToPlace = new List<int>();

    private bool mPlaceFWCRComplete = false;
    private bool mPlacePComplete = false;

    private int mMaxPlatLoc = 65536;
    private int mPlaceFWCRThreshold;

    private bool mStarted = false;

    private float mYMinRandom;
    private float mYMaxRandom;

    //base wall is a type of 
    private float mBaseWallAbsError = 0.25f;

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
    }

    //if we create a reset feature, it's quite important to have this behaviour accessible from outside void Start()
    private void startIt()
    {
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
        SpatialUnderstandingDll.Imports.PlayspaceAlignment lPA = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignment();
        mYMinRandom = (lPA.CeilingYValue - lPA.FloorYValue) / 2 + lPA.FloorYValue;
        mYMaxRandom = lPA.CeilingYValue;

        if (mYMaxRandom == 0) //something went wrong when retrieving mYMaxRandom
            mYMaxRandom = 4;

    }

    // Update is called once per frame
    void Update()
    {
        if (!mStartFlag) //mStartFlag decides whether this code runs or not
        {
            return;
        }

        if (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Done)
            return;
        if (SpatialUnderstanding.Instance.UnderstandingCustomMesh.IsImportActive == true)
            return;



        if (mComplete) //if we have completed the placement don't try to place other things anymore
        {
            return;
        }

        if (!mStarted) //mStartFlag was just set to true, so we initialise what we need
        {
            startIt();
            mStarted = true;
        }

        if (!mPlaceFWCRComplete) //placing objects on Floor, Wall, Ceiling and Air (query is named RandomInAir, hence the R) is async. This is a type of polling.
        {
            ContinueStage1();
        }
        else
        if (!mPlacePComplete) //at the moment, placePlat is sync, but it can be separated into multiple stages should performance issues appear
            placePlat();
        else
        {
            //in case something went wrong, that part would set mComplete to true and mSuccessful to false
            //since we managed to arrive here, it means everything went according to plan. Placement successful!
            mSuccessful = true;
            mComplete = true;
        }
        
    }

    //Stage 1 are the Floor, Wall, Ceiling and Air (query is RandomInAir, hence the R). 
    //Since the underlying API is using an async way of doing things, this function is similar to polling
    private void ContinueStage1()
    {
        if (mTimesTried < mMaxTries) //placement may not be correct from the first try, since the API may collide clues between themselves. So, we try multiple times.
        {

            if (mTimesTried == 0) //initial sending of placement queries
            {
                sendQuery();
            }

            if (mLeftToPlace.Count > 0) //still have clues to place
            {
                if (LevelSolver.Instance.queryStatus.State != LevelSolver.QueryStates.Processing) //if the async placement has finished
                {
                    List<Result> Results = getResults();

                    mLeftToPlace.Clear();
                    for (int i = 0; i < Results.Count; i++)
                    {
                        GameObject nowGO = mToPlace[Results[i].OriginalIndex];
                        nowGO.transform.localPosition = Results[i].Position;
                        nowGO.transform.localRotation = Results[i].Rotation;

                        if (canPlace(Results[i].OriginalIndex)) //canPlace returns true if this clue collides with any other clues
                        {
                            mPlaced.Add(Results[i].OriginalIndex);
                        }
                        else
                        {
                            mLeftToPlace.Add(Results[i].OriginalIndex);

                            nowGO.transform.localPosition = new Vector3(0,-200,0); //put it away from sight (in the underworld). 
                            //Also, to not accidentally collide with other possible well-placed clues 
                        }
                    }

                    if (mLeftToPlace.Count + mPlaced.Count != mPlaceFWCRThreshold)
                    {
                        for (int i = 0; i < mToPlace.Length; i++)
                            if ((!mPlaced.Contains(i)) && (!mLeftToPlace.Contains(i))) //lost item. Had no results in placement query. Happens from time to time.
                                    mLeftToPlace.Add(i);
                    }
                    
                    if ((mLeftToPlace.Count > 0) && (mTimesTried < mMaxTries)) //still have clues to place. still have tries left. let's have another go.
                        sendQuery();
                }
            }
            else //managed to place all the clues of the FWCR placement types
            {
                mPlaceFWCRComplete = true;
            }

        }
        else
        {
            //ran out of tries. Placement failed.
            mSuccessful = false;
            mComplete = true;
        }
    }

    //canPlace returns true if the clue at index ToCheck does not collide with any other clue
    //note that this includes even the not placed yet clues. since they are in the underworld (at -200 on the y axis), they do not interfere
    private bool canPlace(int ToCheck) 
    {
        if (mPosition[ToCheck] == 3) //3 means in air. In air needs to be in the upper half. 
        {
            Collider lColl = mToPlace[ToCheck].GetComponent<Collider>();
            float lCollMinY = lColl.bounds.min.y;
            float lCollMaxY = lColl.bounds.max.y;
            if ((lCollMinY < mYMinRandom) || (lCollMaxY > mYMaxRandom))
                return false;
        }

        for (int i = 0; i < mPlaced.Count; i++)
        {
            GameObject nowGO = mToPlace[mPlaced[i]];
            if (mToPlace[ToCheck].GetComponent<Collider>().bounds.Intersects(nowGO.GetComponent<Collider>().bounds))
                return false;
        }
        return true;
    }

    //sendQuery will construct the queries for the clues in the LeftToPlace List and send them to the ObjectPlacement API.
    //note that Platform queries are not in the LeftToPlace List, but in the PlatformLeftToPlace list
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

    //getResults gets the Placement Results.
    //it gets them in the Result structure, which basically means in tuples of <Index in the ToPlace array, Position of the Placement, Rotation of the Placement>
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

    //gets the ObjectPlacementDefinition for the placement type i
    //Default to Floor
    private SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition getDef(int i, Vector3 halfDims)
    {
        if (i == 0)
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfDims);
        if (i == 1)
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnWall(halfDims, mMinHeightWall, mMaxHeightWall);
        if (i == 2)
        {
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnCeiling(halfDims);
        }
        if (i == 3)
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_RandomInAir(halfDims);

        if (i == 5)
        {
            //base means on the wall but very close to the floor
            //the SpatialUnderstandingDllObjectPlacement is trying to position the center of the object within this constraint
            //this naturally means that for taller objects we need to give a higher height, hence the halfDims.y
            //there is also a bit of an error, since usually the scan does not make the start of the wall perpendicular on the floor, but rather a bit diagonalized
            //this means that trying to place a clue exactly at the base will fail 
            //(since the wall the SpatialUnderstandingObjectPlacement API sees does not start from the floor, but from a bit higher up)  
            return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnWall(halfDims, 0, halfDims.y + mBaseWallAbsError);
        }

        return SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfDims);
    }

    //can also add rules based on the index. Put here in case someone wants to do further development.
    //to understand what a rule is, check the SpatialUnderstandingDllObjectPlacement API
    private List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> getListRules(int i)
    {
        return null;
    }

    //can also add constraints based on the index. Put here in case someone wants to do further development.
    //to understand what a constraint is, check the SpatialUnderstandingDllObjectPlacement API
    private List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> getListConstraints(int i)
    {
        return null;
    }

    //Platform placements use a different API from the other placements. Thus, they need to be separated
    private bool separatePlat(int i)
    {
        if (i == 4)
            return true;
        return false;
    }

    //if any problems arises (i.e. fps spikes when placing), this function will need to be split in multiple stages.
    //Then, rest must be given to Unity between the stages. I.e. call the stages one at a time in the Update() part. Don't call multiple stages in the same Update() call.
    //placePlat handles the placement for the Platform type of placement.
    //it relies on the SpatialUnderstandingDllShapes API. To add other Shape Definitions, see the ShapeDefinition script from HoloToolkit-Example/SpatialUnderstanding scripts
    private void placePlat()
    {
        List<Vector3>[] lPositions = new List<Vector3>[mPlatformLeftToPlace.Count];
        SpatialUnderstandingDllShapes.ShapeResult[] lPlatPos = new SpatialUnderstandingDllShapes.ShapeResult[mMaxPlatLoc];
        System.IntPtr lPlatPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(lPlatPos);

        for (int i=0;i< mPlatformLeftToPlace.Count;i++) //prepare to find all positions for each clue
        {
            lPositions[i] = new List<Vector3>();
        }

        for (int lIt = 0; lIt < mPlatformLeftToPlace.Count; lIt++) //here we find all Platform positions for each clue
        {
            int i = mPlatformLeftToPlace[lIt];
            Collider lColl = mToPlace[i].GetComponent<Collider>();
            float lMax = Mathf.Max(lColl.bounds.extents.x, Mathf.Max(lColl.bounds.extents.y, lColl.bounds.extents.z));
            int lPlatCount = SpatialUnderstandingDllShapes.QueryShape_FindPositionsOnShape(
                                "Platform", 1.1f*lMax,
                                lPlatPos.Length, lPlatPtr);

            for (int lPlatIt = 0; lPlatIt < lPlatCount; lPlatIt++)
            {
                lPositions[lIt].Add(lPlatPos[lPlatIt].position);
            }
            
            if (lPositions[lIt].Count == 0) //if there is no position for an object, just exit. The placement failed.
                lIt = mPlatformLeftToPlace.Count + 2;
        }

        int lItObj;

        //here we choose a random position from all our positions for each clue.
        //since there might be multiple clues of the same size on platforms, those will have exactly the same positions.
        //So it would be faster to try to select a position at random than collide for a while on the first positions
        for (lItObj=0;lItObj< mPlatformLeftToPlace.Count;lItObj++)
        {
            int iObj = mPlatformLeftToPlace[lItObj];
            int lItPos=2;
            bool lPlaced = false;
            while ((lPositions[lItObj].Count!=0)&&(!lPlaced)) //choose positions at random until we run out of positions
            {
                lItPos = Random.Range(0, lPositions[lItObj].Count - 1);

                //put object correctly on position. Without placing it upper than the position returned, we would have the center of the object at the position on the Platform
                //which essentially means the lower half will go through the Platform
                mToPlace[iObj].transform.localPosition = lPositions[lItObj][lItPos] + mToPlace[iObj].GetComponent<Collider>().bounds.extents.y * Vector3.up;

                if (canPlace(iObj)) //can place the object at this position
                {
                    mPlaced.Add(iObj);
                    lPlaced = true;
                }
                else
                    lPositions[lItObj].RemoveAt(lItPos); //position unusable
            }
            if (!lPlaced) //couldn't place the object, so just exit this for loop. The placement failed.
            {
                lItObj = mPlatformLeftToPlace.Count + 2;
                mToPlace[iObj].transform.localPosition = new Vector3(0, -1000, 0);
            }
        }

        if (lItObj == mPlatformLeftToPlace.Count) //successfully placed all the objects
        {
            mPlacePComplete = true;
        }
        else //could not place the objects
        {
            mPlacePComplete = false;
            mSuccessful = false;
            mComplete = true;
        }
    }
}
