using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Stage[] stages;

    // Start is called before the first frame update
    void Start()
    {
        stages = GetComponentsInChildren<Stage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
