using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelector : MonoBehaviour
{
    [HideInInspector]
    public Stage selected_stage;
    
    private bool is_init = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void init( Stage iStartingStage )
    {
        selected_stage = iStartingStage;
        is_init = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_init)
            return;

        // TODO CONTROLLER CTRL
        var up      = Input.GetButtonDown("Up")      ;
        var down    = Input.GetButtonDown("Down")    ;
        var right   = Input.GetButtonDown("Right")   ;
        var left    = Input.GetButtonDown("Left")    ;
        var enter   = Input.GetButtonDown("Submit");
        if( up || down || right ||left )
        {
            POI neighbor = null;
            if (up)
                neighbor = selected_stage.tryNeighbor(POI.DIRECTIONS.UP);
            else if (down)
                neighbor = selected_stage.tryNeighbor(POI.DIRECTIONS.DOWN);
            else if (left)
                neighbor = selected_stage.tryNeighbor(POI.DIRECTIONS.LEFT);
            else if (right)
                neighbor = selected_stage.tryNeighbor(POI.DIRECTIONS.RIGHT);
            
            if ( neighbor!=null )
            {
                selected_stage = (Stage)neighbor;
                moveTo( neighbor.gameObject.transform );
            }
        }
        else if ( enter )
        {
            if (!!selected_stage)
                selected_stage.Load();
            else
                Debug.Log("FAILED TO LOAD LEVEL SCENE. NO SELECTED STAGE.");
        }

    }//! Update

    public void moveTo( Transform iDestination )
    {
        gameObject.transform.position = iDestination.position;
    }
}
