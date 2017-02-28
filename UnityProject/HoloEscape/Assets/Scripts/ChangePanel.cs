using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePanel : MonoBehaviour {

    public GameObject currentPanel;
    public GameObject newPanel;

    public void ChangeActivePanel()
    {

        currentPanel = GameObject.FindGameObjectWithTag("CurrentPanel");
        currentPanel.SetActive(false);
        currentPanel.tag = "Untagged";
        newPanel.SetActive(true);
        newPanel.tag = "CurrentPanel";
    }
}
