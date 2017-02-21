// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using HoloToolkit.Unity;
using System;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.UI;

public class App : Singleton<App>, ISourceStateHandler, IInputClickHandler
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
    public String Tip;
    public Transform Parent_Scene;
    public SpatialMappingObserver MappingObserver;
    public SpatialUnderstandingCursor AppCursor;
    public GameObject TipsText;
    public GameObject MainText;

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
            if ((stats.TotalSurfaceArea > kMinAreaForComplete) ||
                (stats.HorizSurfaceArea > kMinHorizAreaForComplete) ||
                (stats.WallSurfaceArea > kMinWallAreaForComplete))
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
    /*
    public Color PrimaryColor
    {
        get
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
            {
                if (trackedHandsCount > 0)
                {
                    return DoesScanMeetMinBarForCompletion ? Color.white : Color.white;
                }
                return DoesScanMeetMinBarForCompletion ? Color.yellow : Color.white;
            }

            // If we're looking at the menu, fade it out
            Vector3 hitPos, hitNormal;
            UnityEngine.UI.Button hitButton;
            float alpha = AppCursor.RayCastUI(out hitPos, out hitNormal, out hitButton) ? 0.15f : 1.0f;

            // Special case processing & 
            return (!string.IsNullOrEmpty(SpaceQueryDescription) || !string.IsNullOrEmpty(ObjectPlacementDescription)) ?
                (PrimaryText.Contains("processing") ? new Color(1.0f, 0.0f, 0.0f, 1.0f) : new Color(1.0f, 0.7f, 0.1f, alpha)) :
                new Color(1.0f, 1.0f, 1.0f, alpha);
        }
    }
    */
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
                Tip = "If you see an area with a gap in the mesh, try focusing on it from different angles and distances to fully scan it";
                counter = 0;
                return;
        }
    }
    /*
    public string DetailsText
    {
        get
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.None)
            {
                return "";
            }

            // Scanning stats get second priority
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                (SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {/*
                IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                {
                    return "Playspace stats query failed";
                }
                
                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                // Start showing the stats when they are no longer zero
                if (stats.TotalSurfaceArea > kMinAreaForStats)
                {
                    string subDisplayText = "Remember to look up and don't forget the corners either.";
                    return subDisplayText;
                }
                return "";
            }
            return "";
        }
    }
    */

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

    private void Update_Display(float deltaTime)
    {
        // Basic checks
        if (DebugDisplay == null)
        {
            return;
        }
        /*
        if (OverrideNormalText)
        {
            DebugDisplay.text = CustomText;
        }
        else
        {
            // Update display text
            DebugDisplay.text = PrimaryText;
        }

        DebugDisplay.color = PrimaryColor;
        //DebugSubDisplay.text = DetailsText;
        //Tips.text = DetailsText;
        */
        MainText.GetComponent<Text>().text = PrimaryText;
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done){
            TipsText.GetComponent<Text>().text = "";
        } else 
            TipsText.GetComponent<Text>().text = Tip;
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
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
            !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
        {
            SpatialUnderstanding.Instance.RequestFinishScan();
        }
    }
}