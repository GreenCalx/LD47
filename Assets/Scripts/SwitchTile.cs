using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTile : ActivatorObject
{

    public bool isSwitched = false;
    public Sprite spriteSwitched;
    private List<GameObject> objectsOnSwitch = new List<GameObject>();
    public Sprite spriteNotSwitched;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isSwitched)
        {
            objectsOnSwitch.Add(other.gameObject);

            foreach( ActivableObject ao in activableObjects )
                ao.listen(signalKey, this, true);

            GetComponentInChildren<AudioSource>().Play();
            isSwitched = true;
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (!!sr)
                sr.sprite = spriteSwitched;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        objectsOnSwitch.Remove(other.gameObject);

        // switch is reseted when going out of it
        // and nothing is on it anymore
        if (isSwitched && objectsOnSwitch.Count == 0)
        {
            foreach( ActivableObject ao in activableObjects )
                ao.listen(signalKey, this, false);

            isSwitched = false;

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (!!sr)
                sr.sprite = spriteNotSwitched;
        }
    }

}
