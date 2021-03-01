using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimelineSwitcher : MonoBehaviour
{

    private Image __self_img;

    /// ORDER IS STRICT
    // 0 : Timeline-0
    // 1 : Timeline-1
    // 2 : Timeline-2
    // 3 : Timeline-3
    // 4 : Timeline-4
    public Sprite[] __sr_states;


    // Start is called before the first frame update
    void Start()
    {
        __self_img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void switchTimeline( int iLoopLevel)
    {
        if (!__self_img)
            return;

        if ( (iLoopLevel>=0) && ( iLoopLevel<__sr_states.Length) )
        {
            __self_img.sprite = __sr_states[iLoopLevel];
        } else 
        {
            Debug.LogError(" No available Sprite for UITimelineswitcher level " + iLoopLevel + ". Size of given sprite array is " + __sr_states.Length );
        }
    }
}
