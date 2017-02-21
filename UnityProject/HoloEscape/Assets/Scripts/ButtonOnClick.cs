using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClick : MonoBehaviour {

    public Button yourButton;

    public void DoOnClick()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.Invoke();
        
    }
}
