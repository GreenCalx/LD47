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
    public GameObject[] activators;
    public List<ActivatorObject> activatorsObject = new List<ActivatorObject>();
    private List<ActivatorObject> currentActivatorsObject = new List<ActivatorObject>();

    public SIGNAL_KEYS key;
    public bool isTriggered     = false;

    // Start is called before the first frame update
    public void Start()
    {
        isTriggered = false;

        foreach (var go in activators)
        {
            activatorsObject.Add(go.GetComponent<ActivatorObject>());
        }
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

    public void listen(SIGNAL_KEYS iSigKey, ActivatorObject activator, bool signalType) 
    {
        if (signalType)
        {
            currentActivatorsObject.Add(activator);

            bool allActivated = true;
            foreach (ActivatorObject ao in activatorsObject)
            {
                if (!currentActivatorsObject.Contains(ao))
                {
                    allActivated = false;
                    break;
                }
            }
            if (allActivated)
            {

                if (key == SIGNAL_KEYS.NONE)
                {
                    trigger(signalType);
                }
                else if (iSigKey == key)
                {
                    trigger(signalType);
                }

            }
        }
        else
        {
            currentActivatorsObject.Remove(activator);
            trigger(signalType);
        }
        
    }
}
