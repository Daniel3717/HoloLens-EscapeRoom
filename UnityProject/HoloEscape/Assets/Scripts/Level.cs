using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public Level data;
}

[Serializable]
public class Level
{
    public string code;
    public string connect_code;
    public string title;
    public string subtitle;
    public string description;
    public List<Clue> clues;

    public Level ()
    {
    }

    public Level (string title, string description, List<Clue> clues)
    {
        this.title = title;
        this.description = description;
        this.clues = clues;
    }
}

[Serializable]
public class Clue
{

    public int id;
    public string identifier;
    public string name;
    public List<Property> initial_properties;
    public List<Events> event_outlets;
    public List<string> placement;

    public Clue()
    {
    }
    public Clue(int id,
        string name,
        List<Property> initial_properties,
        List<Events> event_outlets,
        List<string> placement)
    {
        this.id = id;
        this.name = name;
        this.initial_properties = initial_properties;
        this.event_outlets = event_outlets;
        this.placement = placement;
    }
}

[Serializable]
public class Property
{
    public string name;
    public string type;
    public string value;

    public Property() { }

    public Property(string name, string type, string value)
    {
        this.name = name;
        this.type = type;
        this.value = value;
    }

    public object getObject(string type, string value)
    {
        switch (type)
        {
            case "bool":
                {
                    switch (value)
                    {
                        case "true":
                            return true;
                        case "false":
                            return false;
                        default:
                            return null;
                    }
                }
            case "int":
                return Int32.Parse(value);
            case "float":
                return float.Parse(value);
            case "string":
                return value;
        }

        return null;
    }
}

[Serializable]
public class Events
{
    public string event_name;
    public List<Outlet> outlets;

    public Events() { }

    public Events (string event_name, List<Outlet> outlets)
    {
        this.event_name = event_name;
        this.outlets = outlets;
    }
}

[Serializable]
public class Outlet
{
    public int clue_id;
    public string action_name;

    public Outlet() { }

    public Outlet(int clue_id, string action_name)
    {
        this.clue_id = clue_id;
        this.action_name = action_name;
    }
}
