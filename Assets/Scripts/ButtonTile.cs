using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTile : ActivatorObject
{
    //public GameObject activable;
    //public SIGNAL_KEYS signalKey;
    //private ActivableObject activableObject;
    public Sprite sprite_on;
    public Sprite sprite_off;
    public AudioSource sound_on;
    public AudioSource sound_off;

    private void OnTriggerEnter2D(Collider2D other)
    {
        /*
        foreach( ActivableObject ao in activableObjects )
            ao.listen(signalKey, this, true);
        */

        // register object to get called on tick
        // TODO toffa: make this work in case it is another object than player that trigger
        GameObject.Find("GameLoop").GetComponent<WorldManager>().AddListener(other.gameObject.GetComponent<PlayerController>() as ITickObserver, this);

        if (pulsate(true))
            sound_on.Play();

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (!!sr)
            sr.sprite = sprite_on;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        GameObject.Find("GameLoop").GetComponent<WorldManager>().AddListener(other.gameObject.GetComponent<PlayerController>() as ITickObserver, this);

        // button doesn't change
        if (pulsate(true))
            sound_on.Play();

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // we just don't send a pulse now
        if (pulsate(false))
            sound_off.Play();

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (!!sr)
            sr.sprite = sprite_off;
    }
}
