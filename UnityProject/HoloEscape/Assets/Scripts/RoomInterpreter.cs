using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using Clues;
using HoloToolkit.Unity.InputModule;

public class RoomInterpreter : MonoBehaviour {

    // oldPanel corresponds to the panel that contains a list of buttons corresponding to the games available from the server
    public GameObject roomPanel;
    // newPanel is set to active when a button in oldPanel is clicked,
    // it will be populated with information that depends on which button was clicked
    public GameObject postRoomPanel;
    // prefab to a sample button to be initialised at a later point
    public GameObject sampleButton;
    // errorPanel will be set to display an error message to the user if something goes wrong
    public GameObject errorPanel;
    Text errorText;

    ChangePanel panelChanger = new ChangePanel();

    public GameObject currentPanel;

    public Transform contentPanel;

    public bool coroutineFinished = false;

    public GameObject button;

    WWW www;

    // Use this for initialization
    void Start()
    {
        errorText = errorPanel.GetComponentInChildren<Text>();
        getJsonFromURL("http://api.holoescape.tk/v1/games/published");
    }

    void getJsonFromURL(string url)
    {
        www = new WWW(url);

        StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            roomsFromJson(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
            Debug.Log(www.text);
            panelChanger.ChangeActivePanel(currentPanel, errorPanel);
            errorText.text = www.error;
        }
        coroutineFinished = true;
    }

    void roomsFromJson(string levelsJson)
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


        LevelList levelList = JsonUtility.FromJson<LevelList>(levelsJson);

        foreach (LevelListItem level in levelList.data)
        {
            // for each level in the json levelList, create a new button and add it to the list in the roomPanel
            GameObject newButton = Instantiate(sampleButton) as GameObject;
            SampleButtonScript labelButton = newButton.GetComponent<SampleButtonScript>();
            labelButton.label.text = level.name;

            KeywordManager keywordManager = roomPanel.GetComponent<KeywordManager>();
            KeywordManager.KeywordAndResponse[] newResponses = new KeywordManager.KeywordAndResponse[keywordManager.KeywordsAndResponses.Length + 1];

            //keywordManager.KeywordsAndResponse

            // Add functionality to the Button in the new roomPanel as well as the button and Texts in the postRoomPanel
            labelButton.button.onClick.AddListener(() => {
                // the Button sets changes the current visible Panel from roomPanel to postRoomPanel
                panelChanger.ChangeActivePanel(currentPanel, postRoomPanel);

                // set Text corresponding to the level name, author name and level description
                Text[] postRoomPanelText = postRoomPanel.GetComponentsInChildren<Text>();
                postRoomPanelText[0].text = level.name;
                postRoomPanelText[1].text = level.author.name;
                postRoomPanelText[2].text = level.description;

                // newPanelText[1].GetComponentInChildren < Image >() = level.author.picture;

                // Add functionality to the Button component in the postRoomPanel (the Continue button)
                UnityEngine.UI.Button continueButton = postRoomPanel.GetComponentInChildren<UnityEngine.UI.Button>();
                continueButton.onClick.AddListener(() =>
                                                    {
                                                        getRoom(level.id);
                                                    });
            });
            // set the Parent of the newly created button to be the Panel containing the list of rooms - dynamically increase the list
            // the contentPanel is a child of the roomPanel
            newButton.transform.SetParent(contentPanel, false);
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
            interpretRoomFromJson(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
            panelChanger.ChangeActivePanel(currentPanel, errorPanel);
            errorText.text = www.error;
        }
        coroutineFinished = true;
    }

    Dictionary<int, GameObject> clueObjects = new Dictionary<int, GameObject>();
    List<ClueToPlace> cluesToPlace = new List<ClueToPlace>();

    private void interpretRoomFromJson(string leveljson)
    {
        Debug.Log(leveljson);
        JsonLevelData leveldata = JsonUtility.FromJson<JsonLevelData>(leveljson);
        JsonLevel level = leveldata.data;

        foreach (JsonClue clue in level.clues)
        {
            Debug.Log(clue.name);
            //GameObject button =  as GameObject;
            //string identifier = "Clues.Base.Button";
            string identifier = clue.identifier;
            string path = IdentifierToPath(identifier);

            GameObject clueObject = Instantiate(Resources.Load(path, typeof(GameObject))) as GameObject;
            foreach (JsonProperty property in clue.initial_properties)
            {

                Clues.Base.Button.Button bton = new Clues.Base.Button.Button();
                bton.SetProperty(property);
                if(clueObject == null)
                {
                    Debug.Log("clueObject null");
                }
                if (property == null)
                {
                    Debug.Log("property null");
                }
                clueObject.BroadcastMessage("SetProperty", property);
            }

            clueObjects.Add(clue.id, clueObject);

        }

        // Connect event outlets
        foreach (JsonClue clue in level.clues)
        {
            foreach (JsonEvent events in clue.event_outlets)
            {
                foreach (JsonOutlet outlet in events.outlets)
                {
                    clueObjects[clue.id].BroadcastMessage("AddAction", new TriggerAction(events.event_name, clueObjects[outlet.clue_id], outlet.action_name));
                }
            }
            cluesToPlace.Add(new ClueToPlace(clueObjects[clue.id], clue.placement));
        }
        // Place objects
        // By Passing cluesToPlace.ToArray() to Daniel
        LoadClues(cluesToPlace.ToArray());

        SceneManager.LoadScene(4);
    }

    public string IdentifierToPath(string identifer)
    {
        string[] idArray;
        idArray = identifer.Split('.');
        return String.Join("/", idArray) + "/" + idArray[idArray.Length - 1];
    }
}