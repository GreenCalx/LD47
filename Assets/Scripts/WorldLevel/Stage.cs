using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // Start is called before the first frame update
    void Start()
    {
        base.init();
        id = stage_to_load;
        Debug.Log("init stage " + id);
        __completion_status = COMPLETION.LOCKED;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Load()
    {
        if (__completion_status == COMPLETION.LOCKED)
            return;
        string scene_to_load = LEVEL_NAME_PREFIX + level_to_load + STAGE_NAME_PREFIX + stage_to_load;
        SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
    }


}
