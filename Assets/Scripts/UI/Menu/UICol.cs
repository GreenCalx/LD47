using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UICol : MonoBehaviour
{
    public string UIMenuDico_key;

    public bool is_selected;

    // Start is called before the first frame update
    void Start()
    {
        is_selected = false;
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void refresh()
    {
        Text txt = GetComponent<Text>();
        if (is_selected)
            txt.color = UIColor.base_color;
        else 
            txt.color = UIColor.darken_color;
    }
    
    public void doAction()
    {
        UIMenuDico.callFunction(UIMenuDico_key);
    }



}
