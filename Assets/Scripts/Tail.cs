using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using UnityEngine;

public class Tail : MonoBehaviour
{
    float Divider = 2f;
    public SpriteRenderer SR;
    public bool IsTickBased = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Tick()
    {
        if (IsTickBased)
        {
            SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, SR.color.a / Divider);
            this.gameObject.transform.localScale = this.gameObject.transform.localScale / Divider;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsTickBased)
        {
            SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, SR.color.a - (Time.deltaTime * 1.5f));
            //this.gameObject.transform.localScale = this.gameObject.transform.localScale - new Vector3(transform.localScale.x * (Time.deltaTime * 1.5f),
            //                                                                                         transform.localScale.y *(Time.deltaTime * 1.5f), 0);
        }


        if (SR.color.a <= 0.1)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
