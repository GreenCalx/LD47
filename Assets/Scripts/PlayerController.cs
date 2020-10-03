using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public class Looper
    {
        public PlayerController PC;
        
        public Vector2 StartPosition;
        public List<PlayerController.Direction> Events = new List<PlayerController.Direction>();
        public bool IsPaused = false;
        public bool IsRunning = false;
        public bool IsRecording = true;

        public int CurrentIdx;

        public PlayerController.Direction Tick()
        {
            if (CurrentIdx >= Events.Count) return PlayerController.Direction.NONE;
            else return Events[CurrentIdx];            
        }

        public void Reset()
        {
            PC.gameObject.transform.position = new Vector3(StartPosition.x, StartPosition.y, 0);
        }

        public void Start()
        {
            Reset();
        }

        public void Stop()
        {
            Reset();
        }
        public void StartRecord()
        {
            StartPosition = PC.gameObject.transform.position;
            IsRecording = true;
            IsPaused = false;
            IsRunning = false;
        }

        public void EndRecord()
        {
            IsRecording = false;
        }
    }
    public Looper L = new Looper();

    public enum Direction { UP, DOWN, RIGHT, LEFT, NONE };
    //readonly string[]  DirectionInputs = { "Vertical",          "Vertical",          "Horizontal",        "Horizontal"               };
    readonly string[] DirectionInputs = { "Up", "Down", "Right", "Left" };
    readonly Vector2[] Directionf = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    public Direction CurrentDirection = Direction.NONE;

    bool TickRequired = false;
    public bool IsLoopedControled = false;
    bool HasAlreadyBeenBreakedFrom = false;
    public EnergyCounter energyCounter;

    float Speed = 1f;

    public GameObject GameLoop;
    WorldManager WM;
    // Start is called before the first frame update
    void Start()
    {
        if(GameLoop)
        {
            WM = GameLoop.GetComponent<WorldManager>();
        }
 
        L.PC = this;
        this.energyCounter = new EnergyCounter();
    }

    public void RequireTick(int CurrentIdx)
    {
        TickRequired = true;
        L.CurrentIdx = CurrentIdx;
    }

    void FixedUpdate()
    {
        if (TickRequired)
        {
            if (IsLoopedControled && L.IsRunning)
            {
                CurrentDirection = L.Tick();
            }

            if (CurrentDirection != Direction.NONE)
                this.gameObject.transform.position += new Vector3(Speed * Directionf[(int)CurrentDirection].x,
                                                                  Speed * Directionf[(int)CurrentDirection].y,
                                                                  0);
            if (L.IsRecording)
            {
                L.Events.Add(CurrentDirection);
            }

            CurrentDirection = PlayerController.Direction.NONE;
        }
        TickRequired = false;
    }

    // Update is called once per frame
    void Update()
    {
        var Start = Input.GetKeyDown(KeyCode.A);
        var Stop = Input.GetKeyDown(KeyCode.Z);
        var Pause = Input.GetKeyDown(KeyCode.E);
        var Restart = Input.GetKeyDown(KeyCode.R);
        var Record = Input.GetKeyDown(KeyCode.T);
        var StopRecord = Input.GetKeyDown(KeyCode.Y);

        if (L.IsRunning)
        {

            if (Stop)
            {
                IsLoopedControled = false;
                L.IsRunning = false;
            }
            if (Pause) L.IsPaused = true;
        }
        else
        {
            if (Start)
            {
                L.IsRunning = true;
                IsLoopedControled = true;
            }
        }

        if (L.IsRecording)
        {
            if (StopRecord) L.IsRecording = false;
        }
        else
        {
            if (Record)
            {
                L.IsRecording = true;
                L.StartPosition = this.gameObject.transform.position;
            }
        }


        if (!IsLoopedControled)
        {
            //var Up    = Input.GetAxisRaw(DirectionInputs[(int)Direction.UP]);
            //var Right = Input.GetAxisRaw(DirectionInputs[(int)Direction.RIGHT]);
            var Up = Input.GetButtonDown(DirectionInputs[(int)Direction.UP]);
            var Down = Input.GetButtonDown(DirectionInputs[(int)Direction.DOWN]);
            var Right = Input.GetButtonDown(DirectionInputs[(int)Direction.RIGHT]);
            var Left = Input.GetButtonDown(DirectionInputs[(int)Direction.LEFT]);

            if (Up) CurrentDirection = Direction.UP;
            if (Down) CurrentDirection = Direction.DOWN;
            if (Left) CurrentDirection = Direction.LEFT;
            if (Right) CurrentDirection = Direction.RIGHT;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.B) && !HasAlreadyBeenBreakedFrom)
            {
                // break from the loop
                HasAlreadyBeenBreakedFrom = true;
                // create a new player at current position
                var GO = WM.AddPlayer(this.gameObject.transform.position);
                var P = GO.GetComponent<PlayerController>();
                if (P)
                {
                    P.L.Events = this.L.Events.GetRange(0, this.L.CurrentIdx+1);
                    P.L.IsRecording = true;
                    P.L.StartPosition = this.L.StartPosition;
                }

                var SpriteRender = GO.GetComponent<SpriteRenderer>();
                if (SpriteRender)
                {
                    SpriteRender.color = UnityEngine.Random.ColorHSV();
                }
            }
        }
    }

#if false
    class Looper
    {
        public StartPosition;
        List<PlayerController.Direction> Events;
    }
    public Looper L;
    

    
    public float Speed = 1f;
    public float TileSize = 1f;

    public int PM = 5;

    public bool IsLoopedControled = false;
    bool HasAlreadyBeenBreakedFrom = false;

    bool InputUsed = true;

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
        InputUsed = true;

        if(L)
        {
            L.Events.Add(CurrentDirection);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLoopedControled)
        {
            //var Up    = Input.GetAxisRaw(DirectionInputs[(int)Direction.UP]);
            //var Right = Input.GetAxisRaw(DirectionInputs[(int)Direction.RIGHT]);
            var Up    = Input.GetButtonDown(DirectionInputs[(int)Direction.UP]  );
            var Down  = Input.GetButtonDown(DirectionInputs[(int)Direction.DOWN]);
            var Right = Input.GetButtonDown(DirectionInputs[(int)Direction.RIGHT]);
            var Left  = Input.GetButtonDown(DirectionInputs[(int)Direction.LEFT]);

            // TODO: Make it so that the player cannot press all inputs and doing shit
            // for now just need to be carefull to press only one button per frame
            if (InputUsed)
            {
                CurrentDirection = Direction.NONE;

                if (Up) CurrentDirection = Direction.UP;
                if (Down) CurrentDirection = Direction.DOWN;
                if (Left) CurrentDirection = Direction.LEFT;
                if (Right) CurrentDirection = Direction.RIGHT;

                if (Up || Down || Left || Right) InputUsed = false;
            }
            /*if (Mathf.Abs(Right) >= Mathf.Abs(Up))
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
            }*/
        } else
        {
            if(Input.GetKeyDown(KeyCode.B) && !HasAlreadyBeenBreakedFrom)
            {
                // break from the loop
                HasAlreadyBeenBreakedFrom = true;
                // create a new player at current position
                var GO = Instantiate(PlayerPrefab, this.gameObject.transform.position, Quaternion.identity);
                var P = GO.GetComponent<PlayerController>();
                if (P)
                {
                    P.PlayerPrefab = PlayerPrefab;
                    P.PM = PM - 1;
                }

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
#endif
    }
