using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIDialog : MonoBehaviour
{
    private readonly string MESSAGE_GO_LABEL = "MESSAGE";
    private readonly string HEADER_GO_LABEL  = "HEADER";
    private Text __message;
    private Text __header;

    // Start is called before the first frame update
    void Awake()
    {
        Text[] ui_texts = GetComponentsInChildren<Text>();
        foreach( Text t in ui_texts )
        {
            if ( t.gameObject.name == HEADER_GO_LABEL )
                __header = t;
            else if ( t.gameObject.name == MESSAGE_GO_LABEL )
                __message = t;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void display( string iHeader, string iText )
    {
        if (!!__header)
            __header.text = iHeader;
        if (!!__message)
            __message.text = iText;
    }
}
