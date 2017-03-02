using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToggleMesh : MonoBehaviour, ISpeechHandler {

	// Use this for initialization
	void Start () {

        InputManager.Instance.AddGlobalListener(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch (eventData.RecognizedText.ToLower())
        {
            case "toggle mesh":
                SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh =
                    !SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh;
                //SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = true;
                break;
        }
        //Debug.Log(SpatialUnderstanding.Instance == null);
    }
}
