using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueToPlace
{
    public GameObject clue;
    public List<string> placements;

    public ClueToPlace(GameObject clue, List<string> placements)
    {
        this.clue = clue;
        this.placements = placements;
    }
}
