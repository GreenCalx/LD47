using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILooperState : MonoBehaviour
{
    public readonly string RECORD_MOD_LABEL = "RECORD";
    public readonly string REPLAY_MOD_LABEL = "REPLAY";
    public readonly string REWIND_MOD_LABEL = "REWIND";
    public TextMeshProUGUI __text;

    // Start is called before the first frame update
    void Start()
    {
        __text = GetComponentInChildren<TextMeshProUGUI>();
        setToRecording();
    }

    public void setToRecording()
    {
        if (!!__text)
            __text.text = RECORD_MOD_LABEL;
    }
    public void setToReplay()
    {
        if (!!__text)
            __text.text = REPLAY_MOD_LABEL;
    }

    public void setToEmpty()
    {
        if (!!__text)
            __text.text = "";
    }

    public void setToRewind()
    {
        if (!!__text)
            __text.text = REWIND_MOD_LABEL;
    }

}
