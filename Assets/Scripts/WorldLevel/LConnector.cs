using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LConnector : POI
{
    private readonly string prefix = "World";

    // 
    public int level_target;

    // Start is called before the first frame update
    void Start()
    {
        base.init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Load()
    {
        string scene_to_load = prefix + level_target;
        SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
    }
}
