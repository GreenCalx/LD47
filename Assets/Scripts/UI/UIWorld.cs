using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIWorld : MonoBehaviour
{

    private StageSelector __stage_selector_ref;
    private UIStageName __stage_name_frame;

    private UIReplayFrame __replayFrame;

    // TODO(mtn5): remove me
    private bool isFullscreen = false;
    private Vector2 Position;
    private Vector2 Size;

    // Start is called before the first frame update
    void Start()
    {
        __stage_selector_ref = FindObjectOfType<StageSelector>();

        __stage_name_frame = GetComponentInChildren<UIStageName>();

        __replayFrame = GetComponentInChildren<UIReplayFrame>();
        if (!!__replayFrame)
        {
            RectTransform RT = __replayFrame.GetComponent<RectTransform>();
            Size = RT.sizeDelta;
            Position = RT.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        refreshUI();

        //if (!__replayFrame) return;
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

     // This name is shitty...
     // It will actually switch to fullscreen or back to screened mode
     // and not only make it fullscreen...
    public void switchLevelToFullScreen()
    {
        // switch between full screen and small window for current level
        if (!isFullscreen)
        {
            RectTransform RT = __replayFrame.GetComponent<RectTransform>();
            RT.localPosition = Vector3.zero;
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 640);
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 480);
            // GameObject.Find("Level0").GetComponentInChildren<InputManager>().Activate();
            isFullscreen = true;
        }
        else
        {
            RectTransform RT = __replayFrame.GetComponent<RectTransform>();
            RT.localPosition = Position;
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Size.x);
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Size.y);
            // GameObject.Find("Level0").GetComponentInChildren<InputManager>().DeActivate();
            isFullscreen = false;
        }
    }

    public void switchWorldToFullScreen()
    {
        // switch between full screen and small window for current level
        if (!isFullscreen)
        {
            RectTransform RT = __replayFrame.GetComponent<RectTransform>();
            RT.localPosition = Vector3.zero;
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 640);
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 480);
            // GameObject.Find("Level0").GetComponentInChildren<InputManager>().Activate();
            isFullscreen = true;
        }
        else
        {
            RectTransform RT = __replayFrame.GetComponent<RectTransform>();
            RT.localPosition = Position;
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Size.x);
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Size.y);
            // GameObject.Find("Level0").GetComponentInChildren<InputManager>().DeActivate();
            isFullscreen = false;
        }
    }

}
