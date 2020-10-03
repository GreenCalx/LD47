using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTile : ActivatorObject
{

    void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach( ActivableObject ao in activableObjects )
            ao.listen(signalKey);   
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
