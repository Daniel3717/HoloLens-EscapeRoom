using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.SceneManagement;

public class GoToMainMenu : ISpeechHandler
{
    
    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        switch (eventData.RecognizedText.ToLower())
        {
            case "main menu":
                SceneManager.LoadScene(0);
                break;
        }
    }
}
