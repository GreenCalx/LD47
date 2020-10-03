using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SIGNAL_KEYS
{
    NONE, BLUE, RED, YELLOW
}

public class ActivableObject : MonoBehaviour
{
    public SIGNAL_KEYS key;
    public bool isTriggered     = false;

    // Start is called before the first frame update
    void Start()
    {
        isTriggered = false;
    }

    public virtual void trigger() {}

    public void listen(SIGNAL_KEYS iSigKey) 
    {

        if ( key == SIGNAL_KEYS.NONE )
        {
            trigger();
        } else if (iSigKey == key)
        {
            trigger();
        }
        
    }
}
