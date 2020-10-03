using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{
    float Divider = 2f;
    public SpriteRenderer SR;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Tick()
    {
        SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, SR.color.a / Divider);
        this.gameObject.transform.localScale = this.gameObject.transform.localScale / Divider;
    }

    // Update is called once per frame
    void Update()
    {
        if (SR.color.a <= 0.1)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
