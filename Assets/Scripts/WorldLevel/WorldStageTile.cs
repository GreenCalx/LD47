using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStageTile : MonoBehaviour
{

    public Sprite stage_undone;
    public Sprite stage_done;

    private SpriteRenderer __sr;
    private Stage __stage;

    // Start is called before the first frame update
    void Start()
    {
        __sr = GetComponent<SpriteRenderer>();
        __stage = GetComponent<Stage>();
        if (!!__sr && __stage)
            __sr.sprite = LevelProgress.getCompletion( __stage.level_to_load, __stage.stage_to_load) ? stage_done : stage_undone;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
