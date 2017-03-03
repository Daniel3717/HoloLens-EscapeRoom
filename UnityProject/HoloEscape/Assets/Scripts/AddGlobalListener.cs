using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class AddGlobalListener : MonoBehaviour {

    void Start()
    {
        InputManager.Instance.AddGlobalListener(gameObject);
    }

}
