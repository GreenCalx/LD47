using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivableObject : MonoBehaviour
{
    public SIGNAL_KEYS sig_key;
    public bool isTriggered     = false;
    
    // TODO : Required_PW should be equal to n_emitters in wire.
    public int required_PW;
    private int stored_pulses = 0;

    // Start is called before the first frame update
    public void Start()
    {
        isTriggered = false;
    }

    public void resetStorage()
    {
        this.stored_pulses = 0;
    }

    public virtual void trigger(bool signalType) {}

    public virtual bool listen(PulseToken iPT)
    {
        this.stored_pulses += iPT.PW;
        
        if ( this.stored_pulses >= required_PW )
        { 
            Debug.Log("trig : storage " + this.stored_pulses);
            activate(); return true; 
        }
        else
        { 
            deactivate(); 
        }
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
