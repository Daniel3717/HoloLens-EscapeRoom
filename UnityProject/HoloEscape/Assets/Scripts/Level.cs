using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
    public string name;
    public string description;
    public List<Clue> clues;

    public Level ()
    {
    }

    public Level (string name, string description, List<Clue> clues)
    {
        this.name = name;
        this.description = description;
        this.clues = clues;
    }
}

[Serializable]
public class Clue
{

    public int id;
    public string clue_type;
    public List<Property> initial_properties;
    public List<Events> events;
    public List<string> placements;

    public Clue()
    {
    }
    public Clue(int id,
        string clue_type,
        List<Property> initial_properties,
        List<Events> events,
        List<string> placements)
    {
        this.id = id;
        this.clue_type = clue_type;
        this.initial_properties = initial_properties;
        this.events = events;
        this.placements = placements;
    }
}

[Serializable]
public class Property
{
    public string name;
    public string value;

    public Property() { }

    public Property(string name, string value)
    {
        this.name = name;
        this.value = value;
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
