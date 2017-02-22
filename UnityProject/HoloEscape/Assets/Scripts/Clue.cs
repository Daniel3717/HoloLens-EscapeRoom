using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Clue {

    public int id;
    public string clue_type;
    public Property[] initial_properties;
    public Events[] events;
    public string[] placement;
}
