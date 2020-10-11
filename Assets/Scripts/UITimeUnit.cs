using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UITimeUnit : MonoBehaviour
{

    private Image __img;
    public Sprite im_enabled;
    public Sprite im_disabled;

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
}
