using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class CameraFunctionality : MonoBehaviour, ISpeechHandler
{
    private WWW _www;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void GetfromURL(string url)
    {
        _www = new WWW(url);

        StartCoroutine(WaitForRequest(_www));
    }

    private IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            Debug.Log(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
            Debug.Log(www.text);
        }
    }

    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch (eventData.RecognizedText.ToLower())
        {
            case "take picture":
                GetfromURL("http://localhost/api/holographic/mrc/files");
                break;
        }
    }
}