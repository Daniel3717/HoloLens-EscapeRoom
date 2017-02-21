using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Event
{
    enum testenum { one, two, three };
    public string event_name;
    public Outlet[] outlets;
}