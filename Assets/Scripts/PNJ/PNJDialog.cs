using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PNJDialog : MonoBehaviour
{

    // ID of the dialog to load ( cf. DialogBank )
    public int dialog_id;
    // name to display in the dialog UI header
    public string npc_name;
    // UI to load to show dialog
    public GameObject dialogUI;
    // SFX dialog to play when talking
    public AudioSource[] voices;


    private bool __is_talkable;
    private bool __dialog_ongoing;
    private string[] __dialog;
    private int __curr_dialog_index;
    private UIDialog __loaded_dialog_ui;


    // Start is called before the first frame update
    void Start()
    {
        __is_talkable       = false;
        __dialog_ongoing    = false;
        __dialog = DialogBank.load(dialog_id);

    }

    // Update is called once per frame
    void Update()
    {
        if ( __is_talkable && Input.GetKeyDown(KeyCode.T) )
        {
            talk(); 
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var PC = other.gameObject.GetComponent<StageSelector>();
        if ( PC )
        {
            __is_talkable = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var PC = other.gameObject.GetComponent<StageSelector>();
        if ( PC )
        {
            __is_talkable = false;
        }
    }

    private void talk()
    {
        if (!__dialog_ongoing)
        {
            __dialog_ongoing    = true;
            GameObject ui_go = Instantiate(dialogUI);
            __loaded_dialog_ui  = ui_go.GetComponent<UIDialog>();
            __curr_dialog_index = 0;
        }

        if (__curr_dialog_index >= __dialog.Length )
            end_dialog();

        if (!!__loaded_dialog_ui)
            __loaded_dialog_ui.display( npc_name, __dialog[__curr_dialog_index] );
        
        playVoice();

        __curr_dialog_index++;
    }

    private void playVoice()
    {
        if ( (voices != null) && (voices.Length > 0 ) )
        {
            var rand = new System.Random();
            int voice_to_play = rand.Next(0, voices.Length);
            voices[voice_to_play].Play();
        }
    }

    private void end_dialog()
    {
        __dialog_ongoing = false;
        Destroy(__loaded_dialog_ui.gameObject);
        __curr_dialog_index  = 0;
    }

}
