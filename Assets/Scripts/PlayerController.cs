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
    static public readonly string[] DirectionInputs = { "Up", "Down", "Right", "Left" };
    static public readonly Vector2[] Directionf = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    
    /// <summary>
    /// State variables
    /// </summary>
    public Direction CurrentDirection = Direction.NONE;
    bool TickRequired = false;
    public bool IsLoopedControled = false;
    bool HasAlreadyBeenBreakedFrom = false;
    //public EnergyCounter energyCounter;
    public Timeline timeline;
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
        //if (energyCounter == null)
        //   this.energyCounter = new EnergyCounter( 5, 5);
        
        this.timeline = new Timeline(0);

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

    public void SpawnTail()
    {
        Tails.Add(Instantiate(TailPrefab, GetComponentInChildren<SpriteRenderer>().transform.position, Quaternion.identity));
        Tails[Tails.Count - 1].SetActive(true);
        //Tails[Tails.Count - 1].transform.localScale = Tails[Tails.Count - 1].transform.localScale * 0.8f;
        var c = GetComponentInChildren<SpriteRenderer>().color;
        Tails[Tails.Count - 1].GetComponent<Tail>().SR.color = new Color(c.r, c.g, c.b, c.a * 0.8f);
    }

    public void ApplyPhysics(bool Reverse)
    {
        if (TickRequired)
        {
            // We update the direction from the loop if it is loop controlled
            if (L.IsRunning && IsLoopedControled)
            {
                CurrentDirection = L.Tick();
            }

            if (!Reverse && L.IsRecording)
            {
                L.Events.Add(CurrentDirection);
            }

            if(Reverse && CurrentDirection != Direction.NONE)
            {
                if (CurrentDirection == Direction.UP) CurrentDirection = Direction.DOWN;
                else if (CurrentDirection == Direction.DOWN) CurrentDirection = Direction.UP;
                else if (CurrentDirection == Direction.RIGHT) CurrentDirection = Direction.LEFT;
                else if (CurrentDirection == Direction.LEFT) CurrentDirection = Direction.RIGHT;
            } 

            // Update position of the player
            // TODO: What about physics?? Do we rely on RigidBody?
            if (CurrentDirection != Direction.NONE)
            {
                    // move
                    Movable movable = GetComponent<Movable>();
                    if (!!movable)
                        movable.Move(CurrentDirection);
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
                    //WM.NeedTick = true;
                    WAIT_ORDER = false;
                } else {
                    var Up = Input.GetButtonDown(DirectionInputs[(int)Direction.UP]);
                    var Down = Input.GetButtonDown(DirectionInputs[(int)Direction.DOWN]);
                    var Right = Input.GetButtonDown(DirectionInputs[(int)Direction.RIGHT]);
                    var Left = Input.GetButtonDown(DirectionInputs[(int)Direction.LEFT]);
                    //var None = Input.GetKeyDown(KeyCode.N);

                    if (Up) CurrentDirection = Direction.UP;
                    if (Down) CurrentDirection = Direction.DOWN;
                    if (Left) CurrentDirection = Direction.LEFT;
                    if (Right) CurrentDirection = Direction.RIGHT;
                    //if (None) CurrentDirection = Direction.NONE;

                    if (Up || Down || Right || Left) 
                    { 
                        WM.NeedTick = true;
                    }
                }

            }
            else
            {
                if (Input.GetButtonDown("Break") && !HasAlreadyBeenBreakedFrom)
                {
                    // break from the loop
                    HasAlreadyBeenBreakedFrom = true;
                    // create a new player at current position
                    var GO = WM.AddPlayer(this.gameObject.transform.position);
                    var P = GO.GetComponent<PlayerController>();
                    if (P)
                    {
                        // update energy loop to get nested counter
                        //P.energyCounter = energyCounter.getNestedCounter();
                        P.timeline = timeline.getNestedTimeline();
                        if (!!levelUI)
                        {
                            P.levelUI = levelUI;
                            P.levelUI.updatePlayerRef(GO);
                            P.levelUI.refresh();
                        }

                        // Update newly created looper with current loop previous
                        // frames
                        if (this.L.CurrentIdx != 0)
                        {
                            P.L.Events = this.L.Events.GetRange(0, this.L.CurrentIdx + 1);

                            // TODO : copy into world recording for rewind

                        }
                        P.L.StartRecording();
                        // IMPORTANT : this nees to be done after StartRecording as it will take current 
                        // position as start position and we dont want that
                        P.L.StartPosition = this.L.StartPosition;
                        P.GetComponent<Movable>().StartPosition = P.L.StartPosition;
                    }

                    // For now new players are randomly colored
                    var SpriteRender = GO.GetComponentInChildren<SpriteRenderer>();
                    if (SpriteRender)
                    {
                        var c = this.gameObject.GetComponentInChildren<SpriteRenderer>().color * 0.7f;
                        SpriteRender.color = new Color(c.r,c.g,c.b,1);
                    }
                } else {
                    // not breaked, just replaying scenario
                }
            }
        }
    }
}
