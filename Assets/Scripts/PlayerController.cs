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

    /// NOTE (MTN5) : we need a default constructor to be able to save this object
    public PlayerController()
    {
        this.Mdl = new Model();
    }


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

    [System.Serializable]
    public class Model : IModel
    {
        public Direction CurrentDirection = Direction.NONE;
        public bool FacingRight = true;
        [NonSerialized] private LayerMask wallmask;
        [NonSerialized] public Timeline TL;
    }
    IModel ISavable.GetModel()
    {
        return Mdl;
    }
    public Model Mdl;

    public InputManager IM;

    [Serializable]
    public class TailSpawner
    {
        public GameObject TailPrefab;
        private List<GameObject> Tails = new List<GameObject>();

        public void SpawnTail(Vector3 Position, Color C)
        {
            Tails.Add(Instantiate(TailPrefab, Position, Quaternion.identity));
            Tails[Tails.Count - 1].SetActive(true);
            Tails[Tails.Count - 1].GetComponent<Tail>().SR.color = new Color(C.r, C.g, C.b, C.a * 0.8f);
        }

        public void Tick()
        {
            foreach (var Tail in Tails)
            {
                if (Tail) Tail.GetComponent<Tail>().Tick();
            }
        }
    }
    public TailSpawner Tails;

    public void Awake()
    {

        if (Tails != null && Tails.TailPrefab == null)
            Debug.Log("Missing Tail prefab");
    }

    public void Start()
    {
        StartAnimation();
    }

    /// <summary>
    /// This will apply physics to the object
    /// It will always be called from WorldManager FixedUpdate
    /// Therefore it should be treated as a FixedUpdate function
    /// </summary>
    public Movable.MoveResult ApplyPhysics(bool ReverseDirection = false)
    {
        Movable.MoveResult Result = Movable.MoveResult.CannotMove;
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
        if (Mover)
        {
           Result = Mover.Move(Mdl.CurrentDirection);
           checkSpriteFlip();
        }
        // Reset position once we updated the player
        // This way we expect the position to be None if the player is not
        // touching any button during a tick
        if(Result != Movable.MoveResult.IsAnimating)
            Mdl.CurrentDirection = PlayerController.Direction.NONE;
        Tails.Tick();
        return Result;
    }

    // TODO refacto: do a real animation manager (also a real animation...)
    public Timer animationtime;
    public float maxLocalScale = 2; //2 times bigger
    float startingScale = 0;
    void StartAnimation()
    {
        // needed due to the animation beiong played at startup
        // we dont want it to collide yet
        // it will be enabled again once the animation is done
        GetComponent<BoxCollider2D>().enabled = false;
        startingScale = transform.localScale.x;
        animationtime.SetEndTime(1f);
        animationtime.Restart();
    }
    void EndAnimation()
    {
        this.gameObject.transform.localScale = new Vector3(startingScale, startingScale, 1);
        GetComponent<BoxCollider2D>().enabled = true;
    }
    bool Animate()
    {
        animationtime.Update(Time.deltaTime);
        if (!animationtime.Ended())
        {
            var s = Mathf.Max(startingScale, startingScale + Mathf.Sin((animationtime.Length() - animationtime.GetTime()) * 20) * startingScale * maxLocalScale);
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
        if (Constants.InputMode == 0)
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

    void checkSpriteFlip()
    {
        bool flip_detected = ( ( Mdl.TL.GetCurrent() == Direction.RIGHT ) && !Mdl.FacingRight );
        flip_detected |= ( ( Mdl.TL.GetCurrent() == Direction.LEFT ) && Mdl.FacingRight );

        if (!flip_detected)
            return;

        SpriteRenderer sprite = gameObject.GetComponentInChildren<SpriteRenderer>();
        sprite.flipX    = !sprite.flipX;
        Mdl.FacingRight = !Mdl.FacingRight;
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
