using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitTile : MonoBehaviour
{
    private const string LEVEL_NAME_PREFIX = "LEVEL";
    private const string END_SCENE = "EndGame";

    private const int MAX_LEVELS = 8;
    public int current_level_exit_index;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var PC = other.gameObject.GetComponent<PlayerController>();
        if ( PC)
        {
            GetComponentInChildren<AudioSource>().Play();
            LevelProgress.setProgress( current_level_exit_index, true);

            if (current_level_exit_index >= MAX_LEVELS)
            {
                SceneManager.LoadScene( END_SCENE, LoadSceneMode.Single);
            }
            string scene_to_load = LEVEL_NAME_PREFIX + ( current_level_exit_index + 1 ) ;
            SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
        }
    }
}
