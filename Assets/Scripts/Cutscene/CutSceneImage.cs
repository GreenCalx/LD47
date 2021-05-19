using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneImage : CutSceneElem
{
    public Sprite image;

    public CutSceneImage( Sprite iSprite, double iTimer) : base(iTimer)
    {
        image = iSprite;
    }
}
