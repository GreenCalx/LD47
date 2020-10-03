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

    public virtual void trigger() {}

    public void listen(SIGNAL_KEYS iSigKey, ActivatorObject activator) 
    {
        currentActivatorsObject.Add(activator);

        activatorsObject.Sort();
        currentActivatorsObject.Sort();
        var Result = activatorsObject.Except(currentActivatorsObject);
        if (!Result.Any())
        {

            if (key == SIGNAL_KEYS.NONE)
            {
                trigger();
            }
            else if (iSigKey == key)
            {
                trigger();
            }

        }
        
    }
}
