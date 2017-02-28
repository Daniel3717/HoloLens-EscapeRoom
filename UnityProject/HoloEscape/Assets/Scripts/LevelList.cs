using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelList
{
    public List<LevelListItem> data;
    string[] placements;
}

[Serializable]
public class LevelListItem
{
    public int id;
    public string name;
    public string description;
    public Author author;
}

[Serializable]
public class Author
{
    public string name;
    public string picture;
}