using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneText : CutSceneElem
{
    public string text;
    public CutSceneText( string iText, double iTimer ) : base(iTimer)
    {
        text = iText; 
    }
}
