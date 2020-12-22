using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.AccessControl;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.Serialization;

[RequireComponent(typeof(Movable))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IControllable , ISavable {
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
   [System.Serializable]
    public class Model : IModel
    {
        public Direction CurrentDirection = Direction.NONE;
        [NonSerialized] private LayerMask wallmask;
        public Timeline TL;
    }
    IModel ISavable.GetModel()
    {
        return Mdl;
    }
    public Model Mdl = new Model();

    public InputManager IM;
    public WorldManager WM;
    List<GameObject> Tails = new List<GameObject>();

    /// <summary>
    /// References
    /// </summary>
    public GameObject TailPrefab;

    public void Awake()
    {
        if (!TailPrefab) Debug.Log("[PlayerController] No TailPrefab reference in prefab");
        Tails = new List<GameObject>();

        Mdl = new Model();
    }

    public void Start()
    {
        StartAnimation();
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
    public void ApplyPhysics(bool ReverseDirection)
    {
        // We update the direction from the loop if it is loop controlled
        if (IM.CurrentMode == InputManager.Mode.REPLAY)
            Mdl.CurrentDirection = Mdl.TL.GetCurrent();
        if (IM.CurrentMode == InputManager.Mode.RECORD)
        {
            if (!Mdl.TL.getAt(Mdl.TL.last_tick))
                Mdl.CurrentDirection = Direction.NONE;
            Mdl.TL.SetCurrent(Mdl.CurrentDirection);
        }
        // move
        Movable Mover = GetComponent<Movable>();
        if (ReverseDirection) Mdl.CurrentDirection = InverseDirection(Mdl.CurrentDirection);
        if (Mover) Mover.Move(Mdl.CurrentDirection);
        // Reset position once we updated the player
        // This way we expect the position to be None if the player is not
        // touching any button during a tick
        Mdl.CurrentDirection = PlayerController.Direction.NONE;
        foreach (var Tail in Tails)
        {
            if (Tail) Tail.GetComponent<Tail>().Tick();
        }
    }

    // TODO refacto: do a real animation manager (also a real animation...)
    public float animationtime = 0.5f;
    public float maxLocalScale = 2; //2 times bigger
    float currentAnimationTime = 0;
    float startingScale = 0;
    void StartAnimation()
    {
        // needed due to the animation beiong played at startup
        // we dont want it to collide yet
        // it will be enabled again once the animation is done
        GetComponent<BoxCollider2D>().enabled = false;
        currentAnimationTime = 0;
        startingScale = transform.localScale.x;
    }
    void EndAnimation()
    {
        this.gameObject.transform.localScale = new Vector3(startingScale, startingScale, 1);
        GetComponent<BoxCollider2D>().enabled = true;
    }
    bool Animate()
    {
        currentAnimationTime += Time.deltaTime;
        if (currentAnimationTime < animationtime)
        {
            var s = Mathf.Max(startingScale, startingScale + Mathf.Sin((animationtime - currentAnimationTime) * 20) * startingScale * maxLocalScale);
            this.gameObject.transform.localScale = new Vector3(s, s, 1);
            return true;
        }
        else
        {
            EndAnimation();
            return false;
        }
    }

    void IControllable.ProcessInputs(Save.InputSaver.InputSaverEntry Entry)
    {
        if (Constraints.InputMode == 0)
        {
            var Up = Entry.Inputs[DirectionInputs[(int)Direction.UP]].IsDown || Entry.isDpadUpPressed;
            var Down = Entry.Inputs[DirectionInputs[(int)Direction.DOWN]].IsDown || Entry.isDpadDownPressed;
            var Right = Entry.Inputs[DirectionInputs[(int)Direction.RIGHT]].IsDown || Entry.isDpadRightPressed;
            var Left = Entry.Inputs[DirectionInputs[(int)Direction.LEFT]].IsDown || Entry.isDpadLeftPressed;

            if (Up) Mdl.CurrentDirection = Direction.UP;
            if (Down) Mdl.CurrentDirection = Direction.DOWN;
            if (Left) Mdl.CurrentDirection = Direction.LEFT;
            if (Right) Mdl.CurrentDirection = Direction.RIGHT;
        }
    }

    void Update()
    {
        if (Animate())
        {
            var Mover = gameObject.GetComponent<Movable>();
            if (Mover) Mover.Freeze = true;
        }
        else
        {
            var Mover = gameObject.GetComponent<Movable>();
            if (Mover) Mover.Freeze = false;
        }
    }
}
