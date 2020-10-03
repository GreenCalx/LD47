using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTile : MonoBehaviour
{
    public GameObject activable;
    
    public SIGNAL_KEYS signalKey;
    private ActivableObject activableObject;

    void Start()
    {
        activableObject = activable.GetComponent<ActivableObject>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        activableObject.listen(signalKey);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // button doesn't change
    }

    private void OnTriggerExit2D(Collider2D other)
    { 
    }

}
