using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTile : ActivableObject
{
    private BoxCollider2D __collider2D;
    public AudioSource sound_on;
    public AudioSource sound_off;

    // Start is called before the first frame update
    new void Start()
    {
        __collider2D = GetComponent<BoxCollider2D>();
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!!__collider2D)
        {
            __collider2D.enabled = !isTriggered;
        }
    }//! update

    public override void trigger(bool signalType) // OPEN/CLOSE DOOR
    {
        bool previousState = isTriggered;
        isTriggered = signalType;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (!!sr)
            sr.enabled = !isTriggered;
        if (isTriggered)
            sound_on.Play();
        else if (previousState)
            sound_off.Play();
    }

}
