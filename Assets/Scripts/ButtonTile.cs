using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTile : ActivatorObject
{
    //public GameObject activable;
    //public SIGNAL_KEYS signalKey;
    //private ActivableObject activableObject;
    public AudioSource sound_on;
    public AudioSource sound_off;

    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach( ActivableObject ao in activableObjects )
            ao.listen(signalKey, this);
        sound_on.Play();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // button doesn't change
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        foreach( ActivableObject ao in activableObjects )
            ao.listen(signalKey, this);
        sound_off.Play();
    }
}
