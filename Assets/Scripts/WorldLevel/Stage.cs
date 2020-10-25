using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Stage : POI
{
    public enum COMPLETION {
        LOCKED,
        UNLOCKED,
        DONE
    }

    // CALL SCENES : LEVEL+X+STAGE+Y
    private const string STAGE_NAME_PREFIX = "STAGE";
    private const string LEVEL_NAME_PREFIX = "LEVEL";
    [SerializeField] public int level_to_load;
    [SerializeField] public int stage_to_load;

    [HideInInspector]
    public int id;
    private COMPLETION __completion_status;
    private SpriteRenderer __sr;

    // Start is called before the first frame update
    void Start()
    {
        base.init();
        id = stage_to_load;
        if ( id == 0 ) // Starting stage
            __completion_status = COMPLETION.UNLOCKED;
        else
            __completion_status = COMPLETION.LOCKED;

        __sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void refresh()
    {
        updateCompletionFromLevelProgress();

        // Update stage color based on completion
        if ( __completion_status == COMPLETION.UNLOCKED )
        {
            __sr.color = Color.white;
        } else if ( __completion_status == COMPLETION.DONE )
        {
            __sr.color = Color.green;
        } else {
            __sr.color = Color.grey;
        }
    }


    public void Load()
    {
        if (__completion_status == COMPLETION.LOCKED)
            return;
        string scene_to_load = LEVEL_NAME_PREFIX + level_to_load + STAGE_NAME_PREFIX + stage_to_load;
        //string scene_to_load = LEVEL_NAME_PREFIX + stage_to_load;
        SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
    }

    public void updateCompletion( Level.WORLD_POI iStagePOI )
    {
        if ( iStagePOI == Level.WORLD_POI.LOCKED_STAGE )
            __completion_status = COMPLETION.LOCKED;
        else if ( (iStagePOI==Level.WORLD_POI.START_STAGE) || (iStagePOI==Level.WORLD_POI.UNLOCKED_STAGE) )
            __completion_status = COMPLETION.UNLOCKED;
        else if (iStagePOI == Level.WORLD_POI.DONE_STAGE)
            __completion_status = COMPLETION.DONE;
        else // default
            __completion_status = COMPLETION.LOCKED; 
        
    }

    public void updateCompletionFromLevelProgress()
    {
        bool stage_done = LevelProgress.getCompletion( level_to_load, stage_to_load);
        if ( stage_done )
        {
            // unlock neighbors
            __completion_status = COMPLETION.DONE;
            unlock_neighbors();
        }
    }

    public bool isUnlocked()
    {
        return (__completion_status!=COMPLETION.LOCKED);
    }

    public void tryUnlock()
    {
        if ( __completion_status != COMPLETION.DONE)
            __completion_status = COMPLETION.UNLOCKED;
    }

    public void unlock_neighbors()
    {
        foreach (Tuple<POI,POI.DIRECTIONS> neighbor in neighbors)
        {
            Stage neighbor_stage = (Stage) neighbor.Item1;
            neighbor_stage.tryUnlock();
        }
    }
}
