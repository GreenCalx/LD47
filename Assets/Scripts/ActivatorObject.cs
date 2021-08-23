using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorObject : TickBased 
{
    // OLD ATTR TO CLEAN UP
    public GameObject[] activables;
    public SIGNAL_KEYS signalKey;
    protected List<ActivableObject> activableObjects;

    // NEW ATTR
    protected ConnectorGraph CG;
    public int pulse_speed = 2; // 99/(0..-inf) = Infinite ; 1 = 1 tile/tick ; n = n tiles / tick
    public bool is_active = false;

/*    
    public override void OnTick()
    {
        CG.update_wires();
    }
*/

    // Start is called before the first frame update
    protected virtual void Start()
    {
        activableObjects = new List<ActivableObject>();
        foreach( GameObject go in activables )
        {
            ActivableObject activableObject = go.GetComponent<ActivableObject>(); 
            if (!!activableObject)
            {
                activableObjects.Add(activableObject);
            }
        }

        is_active = false;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pulsate(bool iState) // 0 for no pulse , 1 for pulse
    {
        CG.pulsateFrom(this, iState);
    }

    public void subscribeToGraph( ConnectorGraph iCG)
    {
        CG = iCG;
    }
}
