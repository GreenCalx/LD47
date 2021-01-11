using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIDialog : MonoBehaviour
{
    public float wait_time_to_print_char = 0.1f;
    [HideInInspector]
    public string overflowing_text = "";
    [HideInInspector]
    public bool overflows = false;

    private readonly string MESSAGE_GO_LABEL = "MESSAGE";
    private readonly string HEADER_GO_LABEL  = "HEADER";
    private Text __message;
    private Text __header;

    private string  __msg_to_display;
    private int     __msg_size;
    private int     __curr_msg_index;
    private float   __timer;
    private bool    __text_fully_displayed;
    private bool    __overflow_checked;

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
        __msg_to_display = "";
        __curr_msg_index = 0;
        __text_fully_displayed = false;
        __overflow_checked =false;
    }

    // Update is called once per frame
    void Update()
    {
        __timer += Time.deltaTime;
        if ( __timer > wait_time_to_print_char )
        {
            if ( __curr_msg_index <= __msg_size )
            {
                __message.text = __msg_to_display.Substring( 0, __curr_msg_index);
                __curr_msg_index++;
                __text_fully_displayed = (__curr_msg_index > __msg_size);
            }
            __timer -= wait_time_to_print_char;
        }
        if ( message_is_displayed() && !__overflow_checked )
            updateVerticalOverflow();
    }

    public void force_display()
    {
        __message.text      = __msg_to_display;
        __curr_msg_index    = __msg_size;
        __text_fully_displayed = true;
    }

    public bool message_is_displayed()
    {
        return __text_fully_displayed;
    }

    public bool has_a_message_to_display()
    {
        return (__msg_to_display.Length != 0) ;
    }

    public void display( string iHeader, string iText )
    {
        if (!!__header)
            __header.text = iHeader;

        __msg_to_display    = iText;
        __msg_size          = iText.Length;
        __curr_msg_index    = 0;
        __text_fully_displayed = false;
        __overflow_checked = false;
    }

    public void updateVerticalOverflow()
    {
        Canvas.ForceUpdateCanvases();
        int n_visible = __message.cachedTextGenerator.characterCountVisible;
        overflows = n_visible < __msg_to_display.Length;
        if ( overflows )
            overflowing_text = __msg_to_display.Substring( n_visible );
        __overflow_checked = true;
    }
}
