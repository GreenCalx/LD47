using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    public GameObject playerRef;

    public Sprite available_time_unit;
    public Sprite disabled_time_unit;
    public GameObject time_cursor_GO;

    //public GameObject[] time_units_squares;
    private UITimeUnit[] __time_units_squares;
    private PlayerController playerController = null;
    private bool hasPlayerRef = false;

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
                //ui_replenish_lbl.text   = "" + (ec.getReplenish()-1); // has max cell
                //updateEnergyPanels( ec.getEnergy(), ec.getDisabledEnergy() );
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
                __time_units_squares[i].changeSprite( disabled_time_unit );
            else
                __time_units_squares[i].changeSprite( available_time_unit );

            if (i == iTL.last_tick)
                current_tick_square_transform = __time_units_squares[i].gameObject.transform;
        }
        Debug.Log("current tick at ui " + iTL.last_tick);
        // update time cursor pos
        if (!!time_cursor_GO && !!current_tick_square_transform)
        {
            Vector2 new_cursor_pos = current_tick_square_transform.transform.position;
            time_cursor_GO.transform.position = new_cursor_pos;
        }

    }//! updateTimeUnits
    
}
