using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TalkBubble : MonoBehaviour
{
    private SpriteRenderer __sr;

    public Sprite talk_bubble_sprite;
    public Sprite is_talking_sprite;

    // Start is called before the first frame update
    void Start()
    {
        __sr = GetComponent<SpriteRenderer>();
        display(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void display( bool iState )
    {
        __sr.enabled = iState;
    }

    public void setIsTalking( bool iState )
    {
        __sr.sprite = (iState) ? talk_bubble_sprite : is_talking_sprite;
    } 
}
