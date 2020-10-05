using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReseter : MonoBehaviour
{
        public string scene_to_load;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ( Input.GetKeyDown(KeyCode.R) )
        {
            SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
        }
        if ( Input.GetKeyDown(KeyCode.Escape) )
        {
            SceneManager.LoadScene( "LevelSelectorScene", LoadSceneMode.Single);
        }
    }
}
