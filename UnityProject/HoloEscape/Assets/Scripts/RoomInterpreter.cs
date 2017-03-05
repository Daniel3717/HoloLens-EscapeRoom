using System;
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

    // PostRoomPanel is set to active when a button in oldPanel is clicked,
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
        // Send GET request to url
        _www = new WWW(url);

        // Must wait until www is ready before proceeding
        StartCoroutine(WaitForRequest(_www));
    }

    private IEnumerator WaitForRequest(WWW www)
    {
        // Will yield when request is done
        yield return www;

        // check for errors
        if (www.error == null)
        {
            // If no errors, proceed to retrieve rooms from the json file
            RoomsFromJson(www.text);
        }
        else
        {
            // If there's an error display a message to user and to Debug Log
            Debug.Log("WWW Error: " + www.error);
            Debug.Log(www.text);
            ChangeActivePanel(ErrorPanel);
            _errorText.text = "There appears to be a network error " + www.error + " " + www.text;
        }
        CoroutineFinished = true;
    }

    private void RoomsFromJson(string levelsJson)
    {
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

        // Send POST request to url above
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
            // If no errors proceed to interpret rooom information from the json file
            InterpretRoomFromJson(www.text);
        }
        else
        {
            // If there's an error display a message to user and to Debug Log
            Debug.Log("WWW Error: " + www.error);
            Debug.Log(www.text);
            ChangeActivePanel(ErrorPanel);
            _errorText.text = "There appears to be a network error " + www.error + " " + www.text;
        }
        CoroutineFinished = true;
    }

    private void InterpretRoomFromJson(string leveljson)
    {
        Debug.Log(leveljson);
        var leveldata = JsonUtility.FromJson<JsonLevelData>(leveljson);
        var level = leveldata.data;

        // Add connect code to InfoObject that remains throughout scenes - It is used in the GamePlay Scene
        AnyInfo Info = GameObject.Find("InfoObject").GetComponent<AnyInfo>();
        Info.Someinfo = level.connect_code; 
        
        // Initialise each clue
        foreach (var clue in level.clues)
        {
            Debug.Log(clue.name);

            // Use the identifier to loacte the clue in the Assets folder
            string identifier = clue.identifier;
            string path = IdentifierToPath(identifier);

            // Create an instance of the required clue prefab

            try
            {
                var clueObject = Instantiate(Resources.Load(path, typeof(GameObject))) as GameObject;

                // Set the initial properties by broadcasting a message to make use the clue's SetProperty method
                foreach (var property in clue.initial_properties)
                {
                    Debug.Log(property.name);
                    Debug.Log(property.type);
                    Debug.Log(property.value);
                    clueObject.BroadcastMessage("SetProperty", property);
                }
                clueObject.BroadcastMessage("Initialise");

                // Add the initialised clue object to a dictionary that is later used to make connections between the clues
                _clueObjects.Add(clue.id, clueObject);
            }
            catch (ArgumentException e)
            {

                // If any one clue fails, destroy all the others before showing the error panel
                foreach (var clueObjectPair in _clueObjects)
                {
                    Destroy(clueObjectPair.Value);
                }

                // Display Error Message
                ChangeActivePanel(ErrorPanel);
                _errorText.text = "There seems to be a problem with the " + clue.name +
                " clue. Please check that the correct identifier is being used";
            }

        }

        // Only proceed if the ErroPanel isn't active (there were no previous errors)
        if (ErrorPanel.active == false)
        {
            // Connect event outlets
            foreach (var clue in level.clues)
            {
                foreach (var events in clue.event_outlets)
                    foreach (var outlet in events.outlets)
                        // Add an action the clue object in question - this connects one of its methods to trigger an action in another object
                        _clueObjects[clue.id].BroadcastMessage("AddAction",
                            new TriggerAction(events.event_name, _clueObjects[outlet.clue_id], outlet.action_name));
                _cluesToPlace.Add(new ClueToPlace(_clueObjects[clue.id], clue.placement));
            }

            // Place objects by passing _cluesToPlace.ToArray() to Daniel using LoadClues
            ClueToPlace[] clueArray = _cluesToPlace.ToArray();

            // Wrap clues array so it can be used by the LoadClues
            WrapClues lWC = GameObject.Find("Placements").GetComponent<WrapClues>();
            lWC.LoadClues(clueArray);

            SceneManager.LoadScene(3);
        }
    }


    // This function changes the identifier to the path of the required prefab by replacing dots by slashes
    // and adding the name of the last folder to the path to locate the actual prefab
    // (since the folder containing the prefab and the prefab share the same name)
    public string IdentifierToPath(string identifer)
    {
        var idArray = identifer.Split('.');
        return string.Join("/", idArray) + "/" + idArray[idArray.Length - 1];
    }

    // Changes the active panel in the scene to newPanel
    public void ChangeActivePanel(GameObject newPanel)
    {

        GameObject currentPanel = GameObject.FindGameObjectWithTag("CurrentPanel");
        currentPanel.SetActive(false);
        currentPanel.tag = "Untagged";
        newPanel.SetActive(true);
        newPanel.tag = "CurrentPanel";
    }
}
