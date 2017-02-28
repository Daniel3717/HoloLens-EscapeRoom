// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using HoloToolkit.Unity;
using System;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class App : Singleton<App>, ISourceStateHandler, IInputClickHandler, ISpeechHandler
{
    public bool OverrideNormalText = false;
    public string CustomText = "Default";

    // to modify in other scripts do:
    // AppState.Instance.OverrideNormalText = true;
    // AppState.Instance.CustomText = "Whatever";

    // Consts
    public float kMinAreaForStats = 5.0f;
    public float kMinAreaForComplete = 50.0f;
    public float kMinHorizAreaForComplete = 25.0f;
    public float kMinWallAreaForComplete = 10.0f;

    // Config
    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;
    public string Tip;
    public Transform Parent_Scene;
    public SpatialMappingObserver MappingObserver;
    public SpatialUnderstandingCursor AppCursor;
    public GameObject TipsText;
    public GameObject MainText;
    public Image foregroundImage;
    public Canvas canvas;
    
    // Properties
    public string SpaceQueryDescription
    {
        get
        {
            return spaceQueryDescription;
        }
        set
        {
            spaceQueryDescription = value;
            objectPlacementDescription = "";
        }
    }

    public string ObjectPlacementDescription
    {
        get
        {
            return objectPlacementDescription;
        }
        set
        {
            objectPlacementDescription = value;
            spaceQueryDescription = "";
        }
    }

    public bool DoesScanMeetMinBarForCompletion
    {
        get
        {
            // Only allow this when we are actually scanning
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
                (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                return false;
            }

            // Query the current playspace stats
            IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
            {
                return false;
            }
            SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

            // Check our preset requirements
            if (/*(stats.TotalSurfaceArea > kMinAreaForComplete) || */
                (stats.HorizSurfaceArea >= kMinHorizAreaForComplete) &&
                (stats.WallSurfaceArea >= kMinWallAreaForComplete))
            {
                return true;
            }
            return false;
        }
    }

    public string PrimaryText
    {
        get
        {
            /* REMOVED
            // Display the space and object query results (has priority)
            if (!string.IsNullOrEmpty(SpaceQueryDescription))
            {
                return SpaceQueryDescription;
            }
            else if (!string.IsNullOrEmpty(ObjectPlacementDescription))
            {
                return ObjectPlacementDescription;
            }
            */

            // Scan state

            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        
                        /* REMOVED
                        // Get the scan stats
                        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                        {
                            return "playspace stats query failed";
                        }
                        */
                // The stats tell us if we could potentially finish
                if (DoesScanMeetMinBarForCompletion)
                {
                    return "When ready, air tap to finish the scan";
                }
                return "Walk around and scan the room";
            case SpatialUnderstanding.ScanStates.Finishing:
                return "Finalising scan (please wait)";
            case SpatialUnderstanding.ScanStates.Done:
                return "Scan complete";
            default:
                return "ScanState = " + SpatialUnderstanding.Instance.ScanState.ToString();
                }
                
            }
            return "";
        }
    }


    int counter = 0;

    public void NextTip () {
        switch(counter)
        {
            case 0:
                Tip = "Remember to look up";
                counter++;
                return;
            case 1:
                Tip = "Don't forget to scan the corners";
                counter++;
                return;
            case 2:
                Tip = "Remember to keep objects within the scanning range: 0.85 to 3 meters away from the HoloLens";
                counter++;
                return;
            case 3:
                Tip = "The progress bar corresponds to the minimum amount of scanning required, but feel free to do some more!";
                counter++;
                return;
            case 4:
                Tip = "If you see an area with a gap in the mesh, try focusing on it from different angles and distances to fully scan it";
                counter = 0;
                return;
        }
    }

    // Privates
    private string spaceQueryDescription;
    private string objectPlacementDescription;
    private uint trackedHandsCount = 0;

    // Functions
    private void Start()
    {
        // Default the scene & the HoloToolkit objects to the camera
        Vector3 sceneOrigin = Camera.main.transform.position;
        Parent_Scene.transform.position = sceneOrigin;
        MappingObserver.SetObserverOrigin(sceneOrigin);
        InputManager.Instance.AddGlobalListener(gameObject);
        InvokeRepeating("NextTip", 7, 10);
    }

    protected override void OnDestroy()
    {
        InputManager.Instance.RemoveGlobalListener(gameObject);
    }

    float oldfill = 0;

    private void Update_Display(float deltaTime)
    {
        // Basic checks
        if (MainText == null)
        {
            return;
        }

        MainText.GetComponent<Text>().text = PrimaryText;
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Finishing) || (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done))
        {
            TipsText.GetComponent<Text>().text = "";
        }
        else
            TipsText.GetComponent<Text>().text = Tip;


        //MainText.GetComponent<Text>().text = (foregroundImage.fillAmount).ToString() + " tot " + stats.TotalSurfaceArea + " floor " + stats.HorizSurfaceArea + " wall " + stats.WallSurfaceArea;
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
        {
            float totalArea = /*kMinAreaForComplete + */ kMinHorizAreaForComplete + kMinWallAreaForComplete;

            if (oldfill < 1 )
            {
                float fillAmount = 0;
                float floor;
                float wall;

                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
                //MainText.GetComponent<Text>().text = (foregroundImage.fillAmount).ToString() + " f " + stats.HorizSurfaceArea + " w " + stats.WallSurfaceArea;

                /*
                if (stats.TotalSurfaceArea < kMinAreaForComplete)
                {
                    fillAmount += stats.TotalSurfaceArea / totalArea;
                }
                else
                {
                    fillAmount += kMinAreaForComplete / totalArea;
                }
                */

                if (stats.HorizSurfaceArea < kMinHorizAreaForComplete)
                {
                    floor = stats.HorizSurfaceArea;
                }
                else
                {
                    floor = kMinHorizAreaForComplete;
                }

                if (stats.WallSurfaceArea < kMinWallAreaForComplete)
                {
                    wall = stats.WallSurfaceArea;
                }
                else
                {
                    wall = kMinWallAreaForComplete;
                }

                fillAmount = (floor + wall) / totalArea;
                foregroundImage.fillAmount = fillAmount;
                oldfill = fillAmount;
            }
        }
    }

    /*
    private void Update_KeyboardInput(float deltaTime)
    {
        // Toggle SurfaceMapping & CustomUnderstandingMesh visibility
        if (Input.GetKeyDown(KeyCode.BackQuote) &&
            (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)))
        {
            SpatialMappingManager.Instance.DrawVisualMeshes = !SpatialMappingManager.Instance.DrawVisualMeshes;
            Debug.Log("SpatialUnderstanding -> ProcessedMap.drawMeshes=" + SpatialMappingManager.Instance.DrawVisualMeshes);
        }
        else if (Input.GetKeyDown(KeyCode.BackQuote) &&
                 (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = !SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh;
            Debug.Log("SpatialUnderstanding -> ProcessedMap.drawMeshes=" + SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh);
        }
    }
    */

    private void Update()
    {
        // Updates
        Update_Display(Time.deltaTime);
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        // If the source has positional info and there is currently no visible source
        if (eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            trackedHandsCount++;
        }
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        if (eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            trackedHandsCount--;
        }
    }

    public void OnInputClicked(InputEventData eventData)
    {
        FinishScan();
    }

    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch(eventData.RecognizedText.ToLower())
        {
            case "finish scan":
                FinishScan();
                break;
            case "main menu":
                SceneManager.LoadScene(0);
                break;
            case "skip scan":
                SceneManager.LoadScene(3);
                break;
        }
    }
    public void FinishScan()
    {
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
            !SpatialUnderstanding.Instance.ScanStatsReportStillWorking &&
            DoesScanMeetMinBarForCompletion)
        {
            Camera camera = canvas.GetComponent<Canvas>().worldCamera;
            //TipsText.GetComponent<Text>().text = "x " + camera.transform.position.x + " y " + camera.transform.position.y + " z " + camera.transform.position.z + " x1 " + canvas.transform.position.x + " y1 " + canvas.transform.position.y + " z1 " + canvas.transform.position.z;
            canvas.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane + 2));
            SpatialUnderstanding.Instance.RequestFinishScan();
        }
        SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial = null;
    }
}