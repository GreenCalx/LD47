using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInputsPanel : MonoBehaviour
{
    private Text[] textFields;
    // Start is called before the first frame update
    void Start()
    {
        textFields = GetComponentsInChildren<Text>();
    }

    public void setToRecording()
    {
        textFields[0].text = "W A S D";
        textFields[1].text = "Move";
        textFields[2].text = "C";
        textFields[3].text = "Stand";
        textFields[4].text = "R";
        textFields[5].text = "Reset";
    }

    public void setToReplay()
    {
        textFields[0].text = "Q E";
        textFields[1].text = "Forward/Backward time";
        textFields[2].text = "W A S D C";
        textFields[3].text = "Break";
        textFields[4].text = "R";
        textFields[5].text = "Reset";
    }

    public void setToEmpty()
    {
        textFields[0].text = "";
        textFields[1].text = "";
        textFields[2].text = "";
        textFields[3].text = "";
        textFields[4].text = "R";
        textFields[5].text = "Reset";
    }
}
