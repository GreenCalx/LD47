using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIReplayFrame : MonoBehaviour
{

    private RawImage __image;

    // Start is called before the first frame update
    void Start()
    {
        __image = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setImage( Texture iSprite )
    {
        if(!!__image)
            __image.texture = iSprite;
    }
}
