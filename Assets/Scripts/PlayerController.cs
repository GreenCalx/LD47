using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Movable))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Directions related variables
    /// </summary>
    public enum Direction { UP, DOWN, RIGHT, LEFT, NONE };
    static public readonly string[] DirectionInputs = { "Up", "Down", "Right", "Left" };
    static public readonly Vector2[] Directionf = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };

    static public Direction InverseDirection(Direction D)
    {
        if (D == PlayerController.Direction.UP) D = PlayerController.Direction.DOWN;
        else if (D == PlayerController.Direction.DOWN) D = PlayerController.Direction.UP;
        else if (D == PlayerController.Direction.RIGHT) D = PlayerController.Direction.LEFT;
        else if (D == PlayerController.Direction.LEFT) D = PlayerController.Direction.RIGHT;

        return D;
    }

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
    public bool has_active_ui = false;
    [SerializeField] private LayerMask wallmask;
    readonly float Speed = 1f;
    public bool WAIT_ORDER = false;

    public Looper L;

    List<GameObject> Tails = new List<GameObject>();

    /// <summary>
    /// References
    /// </summary>
    public WorldManager WM;
    public GameObject TailPrefab;

    // Start is called before the first frame update
    public void Start()
    {
        if (!WM) Debug.Log("[PlayerController] No WorldManager reference in prefab");
        if (!TailPrefab) Debug.Log("[PlayerController] No TailPrefab reference in prefab");

        // can it ever be !null at start?
        if (timeline == null) timeline = new Timeline(0);

        // needed due to the animation beiong played at startup
        // we dont want it to collide yet
        // it will be enabled again once the ani;ation is done
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void initUI(GameObject iUIGORef)
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

    // TODO refacto: dont think it is needed?
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

    /// <summary>
    /// This will apply physics to the object
    /// It will always be called from WorldManager FixedUpdate
    /// Therefore it should be treated as a FixedUpdate function
    /// </summary>
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
                // move
                Movable Mover = GetComponent<Movable>();
                if (Mover) Mover.Move(CurrentDirection);
            }

            // Reset position once we updated the player
            // This way we expect the position to be None if the player is not
            // touching any button during a tick
            CurrentDirection = PlayerController.Direction.NONE;

            foreach (var Tail in Tails)
            {
                if (Tail) Tail.GetComponent<Tail>().Tick();
            }
        }
        TickRequired = false;
    }

    // TODO refacto: do a real animation manager (also a real animation...)
    public float animationtime = 0.5f;
    public float maxLocalScale = 200;
    float currentAnimationTime = 0;
    void testAnimation()
    {
        currentAnimationTime += Time.deltaTime;

        if (currentAnimationTime < animationtime)
        {
            var s = Mathf.Max(100f, 100f + Mathf.Sin((animationtime - currentAnimationTime) * 20) * (maxLocalScale - 100f));
            this.gameObject.transform.localScale = new Vector3(s, s, 1);
        }
        else
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
            GetComponent<BoxCollider2D>().enabled = true;
            // INPUTS RELATED
            if (!IsLoopedControled)
            {
                if (WAIT_ORDER)
                {
                    CurrentDirection = Direction.NONE;
                    WAIT_ORDER = false;
                }
                else
                {
                    var Up = Input.GetButtonDown(DirectionInputs[(int)Direction.UP]);
                    var Down = Input.GetButtonDown(DirectionInputs[(int)Direction.DOWN]);
                    var Right = Input.GetButtonDown(DirectionInputs[(int)Direction.RIGHT]);
                    var Left = Input.GetButtonDown(DirectionInputs[(int)Direction.LEFT]);

                    if (Up) CurrentDirection = Direction.UP;
                    if (Down) CurrentDirection = Direction.DOWN;
                    if (Left) CurrentDirection = Direction.LEFT;
                    if (Right) CurrentDirection = Direction.RIGHT;

                    if (Up || Down || Right || Left)
                    {
                        WM.NeedTick = true;
                    }
                }

            }
            else
            {
                bool needBreak = false;
                var Up = Input.GetButtonDown(DirectionInputs[(int)Direction.UP]);
                var Down = Input.GetButtonDown(DirectionInputs[(int)Direction.DOWN]);
                var Right = Input.GetButtonDown(DirectionInputs[(int)Direction.RIGHT]);
                var Left = Input.GetButtonDown(DirectionInputs[(int)Direction.LEFT]);

                needBreak = (Up || Down || Right || Left || Input.GetButtonDown("Break"));


                if (needBreak && !HasAlreadyBeenBreakedFrom && !WM.IsRewinding)
                {
                    // break from the loop
                    HasAlreadyBeenBreakedFrom = true;
                    // create a new player at current position
                    var GO = WM.AddPlayer(this.gameObject.transform.position);
                    Direction Dir = Direction.NONE;
                    if (Up) Dir = Direction.UP;
                    if (Down) Dir = Direction.DOWN;
                    if (Left) Dir = Direction.LEFT;
                    if (Right) Dir = Direction.RIGHT;

                    GO.GetComponent<PlayerController>().CurrentDirection = Dir;

                    WM.NeedTick = true;
                }
            }
        }
        if (has_active_ui)
            levelUI.refresh();
    }
}
