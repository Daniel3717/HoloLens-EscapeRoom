using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;

[Serializable]
public class LevelList
{
    public List<LevelItem> levels;
}

[Serializable]
public class LevelItem
{
    public int id;
    public string name;
    public string description;
    public Button.ButtonClickedEvent thingsToDo;
}

public class RoomInterpreter : MonoBehaviour {

    public GameObject sampleButton;
    public LevelList levelList;

    public Transform contentPanel;

    // Use this for initialization
    void Start()
    {
        
        getRooms();
        /*
        LevelList levelList = new LevelList();
        levelList.levels = new List<LevelItem>();
        LevelItem level1 = new LevelItem();
        level1.name = "Level1";
        level1.id = 1;
        level1.description = "this is Level1's description";
        LevelItem level2 = new LevelItem();
        level2.name = "Level1";
        level2.id = 2;
        level2.description = "this is Level2's description";
        levelList.levels.Add(level1);
        levelList.levels.Add(level2);
        Debug.Log(JsonUtility.ToJson(levelList));
        */
    }

    void getRooms()
    { 
        string levels = File.ReadAllText(Application.dataPath + "/levelslist.json");
        levelList = JsonUtility.FromJson<LevelList>(levels);
        Debug.Log(levelList.levels.Count);
        foreach (var level in levelList.levels)
        {
            GameObject newButton = Instantiate(sampleButton) as GameObject;
            SampleButtonScript labelButton = newButton.GetComponent<SampleButtonScript>();
            labelButton.label.text = level.name;
            labelButton.button.onClick = level.thingsToDo;
            newButton.transform.SetParent(contentPanel, false);
            Debug.Log(level.name);
        }
    }

}
