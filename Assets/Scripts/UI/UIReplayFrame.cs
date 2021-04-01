using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIReplayFrame : MonoBehaviour
{

    private Image __image;

    // Start is called before the first frame update
    void Start()
    {
        __image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setImage( Sprite iSprite )
    {
        if(!!__image)
            __image.sprite = iSprite;
    }
}
