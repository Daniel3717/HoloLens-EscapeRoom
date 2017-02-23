using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[System.Serializable]
public class Item {
    public string name;
    public Button.ButtonClickedEvent thingsToDo;
}


public class CreateScrollList : MonoBehaviour {

    public GameObject sampleButton;
    public List<Item> itemList;

    public Transform contentPanel;

	// Use this for initialization
	void Start () {
        PopulateList();
	}

    void PopulateList()
    {
        foreach(var item in itemList) {
            GameObject newButton = Instantiate(sampleButton) as GameObject;
            SampleButtonScript labelButton = newButton.GetComponent<SampleButtonScript>();
            labelButton.label.text = item.name;
            labelButton.button.onClick = item.thingsToDo;
            newButton.transform.SetParent(contentPanel, false);
        }
    }

    public void SomethingToDo()
    {
        Debug.Log("test");
    }
}
