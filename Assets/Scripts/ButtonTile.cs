using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTile : MonoBehaviour
{
    public GameObject activable;
    private ActivableObject activableObject;

    void Start()
    {
        activableObject = activable.GetComponent<ActivableObject>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        activableObject.isTriggered = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        activableObject.isTriggered = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        activableObject.isTriggered = false;
    }
}
