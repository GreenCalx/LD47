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
    
    // TODO : Required_PW should be equal to n_emitters in wire.
    public int required_PW;

    // Start is called before the first frame update
    public void Start()
    {
        isTriggered = false;
        required_PW = 1;
    }

    public virtual void trigger(bool signalType) {}

    public virtual bool listen(PulseToken iPT)
    {
        if ( iPT.PW >= required_PW )
        { activate(); return true; }
        else
        { deactivate(); }
        return false;
    }

    public void activate()
    {
        trigger(true);
    }

    public void deactivate()
    {
        trigger(false);
    }
}
