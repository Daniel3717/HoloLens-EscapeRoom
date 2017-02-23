using UnityEngine;

namespace Clues {
    
public class Drawer : Clue
{

    public GameObject ObjectToTrigger;
    public string DrawerCode;

    void Start()
    {
        AddAction("OnSelect", ObjectToTrigger, DrawerCode);
    }

    void OnSelect()
    {
        Trigger("OnSelect");
    }

}
}
