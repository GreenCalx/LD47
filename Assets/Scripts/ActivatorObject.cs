using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorObject : MonoBehaviour
{
    // OLD ATTR TO CLEAN UP
    public GameObject[] activables;
    public SIGNAL_KEYS signalKey;
    protected List<ActivableObject> activableObjects;

    // NEW ATTr
    protected ConnectorGraph CG;
    public int pulse_speed = 2; // 99 = Infinite ; 1 = 1 tile/tick ; n = n tiles / tick

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
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void sendPulse()
    {
        CG.pulsateFrom(this);
    }

    public void subscribeToGraph( ConnectorGraph iCG)
    {
        CG = iCG;
    }
}
