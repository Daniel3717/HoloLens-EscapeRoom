using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using LitJson;
using System.IO;

public class RoomInterpreter : MonoBehaviour {
    Level myObject;
    //path = EditorUtility.OpenFilePanel("level", "C:/Users/Elias/Documents/Uni/YearTwo/Lent/GroupProject/JSON/", "json");

    public string json;
    public string level;
    string clue;
    public string roomName;

    // Use this for initialization
    void Start()
    {
        Level test = new Level();
        test.description = "test descript";
        test.name = "test name";
        test.clues = new Clue[2];
        test.clues[0] = new Clue();
        test.clues[0].id = 0;
        test.clues[0].clue_type = "bool";
        test.clues[0].initial_properties = new Property[1];
        test.clues[0].initial_properties[0] = new Property();
        test.clues[0].initial_properties[0].name = "property name";
        test.clues[0].initial_properties[0].value = "true";
        test.clues[0].events = new Event[2];
        test.clues[0].events[0].event_name = "on_unlock";
        test.clues[0].events[0].outlets = new Outlet[2];
        test.clues[0].events[0].outlets[0].clue_id = 2;
        test.clues[0].events[0].outlets[0].action_name = "open";
        test.clues[0].events[0].outlets[1].clue_id = 3;
        test.clues[0].placement = new Placement[3];
        test.clues[0]

        string jtest = JsonUtility.ToJson(test);
        json = File.ReadAllText(Application.dataPath + "/test.json");
        clue = File.ReadAllText(Application.dataPath + "/configured_clue.json");
        level = File.ReadAllText(Application.dataPath + "/level.json");
        JsonData jsondata = JsonMapper.ToObject(json);
        JsonData cluedata = JsonMapper.ToObject(clue);
        JsonData leveldata = JsonMapper.ToObject(level);
        //getRoom();
        //Debug.Log(level);
        //Debug.Log(json);
        Debug.Log(jsondata["album"]["artist"]);
        Debug.Log(leveldata["clues"]);
        Debug.Log(jtest);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void getRoom()
    {
        //json = File.ReadAllText(Application.dataPath + "/level.json");
        //JsonData data = JsonMapper.ToObject(level);
        //roomName = (string)data["album"]["artist"];
        //myObject = JsonUtility.FromJson<Level>(json);
    }

}
