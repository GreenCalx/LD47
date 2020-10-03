using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public enum        Direction         { UP,                  DOWN,                RIGHT,               LEFT,                 NONE };
    readonly string[]  DirectionInputs = { "Vertical",          "Vertical",          "Horizontal",        "Horizontal"               };
    readonly Vector2[] Directionf =      { new Vector2( 0, 1 ), new Vector2(0, -1 ), new Vector2( 1, 0 ), new Vector2( -1, 0 )       };
    public Direction          CurrentDirection = Direction.NONE;
    
    public float Speed = 0.5f;

    public bool IsLoopedControled = false;
    bool HasAlreadyBeenBreakedFrom = false;

    public GameObject PlayerPrefab; // needed to create new players when breaking from the loop

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if(CurrentDirection != Direction.NONE)
            this.gameObject.transform.position += new Vector3(Speed * Directionf[(int)CurrentDirection].x, 
                                                              Speed * Directionf[(int)CurrentDirection].y, 
                                                              0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLoopedControled)
        {
            var Up = Input.GetAxis(DirectionInputs[(int)Direction.UP]);
            var Right = Input.GetAxis(DirectionInputs[(int)Direction.RIGHT]);

            CurrentDirection = Direction.NONE;
            if (Mathf.Abs(Right) >= Mathf.Abs(Up))
            {
                if (Right > 0)
                    CurrentDirection = Direction.RIGHT;
                if (Right < 0)
                    CurrentDirection = Direction.LEFT;
            }
            else
            {
                if (Up > 0)
                    CurrentDirection = Direction.UP;
                if (Up < 0)
                    CurrentDirection = Direction.DOWN;
            }
        } else
        {
            if(Input.GetKeyDown(KeyCode.B) && !HasAlreadyBeenBreakedFrom)
            {
                // break from the loop
                HasAlreadyBeenBreakedFrom = true;
                // create a new player at current position
                var GO = Instantiate(PlayerPrefab, this.gameObject.transform.position, Quaternion.identity);
                GO.GetComponent<PlayerController>().PlayerPrefab = PlayerPrefab;
                var SpriteRender = GO.GetComponent<SpriteRenderer>();
                if(SpriteRender)
                {
                    SpriteRender.color = UnityEngine.Random.ColorHSV();
                }

                var Looper = GO.GetComponent<Loop>();
                if(Looper)
                {
                    Looper.IsRecording = true;
                    Looper.StartPosition = GO.transform.position;
                }
            }
        }
    }

    public void SetPosition( Vector2 Position )
    {
        this.gameObject.transform.position = new Vector3(Position.x, Position.y, 0);
    }
}
