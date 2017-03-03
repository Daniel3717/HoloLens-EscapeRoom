using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

public class ShowConnectCode : MonoBehaviour
{
    public TextMesh ConnectCodeTextMesh;
    public GameObject ConnectCodePanel;
    public Text ConnectCodeLabelText;

    // Use this for initialization
    void Start () {
        AnyInfo info = GameObject.Find("InfoObject").GetComponent<AnyInfo>();
        ConnectCodeLabelText.text += " " + info.someinfo;
        ConnectCodeTextMesh.text = "Connect Code: " + info.someinfo;
        StartCoroutine(DisplayForSeconds(10));
    }
	
    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch (eventData.RecognizedText.ToLower())
        {
            case "show connect code":
                //if(ConnectCodePanel.active == false)
                if (ConnectCodeTextMesh.gameObject.active == false) {
                    StartCoroutine(DisplayForSeconds(10));
                    Debug.Log("show connect code");
                }
        break;
        }
        //Debug.Log(SpatialUnderstanding.Instance == null);
    }

    IEnumerator DisplayForSeconds(int seconds)
    {
        //ConnectCodePanel.SetActive(true);
        ConnectCodeTextMesh.gameObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        ConnectCodeTextMesh.gameObject.SetActive(false);
        //ConnectCodePanel.SetActive(false);
    }
}
