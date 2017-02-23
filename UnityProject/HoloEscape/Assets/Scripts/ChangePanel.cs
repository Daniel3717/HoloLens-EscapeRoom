using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ChangePanel : MonoBehaviour
{
    
    public GameObject oldPanel;
    public GameObject newPanel;

    // Use this for initialization
    void Start()
    {
        oldPanel.SetActive(false);
        newPanel.SetActive(true);
    }
}
