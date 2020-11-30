using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitTile : MonoBehaviour
{
    private const string LEVEL_NAME_PREFIX = "LEVEL";
    private const string STAGE_NAME_PREFIX = "STAGE";

    private const string END_SCENE = "EndGame";

    private int __level_id;
    private int __stage_id;

    private const int MAX_LEVELS = 8;
    // Start is called before the first frame update
    void Start()
    {
    }

    void Awake()
    {
        getLevelAndStageID();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void getLevelAndStageID()
    {
        Scene scene = SceneManager.GetActiveScene();
        string scene_name = scene.name;
        string prev_letters  = "";
        string prev_digits   = "";

        foreach(char c in scene_name)
        {
            if (char.IsLetter(c))
            {
                if ( prev_digits.Length != 0 )
                {
                    if ( prev_letters == LEVEL_NAME_PREFIX )
                        __level_id = int.Parse(prev_digits);
                    else if (prev_letters == STAGE_NAME_PREFIX)
                        __stage_id = int.Parse(prev_digits);
                    prev_digits     = "";
                    prev_letters    = "";
                }
                prev_letters += c;
                continue;
            }
            else if (char.IsDigit(c))
            {
                prev_digits += c;
            }
        }
        // no whitespace at the end so we go out with the last digit.
        // thus we read it after the forloop
        //  ugly stuff can bve done better
        if ( (prev_letters.Length != 0) && (prev_digits.Length != 0 ) )
        {
            if (prev_letters == STAGE_NAME_PREFIX)
                __stage_id = int.Parse(prev_digits);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var PC = other.gameObject.GetComponent<PlayerController>();
        if ( PC)
        {
            exit();
        }
    }

    private void exit()
    {
        registerProgress();
        GetComponentInChildren<AudioSource>().Play();
        if (LevelProgress.gameIsFinished())
        {
            SceneManager.LoadScene( END_SCENE, LoadSceneMode.Single);
        }
        //string scene_to_load = LEVEL_NAME_PREFIX + ( current_level_exit_index + 1 ) ;
        string world_to_load = "World"+__level_id;

        var S = GameObject.Find("Saver");
        if (S)
        {
            var Save = S.GetComponent<Save>();
            if(Save)
            {
                Save.EndLevel(); 
            }
        }

        SceneManager.LoadScene( world_to_load, LoadSceneMode.Single);
    }

    private void registerProgress()
    {
        //LevelProgress.setProgress( current_level_exit_index, true);
        LevelProgress.completeStage( __level_id, __stage_id);
        Debug.Log(" COMPLETED " + __level_id  + " / " + __stage_id );
    }
}
