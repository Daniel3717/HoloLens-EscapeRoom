using System.Collections;
using System.Collections.Generic;
using System.IO;
using Clues;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomInterpreter : MonoBehaviour
{
    private readonly Dictionary<int, GameObject> _clueObjects = new Dictionary<int, GameObject>();
    private readonly List<ClueToPlace> _cluesToPlace = new List<ClueToPlace>();

    private Text _errorText;

    private WWW _www;

    public Transform ContentPanel;

    public bool CoroutineFinished;
    
    // ErrorPanel will be set to display an error message to the user if something goes wrong
    public GameObject ErrorPanel;
    // NewPanel is set to active when a button in oldPanel is clicked,
    // it will be populated with information that depends on which button was clicked
    public GameObject PostRoomPanel;

    // oldPanel corresponds to the panel that contains a list of buttons corresponding to the games available from the server
    public GameObject RoomPanel;
    // prefab to a sample button to be initialised at a later point
    public GameObject SampleButton;

    // Use this for initialization
    private void Start()
    {
        _errorText = ErrorPanel.GetComponentInChildren<Text>();
        GetJsonFromUrl("http://api.holoescape.tk/v1/games/published");
    }

    private void GetJsonFromUrl(string url)
    {
        _www = new WWW(url);

        StartCoroutine(WaitForRequest(_www));
    }

    private IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            RoomsFromJson(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
            Debug.Log(www.text);
            ChangeActivePanel(ErrorPanel);
            _errorText.text = www.error;
        }
        CoroutineFinished = true;
    }

    private void RoomsFromJson(string levelsJson)
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


        var levelList = JsonUtility.FromJson<LevelList>(levelsJson);

        foreach (var level in levelList.data)
        {
            // for each level in the json levelList, create a new button and add it to the list in the RoomPanel
            var newButton = Instantiate(SampleButton);
            var labelButton = newButton.GetComponent<SampleButtonScript>();
            labelButton.label.text = level.name;

            // Add functionality to the Button in the new RoomPanel as well as the button and Texts in the PostRoomPanel
            labelButton.button.onClick.AddListener(() =>
            {
                // the Button changes the current visible Panel from RoomPanel to PostRoomPanel
                ChangeActivePanel(PostRoomPanel);

                // set Text corresponding to the level name, author name and level description
                var postRoomPanelText = PostRoomPanel.GetComponentsInChildren<Text>();
                postRoomPanelText[0].text = level.name;
                postRoomPanelText[1].text = "By " + level.author.name;
                postRoomPanelText[2].text = level.description;

                // newPanelText[1].GetComponentInChildren < Image >() = level.author.picture;

                // Add functionality to the Button component in the PostRoomPanel (the Continue button)
                var confirmRoomButton = PostRoomPanel.GetComponentInChildren<Button>();
                confirmRoomButton.onClick.AddListener(() => { GetRoom(level.id); });
            });
            // set the Parent of the newly created button to be the Panel containing the list of rooms - dynamically increase the list
            // the ContentPanel is a child of the RoomPanel
            newButton.transform.SetParent(ContentPanel, false);
        }
    }

    private void GetRoom(int id)
    {
        var url = "http://api.holoescape.tk/v1/levels/" + id;

        var form = new WWWForm();
        var headers = new Dictionary<string, string>();
        Debug.Log(SystemInfo.deviceUniqueIdentifier);
        headers.Add("Device-Code", SystemInfo.deviceUniqueIdentifier);
        form.AddField("game_id", id);
        var b = new byte[1];
        var www = new WWW(url, b, headers);

        StartCoroutine(PostWaitForRequest(www));
    }

    private IEnumerator PostWaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            InterpretRoomFromJson(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
            ChangeActivePanel(ErrorPanel);
            _errorText.text = www.error + www.text;
        }
        CoroutineFinished = true;
    }

    private void InterpretRoomFromJson(string leveljson)
    {
        Debug.Log(leveljson);
        //leveljson = File.ReadAllText(Application.dataPath + "/william_test_level.json");
        var leveldata = JsonUtility.FromJson<JsonLevelData>(leveljson);
        var level = leveldata.data;

        foreach (var clue in level.clues)
        {
            Debug.Log(clue.name);
            //GameObject button =  as GameObject;
            string identifier = clue.identifier;
            //identifier = "Clues.Base.Button";
            string path = IdentifierToPath(identifier);

            var clueObject = Instantiate(Resources.Load(path, typeof(GameObject))) as GameObject;
            foreach (var property in clue.initial_properties)
            {
                clueObject.BroadcastMessage("SetProperty", property);
            }
            clueObject.BroadcastMessage("Initialise");
            _clueObjects.Add(clue.id, clueObject);
        }

        int i = 0;//to remove
        // Connect event outlets
        foreach (var clue in level.clues)
        {
            foreach (var events in clue.event_outlets)
                foreach (var outlet in events.outlets)
                    _clueObjects[clue.id].BroadcastMessage("AddAction",
                        new TriggerAction(events.event_name, _clueObjects[outlet.clue_id], outlet.action_name));
            _cluesToPlace.Add(new ClueToPlace(_clueObjects[clue.id], clue.placement));
            /*string lPlace = "";
            if (i % 3 == 0)
                lPlace = "air";
            if (i % 3 == 1)
                lPlace = "platform";
            _cluesToPlace.Add(new ClueToPlace(_clueObjects[clue.id], new List<string> { lPlace })); //hard coded
            i++;*/
        }
        // Place objects by Passing _cluesToPlace.ToArray() to Daniel
        ClueToPlace[] clueArray = _cluesToPlace.ToArray();
        //_cluesToPlace[0].clue.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        //_cluesToPlace[1].clue.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        WrapClues lWC = GameObject.Find("Placements").GetComponent<WrapClues>();
        lWC.LoadClues(clueArray);
        Debug.Log("There are "+clueArray.Length+" clues");

        SceneManager.LoadScene(3);
    }

    public string IdentifierToPath(string identifer)
    {
        var idArray = identifer.Split('.');
        return string.Join("/", idArray) + "/" + idArray[idArray.Length - 1];
    }

    public void ChangeActivePanel(GameObject newPanel)
    {

        GameObject currentPanel = GameObject.FindGameObjectWithTag("CurrentPanel");
        currentPanel.SetActive(false);
        currentPanel.tag = "Untagged";
        newPanel.SetActive(true);
        newPanel.tag = "CurrentPanel";
    }
}
