using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour, ISpeechHandler
{
    void Start()
    {
    }
    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch (eventData.RecognizedText.ToLower())
        {
            case "main menu":
                SceneManager.LoadScene(0);
                break;
            case "rooms menu":
                SceneManager.LoadScene(2);
                break;
        }
    }
}
