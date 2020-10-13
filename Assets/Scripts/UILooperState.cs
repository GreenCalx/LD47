using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UILooperState : MonoBehaviour
{
    public Sprite record_img;
    public Sprite replay_img;
    private Image  __img;

    // Start is called before the first frame update
    void Start()
    {
        __img = GetComponent<Image>();
        __img.enabled = true;
    }

    public void setToRecording()
    {
        __img.sprite = record_img;
        __img.enabled = true;
    }
    public void setToReplay()
    {
        __img.sprite = replay_img;
        __img.enabled = true;
    }

    public void setToEmpty()
    {
        __img.sprite = null;
        __img.enabled = false;
    }

}
