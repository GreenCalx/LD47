using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Movable))]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Directions related variables
    /// </summary>
    public enum Direction { UP, DOWN, RIGHT, LEFT, NONE };
    //readonly string[]  DirectionInputs = { "Vertical",          "Vertical",          "Horizontal",        "Horizontal"               };
    readonly string[] DirectionInputs = { "Up", "Down", "Right", "Left" };
    readonly Vector2[] Directionf = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    
    /// <summary>
    /// State variables
    /// </summary>
    public Direction CurrentDirection = Direction.NONE;
    bool TickRequired = false;
    public bool IsLoopedControled = false;
    bool HasAlreadyBeenBreakedFrom = false;
    public EnergyCounter energyCounter;
    private GameObject levelUI_GOref;
    private GameObject levelUI_GO;
    public LevelUI levelUI;
    [SerializeField] private LayerMask wallmask;
    readonly float Speed = 1f;
    public bool WAIT_ORDER = false;

    public Looper L;

    List<GameObject> Tails = new List<GameObject>();

    /// <summary>
    /// References
    /// </summary>
    public GameObject GameLoop;
    public WorldManager WM;
    public GameObject TailPrefab;

    // Start is called before the first frame update
    public void Start()
    {
        if(GameLoop)
        {
            WM = GameLoop.GetComponent<WorldManager>();
        }
        if (energyCounter == null)
            this.energyCounter = new EnergyCounter( 5, 5);

        GetComponent<BoxCollider2D>().enabled = (false);

    }

    public void initUI( GameObject iUIGORef )
    {
        levelUI_GOref = iUIGORef;
        levelUI_GO = Instantiate(levelUI_GOref);
        levelUI = levelUI_GO.GetComponent<LevelUI>();
        levelUI.playerRef = this.gameObject;
        levelUI.refresh();
    }

    /// <summary>
    /// WorldManager will call this with its current tick as it is the 
    /// master of ticks, this way all player's loops are synchronized
    /// it will be used in FixedUpdate as it is fucking with physics possibily
    /// </summary>
    /// <param name="CurrentIdx"></param>
    public void RequireTick(int CurrentIdx)
    {
        TickRequired = true;
        L.CurrentIdx = CurrentIdx;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        var PC = collision.gameObject.GetComponent<PlayerController>();
        if (PC)
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }

    public void ApplyPhysics()
    {
        if (TickRequired)
        {
            // We update the direction from the loop if it is loop controlled
            if (L.IsRunning && IsLoopedControled)
            {
                CurrentDirection = L.Tick();
            }

            if (L.IsRecording)
            {
                L.Events.Add(CurrentDirection);
            }

            // Update position of the player
            // TODO: What about physics?? Do we rely on RigidBody?
            if (CurrentDirection != Direction.NONE)
            {
                Tails.Add(Instantiate(TailPrefab, this.gameObject.transform.position, Quaternion.identity));
                Tails[Tails.Count - 1].SetActive(true);
                Tails[Tails.Count - 1].transform.localScale = Tails[Tails.Count - 1].transform.localScale * 0.8f;
                var c = GetComponent<SpriteRenderer>().color;
                Tails[Tails.Count - 1].GetComponent<Tail>().SR.color = new Color(c.r, c.g, c.b, c.a * 0.8f);

                // move
                Movable movable = GetComponent<Movable>();
                if (!!movable)
                    movable.Move(Directionf[(int)CurrentDirection]);
                else
                    Debug.Log("Missing movable, abnormal pls look into it and add movable script onto player.");
            }

            // Reset position once we updated the player
            // This way we expect the position to be None if the player is not
            // touching any button during a tick
            CurrentDirection = PlayerController.Direction.NONE;

            foreach (var Tail in Tails)
            {
                if (Tail)
                    Tail.GetComponent<Tail>().Tick();
            }
        }
        TickRequired = false;
    }

    /// <summary>
    /// If WorldManager asked a Tick, then we update the player according to
    /// either the current loop doirection or the player asked direction
    /// </summary>
    void FixedUpdate()
    {
       // ApplyPhysic()
    }


    public float animationtime = 0.5f;
    public float maxLocalScale = 200;
    float currentAnimationTime = 0;
    void testAnimation()
    {
        currentAnimationTime += Time.deltaTime;

        if (currentAnimationTime < animationtime)
        {
            var s = Mathf.Max(100f, 100f + Mathf.Sin( (animationtime - currentAnimationTime) * 20) * (maxLocalScale - 100f));
            this.gameObject.transform.localScale = new Vector3(s, s, 1);
        } else
        {
            this.gameObject.transform.localScale = new Vector3(100, 100, 1);
        }
    }

    // Update is called once per frame
    // Everything related to in^puts is done here
    void Update()
    {

        if (currentAnimationTime < animationtime)
            testAnimation();
        else
        {
            GetComponent<BoxCollider2D>().enabled = (true);
            if (!IsLoopedControled)
            {
                if ( WAIT_ORDER  )
                {
                    CurrentDirection = Direction.NONE;
                    Debug.Log("WAIT ORDER.");
                    WM.NeedTick = true;
                } else {

                    var Up = Input.GetButtonDown(DirectionInputs[(int)Direction.UP]);
                    var Down = Input.GetButtonDown(DirectionInputs[(int)Direction.DOWN]);
                    var Right = Input.GetButtonDown(DirectionInputs[(int)Direction.RIGHT]);
                    var Left = Input.GetButtonDown(DirectionInputs[(int)Direction.LEFT]);
                    var None = Input.GetKeyDown(KeyCode.N);

                    if (Up) CurrentDirection = Direction.UP;
                    if (Down) CurrentDirection = Direction.DOWN;
                    if (Left) CurrentDirection = Direction.LEFT;
                    if (Right) CurrentDirection = Direction.RIGHT;
                    if (None) CurrentDirection = Direction.NONE;

                    if (Up || Down || Right || Left || None) 
                    { 
                        WM.NeedTick = true;
                    }
                }

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
                        // update energy loop to get nested counter
                        P.energyCounter = energyCounter.getNestedCounter();
                        if (!!levelUI)
                        {
                            P.levelUI = levelUI;
                            P.levelUI.updatePlayerRef(GO);
                            P.levelUI.refresh();
                        }

                        // Update newly created looper with current loop previous
                        // frames
                        P.L.Events = this.L.Events.GetRange(0, this.L.CurrentIdx + 1);
                        P.L.StartRecording();
                        // IMPORTANT : this nees to be done after StartRecording as it will take current 
                        // position as start position and we dont want that
                        P.L.StartPosition = this.L.StartPosition;
                    }

                    // For now new players are randomly colored
                    var SpriteRender = GO.GetComponent<SpriteRenderer>();
                    if (SpriteRender)
                    {
                        SpriteRender.color = UnityEngine.Random.ColorHSV();
                    }
                } else {
                    // not breaked, just replaying scenario
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

    //public int PM = 5;

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
                    // update energy loop to get nested counter
                    P.energyCounter = energyCounter.getNestedCounter();
                    //P.PM = PM - 1;
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
