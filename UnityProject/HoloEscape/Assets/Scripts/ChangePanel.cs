using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePanel : MonoBehaviour {
    
    public GameObject NewPanel;

    public void ChangeActivePanel()
    {

        GameObject CurrentPanel = GameObject.FindGameObjectWithTag("CurrentPanel");
        CurrentPanel.SetActive(false);
        CurrentPanel.tag = "Untagged";
        NewPanel.SetActive(true);
        NewPanel.tag = "CurrentPanel";
    }
}
