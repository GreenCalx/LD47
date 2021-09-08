using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorObject : TickBasedBehaviour 
{
    // OLD ATTR TO CLEAN UP
    public SIGNAL_KEYS signalKey;

    // NEW ATTR
    protected ConnectorGraph CG;
    public int pulse_speed = 2; // 99/(0..-inf) = Infinite ; 1 = 1 tile/tick ; n = n tiles / tick
    public bool is_active = false;

    // Only one time per tick
    public bool can_pulse;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        is_active = false;
        can_pulse = true;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool pulsate(bool iState) // 0 for no pulse , 1 for pulse
    {
#if false
        if (can_pulse && (CG!=null))
        {
            CG.pulsateFrom(this, iState);
            can_pulse = false;
            return true;
        }
        return false;
#endif
        if(CG != null)
        {
            CG.pulsateFrom(this, iState);
        }
        return true;
    }

    public void subscribeToGraph( ConnectorGraph iCG)
    {
        CG = iCG;
    }
}
