using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using UnityEngine.SceneManagement;

[Serializable]
public class Author
{
    public string name;
    public string picture;
}

[Serializable]
public class LevelList
{
    public List<LevelListItem> data;
}

[Serializable]
public class LevelListItem
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
    public GameObject errorText;

    public Transform contentPanel;

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
            getRooms(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
            errorText.gameObject.SetActive(true);
            errorText.GetComponent<Text>().text = www.error;
        }
        coroutineFinished = true;
    }

    void getRooms(string levelsJson)
    {

        //levels = File.ReadAllText(Application.dataPath + "/levelslist.json");
        //levelsJson = File.ReadAllText(Application.dataPath + "/levelslisttest.json");
        /*levelsJson = @"{ 

    ""data"": [{
		""id"": 10001,
		""name"": ""Test Game"",
		""description"": ""Super Description"",
		""author"": {
			""name"": ""Test User"",
			""picture"": ""http://image.jpg""
        }
	}]";*/
    
        levelList = JsonUtility.FromJson<LevelList>(levelsJson);
        //Debug.Log(levelList.data.Count);
        foreach (var level in levelList.data)
        {
            GameObject newButton = Instantiate(sampleButton) as GameObject;
            SampleButtonScript labelButton = newButton.GetComponent<SampleButtonScript>();
            labelButton.label.text = level.name;
            ChangePanel changePanel = newButton.GetComponent<ChangePanel>();
            changePanel.oldPanel = oldPanel;
            changePanel.newPanel = newPanel;
            labelButton.button.onClick.AddListener(() => {
                oldPanel.SetActive(false);
                newPanel.SetActive(true);

                Text[] newPanelText = newPanel.GetComponentsInChildren<Text>();
                newPanelText[0].text = level.name;
                newPanelText[1].text = level.author.name;
                //newPanelText[1].GetComponentInChildren < Image >() = level.author.picture;
                newPanelText[2].text = level.description;

                Button continueButton = newPanel.GetComponentInChildren<Button>();
                continueButton.onClick.AddListener(() =>
                                                    {
                                                        getRoom(level.id);
                                                    });
            });
            newButton.transform.SetParent(contentPanel, false);
            //Debug.Log(level.name);
        }
    }

    void getRoom(int id)
    {
        string url = "http://api.holoescape.tk/v1/levels/" + id.ToString();

        WWWForm form = new WWWForm();
        Dictionary<string, string> headers = new Dictionary<string, string>();
        Debug.Log(SystemInfo.deviceUniqueIdentifier);
        headers.Add("Device-Code", SystemInfo.deviceUniqueIdentifier);
        form.AddField("game_id", id);
        byte[] b = new byte[1];
        WWW www = new WWW(url, b, headers);

        StartCoroutine(PostWaitForRequest(www));
    }

    IEnumerator PostWaitForRequest(WWW www)
    {

        yield return www;

        // check for errors
        if (www.error == null)
        {
            interpretRoom(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
            errorText.gameObject.SetActive(true);
            errorText.GetComponent<Text>().text = www.error;
        }
        coroutineFinished = true;
    }

    List<GameObject> clueObjects;
    ArrayList c2;
    Dictionary<int, GameObject> c3 = new Dictionary<int, GameObject>();

    private void interpretRoom(string leveljson)
    {
        Debug.Log(leveljson);
        LevelData leveldata = JsonUtility.FromJson<LevelData>(leveljson);
        Level level = leveldata.data;

        foreach (Clue clue in level.clues)
        {
            Debug.Log(clue.name);
            GameObject clueObject = Instantiate(Resources.Load(clue.identifier)) as GameObject;
            foreach (Property property in clue.initial_properties)
            {
                clueObject.SendMessage("setProperty", property);
            }
            //clueObjects
            c3.Add(clue.id, clueObject);
            //clueObjects.Add(clueObject);
        }

        // Connect event outlets

        // Place objects

        SceneManager.LoadScene(4);
    }
}
