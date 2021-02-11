using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    public GameObject playerRef;
    public GameObject time_cursor_GO;
    public GameObject rewind_image_GO;

    private UITimeUnit[]    __time_units_squares;
    private UILooperState   __ui_looper_state;
    private UIInputsPanel   __ui_inputs_panel;

    public Sprite ui_input_up;
    public Sprite ui_input_left;
    public Sprite ui_input_right;
    public Sprite ui_input_down;
    public Sprite ui_input_none;
    private const string ENERGY_PANEL_PREFIX = "ENERGY";

    private WorldManager WM;
    // Start is called before the first frame update
    void Start()
    {
        __time_units_squares = GetComponentsInChildren<UITimeUnit>();

        // update time cursor pos

        if (!!time_cursor_GO && __time_units_squares.Length > 0)
        {
            Vector2 new_cursor_pos = __time_units_squares[0].transform.position;
            time_cursor_GO.transform.position = new_cursor_pos;
        }

        __ui_looper_state = GetComponentInChildren<UILooperState>();
        __ui_inputs_panel = GetComponentInChildren<UIInputsPanel>();
    }

    public void refresh(Timeline TL, InputManager.Mode Mode)
    {
        updateTimeUnits(TL, Mode);
        updateLooperState(Mode);
        updateInfoPanel(Mode);
    }

    private void updateLooperState( InputManager.Mode Mode )
    {
         if (Mode == InputManager.Mode.RECORD)
            __ui_looper_state.setToRecording();
        else if (Mode == InputManager.Mode.REPLAY)
            __ui_looper_state.setToReplay();
        else
            __ui_looper_state.setToEmpty();
    }

    private void updateInfoPanel( InputManager.Mode Mode )
    {
        if (Mode == InputManager.Mode.RECORD)
            __ui_inputs_panel.setToRecording();
        else if (Mode == InputManager.Mode.REPLAY)
            __ui_inputs_panel.setToReplay();
        else
            __ui_inputs_panel.setToEmpty();
    }

    public void setModel(WorldManager WM)
    {
        this.WM = WM;
    }

    private void updateTimeUnits(Timeline iTL, InputManager.Mode Mode)
    {
        // update time units sprites
        Transform current_tick_square_transform = null;
        int current_time_unit_index=0;
        if (null==__time_units_squares)
            return;

        for (int i=0; i<__time_units_squares.Length ;i++)
        {
            bool square_is_active = iTL.getAt(i);
            
            if (!square_is_active)
            {
                //__time_units_squares[i].changeSprite( disabled_time_unit );
                __time_units_squares[i].showDisabled();
            }
            else {
                if ( i > iTL.getTickForTimeUnits(Constants.ShowNextInputsOnTimelineOnReplay && Mode == InputManager.Mode.REPLAY))
                    __time_units_squares[i].showEnabled();
                else if (i < iTL.last_tick)
                    updateSquareInputImage( i, iTL.Events[i]);
            }

            // get current tick time unit square
            if ( i == iTL.getTickForCursor() )
            {
                if ( i < __time_units_squares.Length )
                {
                    current_tick_square_transform = __time_units_squares[i].gameObject.transform;
                    current_time_unit_index = i;
                    __time_units_squares[i].setSelect(true);
                    if(Constants.ShowDefaultTileOnCursor && Mode ==InputManager.Mode.RECORD) {
                        // case record show default
                        if(square_is_active)
                            __time_units_squares[i].changeSprite(ui_input_none);
                    }
                }
            }
            else if ( i < iTL.getTickForCursor() && (i ==__time_units_squares.Length-1) )
            {
                    // REWIND IS ON
                    if (!!rewind_image_GO)
                        current_tick_square_transform = rewind_image_GO.transform;
            } else {
                __time_units_squares[i].setSelect(false);
            }
        }

        // update time cursor pos
        if (!!time_cursor_GO && !!current_tick_square_transform)
        {
            Vector2 new_cursor_pos = current_tick_square_transform.transform.position;
            time_cursor_GO.transform.position = new_cursor_pos;
        }

        // update current tick time unit scale
        //current_tick_square_transform.localScale += new Vector3(.1f,.1f,0f);
        // rescale previous one if not 0
        //if ( current_time_unit_index > 0 )
        //    __time_units_squares[current_time_unit_index-1].gameObject.transform.localScale -= new Vector3(.1f,.1f,0f);

    }//! updateTimeUnits

    private void updateSquareInputImage( int i_square, PlayerController.Direction dir )
    {
        if (i_square > __time_units_squares.Length)
            return;
        if ( dir == PlayerController.Direction.UP)
        {
            __time_units_squares[i_square].changeSprite( ui_input_up );
        } else if ( dir == PlayerController.Direction.DOWN)
        {
            __time_units_squares[i_square].changeSprite( ui_input_down );
        } else if ( dir == PlayerController.Direction.LEFT)
        {
            __time_units_squares[i_square].changeSprite( ui_input_left );
        } else if ( dir == PlayerController.Direction.RIGHT)
        {
            __time_units_squares[i_square].changeSprite( ui_input_right );
        } else //NONE
        {
            __time_units_squares[i_square].changeSprite( ui_input_none );
        }
    }
    
}
