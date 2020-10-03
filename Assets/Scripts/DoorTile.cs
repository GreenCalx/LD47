using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTile : ActivableObject
{
    private BoxCollider2D collider2D;
    // Start is called before the first frame update
    void Start()
    {
        collider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!!collider2D)
        {
            collider2D.enabled = !isTriggered;
        }
    }//! update
}
