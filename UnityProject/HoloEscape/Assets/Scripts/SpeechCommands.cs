using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpeechCommands : MonoBehaviour {

    void Start()
    {
        InputManager.Instance.AddGlobalListener(gameObject);
    }

}
