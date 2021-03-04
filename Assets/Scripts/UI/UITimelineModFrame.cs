using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimelineModFrame : MonoBehaviour
{

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

    public void setToRecord()
    {
    }

    public void setToReplay()
    {
    }

    public void setToRewind()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
