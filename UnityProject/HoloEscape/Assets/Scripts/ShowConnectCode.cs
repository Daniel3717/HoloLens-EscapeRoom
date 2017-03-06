using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

public class ShowConnectCode : MonoBehaviour, ISpeechHandler
{
    public TextMesh ConnectCodeTextMesh;

    // Use this for initialization
    void Start()
    {
        AnyInfo info = GameObject.Find("InfoObject").GetComponent<AnyInfo>();
        ConnectCodeTextMesh.text = "Connect Code: " + info.Someinfo;
        StartCoroutine(DisplayForSeconds(10));
    }

    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch (eventData.RecognizedText.ToLower())
        {
            case "show connect code":

                Debug.Log("tried show connect code");
                if (ConnectCodeTextMesh.gameObject.activeSelf == false)
                {
                    StartCoroutine(DisplayForSeconds(10));
                    Debug.Log("show connect code");
                }
                break;
        }
    }

    IEnumerator DisplayForSeconds(int seconds)
    {
        ConnectCodeTextMesh.gameObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        ConnectCodeTextMesh.gameObject.SetActive(false);
    }
}
