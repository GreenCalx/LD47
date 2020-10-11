using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    public GameObject playerRef;
    public GameObject time_cursor_GO;
    public GameObject rewind_image_GO;

    //public GameObject[] time_units_squares;
    private UITimeUnit[] __time_units_squares;
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
    }

    // Update is called once per frame
    void Update()
    {
        //refresh();
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
            }
        }
        else { Debug.Log("UITimeline : Player ref is missing"); }
    }

    private void updateTimeUnits(Timeline iTL)
    {
        // update time units sprites
        Transform current_tick_square_transform = null;
        for (int i=0; i<__time_units_squares.Length ;i++)
        {
            bool square_is_active = iTL.getAt(i);
            
            if (!square_is_active)
            {
                //__time_units_squares[i].changeSprite( disabled_time_unit );
                __time_units_squares[i].showDisabled();
            }
            else {
                if ( i > iTL.last_tick )
                    __time_units_squares[i].showEnabled();
                    //__time_units_squares[i].changeSprite( available_time_unit );
                else if (playerController.L.Events.Count > i)
                    updateSquareInputImage( i, playerController.L.Events[i]);
            }

            // get current tick time unit square
            if (i == iTL.last_tick)
            {
                if ( i+1 < __time_units_squares.Length )
                {
                    current_tick_square_transform = __time_units_squares[i+1].gameObject.transform;
                }
                else
                {
                    // REWIND IS ON
                    if (!!rewind_image_GO)
                        current_tick_square_transform = rewind_image_GO.transform;
                }
            }
        }

        // update time cursor pos
        if (!!time_cursor_GO && !!current_tick_square_transform)
        {
            Vector2 new_cursor_pos = current_tick_square_transform.transform.position;
            time_cursor_GO.transform.position = new_cursor_pos;
        }

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
