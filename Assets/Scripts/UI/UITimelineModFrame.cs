using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimelineModFrame : MonoBehaviour
{

    public Color record_color;
    public Color rewind_color;
    public Color play_color;

    public float alpha_damp;

    private Image[] __children_img;
    private Image   __self_img;

    private bool backward_tick;
    private bool forward_tick;

    // Start is called before the first frame update
    void Start()
    {
        __self_img      = GetComponent<Image>();
        __children_img  = GetComponentsInChildren<Image>();

        backward_tick = false;
        forward_tick = false;
    }

    private void trySetColor( Color iColor )
    {
        Color dampened_color = iColor;
        dampened_color.a -= alpha_damp;

        if (__self_img.color == dampened_color )
            return;

        if (!!__self_img)
            __self_img.color = dampened_color;
        foreach( Image im in __children_img ) { im.color = dampened_color; }
    }

    public void setToRecord()
    {
        trySetColor(record_color);
    }

    public void setToReplay()
    {
        trySetColor(play_color);
    }

    public void setToRewind()
    {
        trySetColor(rewind_color);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
