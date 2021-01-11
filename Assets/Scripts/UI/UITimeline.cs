using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeline : MonoBehaviour
{
    public Sprite ui_input_up;
    public Sprite ui_input_left;
    public Sprite ui_input_right;
    public Sprite ui_input_down;
    public Sprite ui_input_none;

    private UITimeUnit[] __time_units;
    private Animator     __timeline_animator;

    private int current_time = 0;

    // Start is called before the first frame update
    void Start()
    {
        current_time = 0;

        __timeline_animator = GetComponentInChildren<Animator>();
        __time_units        = GetComponentsInChildren<UITimeUnit>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void update_time(int iNewTime)
    {
        current_time = iNewTime;
        if (!!__timeline_animator)
            __timeline_animator.SetInteger("time", current_time);
    }

}
