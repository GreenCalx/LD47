using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTile : ActivableObject
{
    private BoxCollider2D __collider2D;

    // Start is called before the first frame update
    void Start()
    {
        __collider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!!__collider2D)
        {
            __collider2D.enabled = !isTriggered;
        }
    }//! update

    public override void trigger() // OPEN/CLOSE DOOR
    {
        isTriggered =! isTriggered;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (!!sr)
            sr.enabled = !isTriggered;
    }

}
