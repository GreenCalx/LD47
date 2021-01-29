using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StageBackground : MonoBehaviour
{
    public float max_alpha;
    public float min_alpha;

    private Image __image;

    private double __elapsed_time; 

    // Start is called before the first frame update
    void Start()
    {
        __image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        __elapsed_time += Time.deltaTime;
/* 
        var alpha =  max_alpha - Mathf.Sin(0);
        __image.color = new Color( __image.color.r, 
                                   __image.color.g,
                                   __image.color.b,
                                   alpha); */
    }
}
