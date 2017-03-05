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
    // Consts
    public float kMinHorizAreaForComplete = 25.0f;
    public float kMinWallAreaForComplete = 10.0f;

    // Config
    private string _tip;
    public Transform Parent_Scene;
    public SpatialMappingObserver MappingObserver;
    public SpatialUnderstandingCursor AppCursor;
    public GameObject TipsText;
    public GameObject MainText;
    public Image foregroundImage;
    public Canvas canvas;
    int _tipCounter = 0;
    float _progressBarOldfill = 0;

    // Properties
    public string SpaceQueryDescription
    {
        get { return spaceQueryDescription; }
        set
        {
            spaceQueryDescription = value;
            objectPlacementDescription = "";
        }
    }

    public string ObjectPlacementDescription
    {
        get { return objectPlacementDescription; }
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
            SpatialUnderstandingDll.Imports.PlayspaceStats stats =
                SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

            // Check our preset requirements
            if ( /*(stats.TotalSurfaceArea > kMinAreaForComplete) || */
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
            // Scan state

            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
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


    public void NextTip()
    {
        switch (_tipCounter)
        {
            case 0:
                _tip = "Remember to look up";
                _tipCounter++;
                return;
            case 1:
                _tip = "Don't forget to scan the corners";
                _tipCounter++;
                return;
            case 2:
                _tip = "Remember to keep objects within the scanning range: 0.85 to 3 meters away from the HoloLens";
                _tipCounter++;
                return;
            case 3:
                _tip =
                    "The progress bar corresponds to the minimum amount of scanning required, but feel free to do some more!";
                _tipCounter++;
                return;
            case 4:
                _tip =
                    "If you see an area with a gap in the mesh, try focusing on it from different angles and distances to fully scan it";
                _tipCounter = 0;
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


    private void Update_Display(float deltaTime)
    {
        // Basic checks
        if (MainText == null)
        {
            return;
        }

        MainText.GetComponent<Text>().text = PrimaryText;
        if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Finishing) ||
            (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done))
        {
            TipsText.GetComponent<Text>().text = "";
        }
        else
            TipsText.GetComponent<Text>().text = _tip;


        //MainText.GetComponent<Text>().text = (foregroundImage.fillAmount).ToString() + " tot " + stats.TotalSurfaceArea + " floor " + stats.HorizSurfaceArea + " wall " + stats.WallSurfaceArea;
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
        {
            float totalArea = kMinHorizAreaForComplete + kMinWallAreaForComplete;

            if (_progressBarOldfill < 1)
            {
                float fillAmount = 0;
                float floor;
                float wall;

                SpatialUnderstandingDll.Imports.PlayspaceStats stats =
                    SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

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
                _progressBarOldfill = fillAmount;
            }
        }
    }

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
        switch (eventData.RecognizedText.ToLower())
        {
            case "finish scan":
                FinishScan();
                break;
            case "main menu":
                SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = false;
                SceneManager.LoadScene(0);
                break;
            case "skip scan":
                SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = false;
                SceneManager.LoadScene(2);
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
            canvas.transform.position =
                Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2,
                    Camera.main.nearClipPlane + 2));
            SpatialUnderstanding.Instance.RequestFinishScan();
        }
    }
}