using System;
using System.Collections.Generic;

[Serializable]
public class JsonLevelData
{
    public JsonLevel data;
}

[Serializable]
public class JsonLevel
{
    public List<JsonClue> clues;
    public string code;
    public string connect_code;
    public string description;
    public string subtitle;
    public string title;

    public JsonLevel()
    {
    }

    public JsonLevel(string title, string description, List<JsonClue> clues)
    {
        this.title = title;
        this.description = description;
        this.clues = clues;
    }
}

[Serializable]
public class JsonClue
{
    public List<JsonEvent> event_outlets;
    public int id;
    public string identifier;
    public List<JsonProperty> initial_properties;
    public string name;
    public List<string> placement;

    public JsonClue()
    {
    }

    public JsonClue(int id,
        string name,
        List<JsonProperty> initial_properties,
        List<JsonEvent> events,
        List<string> placement)
    {
        this.id = id;
        this.name = name;
        this.initial_properties = initial_properties;
        event_outlets = events;
        this.placement = placement;
    }
}

[Serializable]
public class JsonProperty
{
    public string name;
    public string type;
    public string value;

    public JsonProperty()
    {
    }

    public JsonProperty(string name, string type, string value)
    {
        this.name = name;
        this.type = type;
        this.value = value;
    }

    public object GetObject()
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
                return int.Parse(value);
            case "float":
                return float.Parse(value);
            case "string":
                return value;
        }

        return null;
    }
}

[Serializable]
public class JsonEvent
{
    public string event_name;
    public List<JsonOutlet> outlets;

    public JsonEvent()
    {
    }

    public JsonEvent(string event_name, List<JsonOutlet> outlets)
    {
        this.event_name = event_name;
        this.outlets = outlets;
    }
}

[Serializable]
public class JsonOutlet
{
    public string action_name;
    public int clue_id;

    public JsonOutlet()
    {
    }

    public JsonOutlet(int clue_id, string action_name)
    {
        this.clue_id = clue_id;
        this.action_name = action_name;
    }
}