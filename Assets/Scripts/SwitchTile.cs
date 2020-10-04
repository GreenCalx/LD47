using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTile : ActivatorObject
{

    public bool isSwitched = false;
    public Sprite spriteSwitched;

    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach( ActivableObject ao in activableObjects )
            ao.listen(signalKey, this);

        if (!isSwitched)
        {
            isSwitched = true;
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (!!sr)
                sr.sprite = spriteSwitched;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // switch doesn't change
    }

    private void OnTriggerExit2D(Collider2D other)
    { 
        // switch doesn't change
    }

}
