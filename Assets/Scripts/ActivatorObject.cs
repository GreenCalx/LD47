using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorObject : MonoBehaviour
{
    public GameObject[] activables;
    public SIGNAL_KEYS signalKey;
    protected List<ActivableObject> activableObjects;

    // Start is called before the first frame update
    void Start()
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
}
