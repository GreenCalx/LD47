using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneController : MonoBehaviour, IControllable
{

    public const float SKIP_TIMER = 0.5f;
    private float skip_elapsed_time = 0f; // for inputs
    private double elapsed_time = 0f; // for regular cutscene timers

    public CutScene cutScene;

    // Start is called before the first frame update
    void Start()
    {
        skip_elapsed_time = 0f;
        elapsed_time = 0f;

        if ( cutScene == null )
        {
            Debug.LogError("Missing CutScene !!!! FULL BROKO ");
        }

        cutScene.launch();
    }

    // Update is called once per frame
    void Update()
    {
        if (cutScene.curr_elem==null)
            return;

        elapsed_time += Time.deltaTime;
        if ( elapsed_time >= cutScene.curr_elem.Item1 )
        {
            go_next();
        }
    }



    void IControllable.ProcessInputs(Save.InputSaver.InputSaverEntry Entry)
    {
        var Up      = Entry.Inputs["Up"].IsDown || Entry.isDpadUpPressed;
        var Down    = Entry.Inputs["Down"].IsDown || Entry.isDpadDownPressed;
        var Right   = Entry.Inputs["Right"].IsDown || Entry.isDpadRightPressed;
        var Left    = Entry.Inputs["Left"].IsDown || Entry.isDpadLeftPressed;
        var Submit  = Entry.Inputs["Break"].IsDown || Entry.isDpadLeftPressed;

        if ( Up || Down || Right || Left || Submit )
        {
            skip_elapsed_time += Time.deltaTime;
            if (skip_elapsed_time > SKIP_TIMER)
            {
                go_next();
            }
        } else {
            skip_elapsed_time = 0f;
        }
    }

    public void go_next()
    {
        cutScene.loadNext();
        skip_elapsed_time = 0f;
        elapsed_time = 0f;
    }
    
}
