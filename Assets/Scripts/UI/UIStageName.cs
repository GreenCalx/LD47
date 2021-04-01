using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStageName : MonoBehaviour
{
    private Text __textField;

    // Start is called before the first frame update
    void Start()
    {
         __textField = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setName( int stageID, string stageName)
    {
        if (!!__textField)
        {
            string name = " STAGE ";
            name +=  stageID;
            name += " - ";
            name += stageName;
            __textField.text = name;
        }
    }
}
