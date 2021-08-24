using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public void Start()
    {
        isTriggered = false;
    }

    public virtual void trigger(bool signalType) {}

    public void activate()
    {
        trigger(true);
    }

    public void deactivate()
    {
        trigger(false);
    }
}
