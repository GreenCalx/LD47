using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Require not strong enough to prevent __img nullptr to occur when reset a scene
[RequireComponent(typeof(Image))]
public class UITimelineInput : MonoBehaviour
{

    private Image __img;
    public  Sprite __disabled;
    // private cache to re-enable a disabled squre
    // never occures?
    private Sprite __cache; // TODO : Weird cache to do mb its not good at all for memory check me

    private Color __base_color;
    public Color  __disabled_color;

    // Start is called before the first frame update
    void Start()
    {
        __img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void showDisabled()
    {
        if (!!__img)
        {
            __img.sprite = __disabled;
            __img.color = __disabled_color;
        }
    }

    public void showEnabled()
    {
        if (!!__img)
        {
            if (__img.sprite == __disabled)
                __img.sprite = __cache;
        }
    }

    public void changeSprite( Sprite iNewSprite )
    {
        if (!!__img)
        {
            __img.sprite = iNewSprite;
            __cache = iNewSprite;
        }
    }

}
