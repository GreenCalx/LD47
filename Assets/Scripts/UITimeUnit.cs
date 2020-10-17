using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UITimeUnit : MonoBehaviour
{

    private Image __img;
    private bool is_selected;
    public Sprite im_enabled;
    public Sprite im_disabled;

    // Start is called before the first frame update
    void Start()
    {
        __img = GetComponent<Image>();
        is_selected = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showDisabled()
    {
        __img.sprite = im_disabled;
    }

    public void showEnabled()
    {
        __img.sprite = im_enabled;
    }

    public void changeSprite( Sprite iNewSprite )
    {
        __img.sprite = iNewSprite;
    }

    private void upscale(Vector3 iScaleOffset)
    {
        gameObject.transform.localScale += iScaleOffset;
    }

    public void setSelect( bool iIsSelected )
    {
        if ( iIsSelected != is_selected )
        {
            if (iIsSelected) {
                upscale(new Vector3(10f,10f,0f));
            } else {
                upscale(new Vector3(-10f,-10f,0f));
            }
            is_selected = iIsSelected;
        }
    }
}
