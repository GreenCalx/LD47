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
    private PlayerController playerController = null;
    private bool hasPlayerRef = false;

    public Sprite ui_input_up;
    public Sprite ui_input_left;
    public Sprite ui_input_right;
    public Sprite ui_input_down;
    public Sprite ui_input_none;
    private const string ENERGY_PANEL_PREFIX = "ENERGY";

    // Start is called before the first frame update
    void Start()
    {
        if ( playerRef != null)
        {
            playerController = playerRef.GetComponent<PlayerController>();
            hasPlayerRef = (playerController != null);
        }
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

    // Update is called once per frame
    void Update()
    {
    }

    public void updatePlayerRef( GameObject newRef)
    {
        if (newRef == null)
            return;
        playerRef = newRef;
        if ( playerRef != null)
        {
            playerController = playerRef.GetComponent<PlayerController>();
            hasPlayerRef = (playerController != null);
            
        }
    }

    public void refresh()
    {
        //updatePlayerRef(playerRef);
        if (!!hasPlayerRef)
        {
            Timeline tl = playerController.timeline;
            if (tl != null)
            {
                updateTimeUnits(tl);
                updateLooperState(playerController.L);
                updateInfoPanel(playerController.L);
            }
        }
        else { Debug.Log("UITimeline : Player ref is missing"); }
    }

    private void updateLooperState( Looper iLooper )
    {
        if ( __ui_looper_state == null )
            return;
        if ( iLooper.IsRecording )
            __ui_looper_state.setToRecording();
        else if ( iLooper.IsRunning )
            __ui_looper_state.setToReplay();
        else
            __ui_looper_state.setToEmpty();
    }

    private void updateInfoPanel(Looper iLooper)
    {
        if (__ui_inputs_panel == null)
            return;
        if (iLooper.IsRecording)
            __ui_inputs_panel.setToRecording();
        else if (iLooper.IsRunning)
            __ui_inputs_panel.setToReplay();
        else
            __ui_inputs_panel.setToEmpty();
    }

    private void updateTimeUnits(Timeline iTL)
    {
        // update time units sprites
        Transform current_tick_square_transform = null;
        int current_time_unit_index=0;
        for (int i=0; i<__time_units_squares.Length ;i++)
        {
            bool square_is_active = iTL.getAt(i);
            
            if (!square_is_active)
            {
                //__time_units_squares[i].changeSprite( disabled_time_unit );
                __time_units_squares[i].showDisabled();
            }
            else {
                if ( i > iTL.getTickForTimeUnits(Constraints.ShowNextInputsOnTimelineOnReplay && hasPlayerRef && playerController.IsLoopedControled))
                    __time_units_squares[i].showEnabled();
                    //__time_units_squares[i].changeSprite( available_time_unit );
                else if (playerController.L.Events.Count > i)
                    updateSquareInputImage( i, playerController.L.Events[i]);
            }

            // get current tick time unit square
            if ( i == iTL.getTickForCursor() )
            {
                if ( i < __time_units_squares.Length )
                {
                    current_tick_square_transform = __time_units_squares[i].gameObject.transform;
                    current_time_unit_index = i;
                    __time_units_squares[i].setSelect(true);
                    if(Constraints.ShowDefaultTileOnCursor && hasPlayerRef && !playerController.IsLoopedControled) {
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
