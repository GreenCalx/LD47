using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIWorld : MonoBehaviour
{

    private StageSelector __stage_selector_ref;
    private UIStageName __stage_name_frame;

    private UIReplayFrame __replayFrame;

    // Start is called before the first frame update
    void Start()
    {
        __stage_selector_ref = FindObjectOfType<StageSelector>();
        if (!!__stage_selector_ref)
            Debug.Log("STAGE SELECTOR REF OK");

        __stage_name_frame = GetComponentInChildren<UIStageName>();
        if (!!__stage_name_frame)
            Debug.Log("STAGE NAME FRAME REF OK");

        __replayFrame = GetComponentInChildren<UIReplayFrame>();
        if (!!__replayFrame)
            Debug.Log("REPLAY FRAME REF OK");
    }

    // Update is called once per frame
    void Update()
    {
        refreshUI();
        //updateReplayFrame();
    }

    public void updateReplayFrame( Texture iSprite )
    {
        __replayFrame.setImage(iSprite);
    }

    private void refreshUI()
    {
        if ( !!__stage_selector_ref && !!__stage_name_frame )
        {
            // refresh stage name
            Stage selected_stage = __stage_selector_ref.selected_stage;
            if ( selected_stage == null )
                return;
            int stage_id = selected_stage.id;
            int level_id = __stage_selector_ref.level_id;
            string stage_name = StageCatalog.getStageName( level_id, stage_id);
            __stage_name_frame.setName( stage_id, stage_name);
        }   
    }

}
