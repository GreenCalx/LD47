using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeline : MonoBehaviour
{
    // Inputs
    public Sprite ui_input_up;
    public Sprite ui_input_left;
    public Sprite ui_input_right;
    public Sprite ui_input_down;
    public Sprite ui_input_none;

    /// UI
    private UITimelineInput[]       __time_units;
    private UILooperState           __ui_looper_state;
    public UITimelineModFrame       __ui_modframe;
    public UITimelineSwitcher       __ui_tl_switcher;

    public TimelineView             __tl_view;
    public Timeline                 __displayedTimeline;

    private Animator                __timeline_animator;

    /// Model
    private WorldManager __WM;

    private int current_time = 0;
    private int current_timeline_level = 0;

    // Start is called before the first frame update
    void Start()
    {
        current_time = 0;

        __timeline_animator = GetComponentInChildren<Animator>();
        __time_units        = GetComponentsInChildren<UITimelineInput>();

        __ui_looper_state   = GetComponentInChildren<UILooperState>();
        __ui_modframe       = GetComponentInChildren<UITimelineModFrame>();
        __ui_tl_switcher    = GetComponentInChildren<UITimelineSwitcher>();

        __tl_view           = GetComponent<TimelineView>();

        // Attach main camera to canvas
        Canvas c = GetComponent<Canvas>();
        if ( !c ) return;
        if (!gameObject.transform.parent) return;


        int hardloopcount = 0;
        Transform current = gameObject.transform.parent;
        while (current != null && ++hardloopcount < 10) {
            Camera cam = current.gameObject.GetComponentInChildren<Camera>();
            if (!!c && !!cam)
            {
                c.worldCamera = cam;
            }   
            current = current.transform.parent;
            Debug.Log(current);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void refresh(InputManager.Mode Mode)
    {
        updateUI();

        if ((null==__ui_looper_state) || (null==__ui_modframe) || (null==__ui_tl_switcher))
        {
            Debug.Log(" UITimeline.cs l:50 - Missing UI component in refresh. ABORTING refresh().");
            return;
        }

        if (Mode == InputManager.Mode.RECORD)
        {
            __ui_looper_state.setToRecording();
            __ui_modframe.setToRecord();
        }
        else if (Mode == InputManager.Mode.REPLAY)
        {
            __ui_looper_state.setToReplay();
            __ui_modframe.setToReplay();
        }
        else
        {
            __ui_looper_state.setToEmpty();
            __ui_modframe.setToRewind();
        }

        // to see only current timeline
        //__ui_tl_switcher.switchTimeline( TL.loop_level );

        //updateLooperState(playerController.L);
    }

    public int getDisplayedLoopLevel()
    {
        return __displayedTimeline.loop_level;
    }

    public void trySwitchTimeline(Timeline iTL)
    {
        __displayedTimeline = iTL;
        __ui_tl_switcher.switchTimeline( __displayedTimeline.loop_level ); // proceed to switch
        Debug.Log("Display TL " + iTL.loop_level);
        //updateUI(); // notneed mosdt likely
    }

    public void setDisplayedTimeline(Timeline iTL)
    {
        __displayedTimeline = iTL;
        Debug.Log("Display TL " + iTL.loop_level);
    }

    private void updateLooperState( Looper iLooper )
    {
        if ( __ui_looper_state == null )
            return;
        if ( iLooper.Data.IsRecording )
            __ui_looper_state.setToRecording();
        else if ( iLooper.Data.IsRunning )
            __ui_looper_state.setToReplay();
        else
            __ui_looper_state.setToEmpty();
    }

    public void setModel(WorldManager WM)
    {
        this.__WM = WM;
    }

    public void update_time(int iNewTime)
    {
        current_time = iNewTime;
        if (!!__timeline_animator)
        {
            AnimTimelineUI anim_script = __timeline_animator.GetBehaviour<AnimTimelineUI>();
            if (!!anim_script)
                anim_script.updateTime( current_time );
            //__timeline_animator.SetInteger("time", current_time);
        }
    }

    private bool tryRefreshTimeUnits()
    {
        __time_units = GetComponentsInChildren<UITimelineInput>();
        return (__time_units != null);
    }

    // REDO ME
    private void updateUI()
    {
        Timeline active_tl  = __displayedTimeline;
        bool is_previous_tl = active_tl.isPrevious();
        
        // to update time units sprites
        Transform current_tick_square_transform = null;

        if (null==__time_units)
        {
            if (!tryRefreshTimeUnits())
                return;
        }


        for (int i=0; i<__time_units.Length ;i++)
        {
            bool square_is_active = active_tl.getAt(i);
            UITimelineInput curr_unit =__time_units[i];

            // show enabled or disabled for curr unit
            if (!square_is_active)
            {
                __time_units[i].showDisabled();
                continue;
            }

            __time_units[i].showEnabled();

            updateSquareInputImage( i, active_tl.Events[i]);


            // RECORD exclusive logic for current timeline
             if ( __WM.IM.CurrentMode==InputManager.Mode.RECORD && !is_previous_tl )
             {
                // Hide forward squares 
                 if ( i > active_tl.getTickForTimeUnits() )
                 {
                     curr_unit.hide();
                 } 
                // Show none as next input at the last_tick position
                 else if ( i == active_tl.getTickForCursor() )
                 {
                     if ( Constants.ShowDefaultTileOnCursor )
                         __time_units[i].changeSprite(ui_input_none);
                 }
             }



        }//! for time units

        // update timeline animator
        update_time(active_tl.getTickForCursor());

    }//! updateUI

    private void updateSquareInputImage( int i_square, PlayerController.Direction dir )
    {
        if (i_square > __time_units.Length)
            return;
        if ( dir == PlayerController.Direction.UP)
        {
            __time_units[i_square].changeSprite( ui_input_up );
        } else if ( dir == PlayerController.Direction.DOWN)
        {
            __time_units[i_square].changeSprite( ui_input_down );
        } else if ( dir == PlayerController.Direction.LEFT)
        {
            __time_units[i_square].changeSprite( ui_input_left );
        } else if ( dir == PlayerController.Direction.RIGHT)
        {
            __time_units[i_square].changeSprite( ui_input_right );
        } else //NONE
        {
            __time_units[i_square].changeSprite( ui_input_none );
        }
    }

}
