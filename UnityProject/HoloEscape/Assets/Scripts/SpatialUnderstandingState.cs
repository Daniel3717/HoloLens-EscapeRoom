using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>
{
    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;

    private bool _triggered;

    public string PrimaryText
    {
        get
        {
            return "PrimaryText";
        }
    }

    public Color PrimaryColor
    {
        get
        {
            return Color.white;
        }
    }

    public string DetailsText
    {
        get
        {
            return "DetailsText";
        }
    }

    private void Update_DebugDisplay()
    {
        // Basic checks
        if (DebugDisplay == null)
        {
            return;
        }

        // Update display text
        DebugDisplay.text = PrimaryText;
        DebugDisplay.color = PrimaryColor;
        DebugSubDisplay.text = DetailsText;
    }

    // Update is called once per frame
    private void Update()
    {
        // Updates
        Update_DebugDisplay();
    }
}