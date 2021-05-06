using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelector : MonoBehaviour, IControllable
{
    [HideInInspector]
    public Stage selected_stage;
    [HideInInspector]
    public POI selected_poi;
    [HideInInspector]
    public LConnector selected_lconn;

    public InputManager IM;
    public UIWorld UI;

    private POI neighbor;

    public int level_id;
    private bool is_init = false;

    // Start is called before the first frame update
    void Start()
    {
        IM.Attach(this as IControllable);
    }

    public void init(int iLevelID, Stage iStartingStage)
    {
        selected_stage = iStartingStage;
        selected_poi = iStartingStage;
        level_id = iLevelID;
        selected_lconn = null;
        
        selected_stage.Load();

        is_init = true;
    }

    public void init(int iLevelID, LConnector iStartingLevelConnector)
    {
        selected_lconn = iStartingLevelConnector;
        selected_poi = iStartingLevelConnector;
        selected_stage = null;
        level_id = iLevelID;
        is_init = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_init)
            return;

        if (neighbor != null)
        {
            if (neighbor is Stage)
            {
                Stage neighbor_stage = (Stage)neighbor;
                if (neighbor_stage.isUnlocked())
                {
                    selected_stage?.UnLoad();
                    selected_stage = neighbor_stage;
                    selected_poi = neighbor;
                    moveTo(neighbor.gameObject.transform);

                    if (!!selected_stage)
                    {
                        InterSceneCache.stage_from = selected_stage.id;
                        InterSceneCache.world_from = level_id;
                        selected_stage.Load();
                    }
                    else if (!!selected_poi && !!selected_lconn)
                    {
                        InterSceneCache.world_from = level_id;
                        InterSceneCache.stage_from = InterSceneCache.UNDEFINED;
                        selected_lconn.Load();
                    }
                    else
                        Debug.Log("FAILED TO LOAD LEVEL SCENE. NO SELECTED STAGE.");


                }
                else
                {
                    // play SFX or send feedback that stage is currently locked.
                    Debug.Log("Targeted neighbor stage is locked. Complete current stage before moving on.");
                }
            }
            else if (neighbor is LConnector)
            {
                // check selected stage is done
                if (selected_stage.isDone() || selected_stage.isStartingStage)
                {
                    selected_poi = neighbor;
                    selected_stage = null;
                    selected_lconn = (LConnector)neighbor;
                    moveTo(neighbor.gameObject.transform);
                }
            }
            neighbor = null;
        }
    }//! Update

    public void moveTo(Transform iDestination)
    {
        gameObject.transform.position = iDestination.position;
    }

    public void ProcessInputs(Save.InputSaver.InputSaverEntry Entry)
    {
        if (!is_init) return;

        if (IM.CurrentMode != InputManager.Mode.DEACTIVATED)
        {
            var up = Entry.Inputs["Up"].IsDown;
            var down = Entry.Inputs["Down"].IsDown;
            var right = Entry.Inputs["Right"].IsDown;
            var left = Entry.Inputs["Left"].IsDown;
            if (up || down || right || left)
            {
                if (up)
                    neighbor = selected_poi.tryNeighbor(POI.DIRECTIONS.UP);
                else if (down)
                    neighbor = selected_poi.tryNeighbor(POI.DIRECTIONS.DOWN);
                else if (left)
                    neighbor = selected_poi.tryNeighbor(POI.DIRECTIONS.LEFT);
                else if (right)
                    neighbor = selected_poi.tryNeighbor(POI.DIRECTIONS.RIGHT);


            }
        }
       
        var space = Entry.Inputs["Submit"].IsDown;
        if (space)
        {
            // Get UIWorld and make the scene full screen
            UI.switchLevelToFullScreen();

            if (IM.CurrentMode != InputManager.Mode.DEACTIVATED)
            {
                IM.CurrentMode = InputManager.Mode.DEACTIVATED;
                selected_stage.get_loaded_stage()?.GetComponentInChildren<InputManager>()?.Activate();
            }
            else
            {
                IM.CurrentMode = InputManager.Mode.PLAYER;
                selected_stage.get_loaded_stage()?.GetComponentInChildren<InputManager>()?.DeActivate();
            }
        }
    }
}
