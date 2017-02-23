using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;

[Serializable]
public class Author
{
    public string name;
    public string picture;
}

[Serializable]
public class LevelList
{
    public List<LevelItem> data;
}

[Serializable]
public class LevelItem
{
    public int id;
    public string name;
    public string description;
    public Author author;
    public Button.ButtonClickedEvent thingsToDo;
}

public class RoomInterpreter : MonoBehaviour {

    public GameObject oldPanel;
    public GameObject newPanel;
    public GameObject sampleButton;
    public LevelList levelList;


    public Transform contentPanel;
    string levels;

    public bool coroutineFinished = false;

    WWW www;

    // Use this for initialization
    void Start()
    {
        getURL();
    }

    void getURL()
    {
        string url = "http://api.holoescape.tk/v1/games";
        www = new WWW(url);

        StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWW www)
    {

        yield return www;

        // check for errors
        if (www.error == null)
        {
            //Debug.Log("WWW Ok!: " + www.text);
            getRooms();
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
        coroutineFinished = true;
    }

    void getRooms()
    {

        levels = File.ReadAllText(Application.dataPath + "/levelslist.json");
        levels = www.text;

        levelList = JsonUtility.FromJson<LevelList>(levels);
        Debug.Log(levelList.data.Count);
        foreach (var level in levelList.data)
        {
            GameObject newButton = Instantiate(sampleButton) as GameObject;
            SampleButtonScript labelButton = newButton.GetComponent<SampleButtonScript>();
            labelButton.label.text = level.name;
            ChangePanel changePanel = newButton.GetComponent<ChangePanel>();
            changePanel.oldPanel = oldPanel;
            changePanel.newPanel = newPanel;
            labelButton.button.onClick.AddListener(() => { oldPanel.SetActive(false); newPanel.SetActive(true); });
            newButton.transform.SetParent(contentPanel, false);
            Debug.Log(level.name);
        }
    }

}
