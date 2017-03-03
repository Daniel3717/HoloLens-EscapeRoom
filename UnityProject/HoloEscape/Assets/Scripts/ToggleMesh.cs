using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleMesh : MonoBehaviour, ISpeechHandler {

    void Start()
    {

    }
    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch (eventData.RecognizedText.ToLower())
        {
            case "toggle mesh":
                Debug.Log("before " + SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh);
                SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh =
                    !SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh;
                Debug.Log("Mesh Toggled");
                Debug.Log("after " + SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh);
                break;
        }
    }
}
