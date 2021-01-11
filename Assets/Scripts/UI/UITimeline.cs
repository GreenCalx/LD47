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
    private UITimelineInput[]    __time_units;
    private UILooperState   __ui_looper_state;
    private Animator        __timeline_animator;

    /// Model
    private WorldManager __WM;

    private int current_time = 0;

    // Start is called before the first frame update
    void Start()
    {
        current_time = 0;

        __timeline_animator = GetComponentInChildren<Animator>();
        __time_units        = GetComponentsInChildren<UITimelineInput>();

        __ui_looper_state = GetComponentInChildren<UILooperState>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void refresh(Timeline TL, InputManager.Mode Mode)
    {
        updateUI(TL);

        if (null==__ui_looper_state)
            return;

        if (Mode == InputManager.Mode.RECORD)
        {
            __ui_looper_state.setToRecording();
        }
        else if (Mode == InputManager.Mode.REPLAY)
        {
            __ui_looper_state.setToReplay();
        }
        else
        {
            __ui_looper_state.setToEmpty();
        }
        //updateLooperState(playerController.L);
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
    private void updateUI(Timeline iTL)
    {
        // update time units sprites
        Transform current_tick_square_transform = null;
        int current_time_unit_index=0;

        if (null==__time_units)
        {
            if (!tryRefreshTimeUnits())
                return;
        }


        for (int i=0; i<__time_units.Length ;i++)
        {
            bool square_is_active = iTL.getAt(i);
            
            if (!square_is_active)
            {
                //__time_units[i].changeSprite( disabled_time_unit );
                __time_units[i].showDisabled();
            }
            else {
                if ( i > iTL.getTickForTimeUnits(Constraints.ShowNextInputsOnTimelineOnReplay && __WM.IM.CurrentMode == InputManager.Mode.REPLAY))
                    __time_units[i].showEnabled();
                else if (i < __WM.TL.last_tick)
                    updateSquareInputImage( i, __WM.TL.Events[i]);
            }

            // get current tick time unit square
            if ( i == iTL.getTickForCursor() )
            {
                if ( i < __time_units.Length )
                {
                    current_tick_square_transform = __time_units[i].gameObject.transform;
                    current_time_unit_index = i;
                    //__time_units[i].setSelect(true);
                    if(Constraints.ShowDefaultTileOnCursor && __WM.IM.CurrentMode==InputManager.Mode.RECORD) {
                        // case record show default
                        if(square_is_active)
                            __time_units[i].changeSprite(ui_input_none);
                    }
                }
            }
            else if ( i < iTL.getTickForCursor() && (i ==__time_units.Length-1) )
            {
                // REWIND IS ON
            } else {
               // __time_units[i].setSelect(false);
            }
        }

        // update timeline animator
        update_time(iTL.getTickForCursor());

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
