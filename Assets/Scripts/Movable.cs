using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using UnityEngine;

public class MoveValue : TransientTickValue
{
    public MoveValue(Movable m, PlayerController.Direction d) {
        _go = m;
        _dir = d;
    }
    public Movable _go;
    public PlayerController.Direction _dir = PlayerController.Direction.NONE;

    public override void OnFixedTick() {
        _go?.UpdatePosition(PlayerController.Directionf[(int)_dir]);
    }
    public override void OnFixedBackTick() { _go?.UpdatePosition(PlayerController.Directionf[(int)(PlayerController.InverseDirection(_dir))]); }
}

public class PlayerMoveValue : FixedTickValue
{
    WorldManager WM;
    public void FindWM()
    {
        WM=GameObject.Find("GameLoop").GetComponent<WorldManager>();
    }

    public PlayerMoveValue(Movable m, PlayerController.Direction d)
    {
        _go = m;
        _dir = d;
    }
    Movable _go;
    PlayerController.Direction _dir = PlayerController.Direction.NONE;

    public override void OnFixedTick() {
        if (!WM) FindWM();
        if (WM.TL.Mode == InputManager.Mode.REPLAY)
        {
            base.OnFixedTick();
        }
        if (WM.TL.Mode == InputManager.Mode.RECORD)
        {
            _go?.Move(_dir, true, true, this);
        }
    }
}

public class TransientTickValue : TickBased
{
}

public class FixedTickValue : TickBasedAndClock
{
}

public class CollisionManagerValue : FixedTickValue
{
    PlayerController PC;
    WorldManager WM;
    public CollisionManagerValue(WorldManager WM, PlayerController PC) { this.PC = PC; this.WM = WM; }
    public override void OnFixedTick()
    {
        foreach(var P in WM.Mdl.Players)
        {
            if (P != PC)
            {
                Physics2D.IgnoreCollision(P.GetComponent<BoxCollider2D>(), PC.GetComponent<BoxCollider2D>(), false);
            }
            else break;
        }
    }

    public override void OnFixedBackTick()
    {
        foreach(var P in WM.Mdl.Players)
        {
            if (P != PC)
            {
                Physics2D.IgnoreCollision(P.GetComponent<BoxCollider2D>(), PC.GetComponent<BoxCollider2D>(), true);
            }
            else break; 
        }
    }
}

public class BreakValue : FixedTickValue
{
}

public class Movable : MonoBehaviour
{
    static public float AnimationTime = 1.0f;
    static public float TailsMultiplier = 3.0f; // want to spawn X tails during animation

    [SerializeField] public LayerMask wallmask;
    [SerializeField] public LayerMask movablemask;

    public bool ResetBetweenLoops = true;

    public Vector2 StartPosition;
    public Vector2 LastPosition;
    public Vector2 NewPosition;

    public bool IsSpawningTails = false;

    public WorldManager WM;

    public SpriteRenderer SR;

    public Timer AnimationTimer;
    public Timer TailsSpawm;
    public bool Freeze = false;
    public bool BumpAnimation = false;

    public enum MoveResult { CanMove, CannotMove, IsAnimating }
    public MoveResult Move(PlayerController.Direction D, bool ApplyPhysicsBetweenPlayers = true, bool RecordEvent = true, FixedTickValue MoveValue = null)
    {
        if (Freeze) return MoveResult.CannotMove;
        //if (!CanMove()) return MoveResult.IsAnimating; 

        MoveResult NeedToBeMoved = MoveResult.CannotMove;

        if (D != PlayerController.Direction.NONE)
        {
            var Direction = PlayerController.Directionf[(int)D];
            Vector3 Dir3 = new Vector3(Direction.x, Direction.y, 0);
            // WALL HITS
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, wallmask);
            if (hits.Length != 0)
            {
                UpdatePositionBump(Direction);
                return MoveResult.CannotMove;
            }
            // MOVABLE HITS
            RaycastHit2D[] hitsm = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, movablemask);
            hitsm = hitsm.Where(val => (val.collider.gameObject != this.gameObject)).ToArray(); // filter our own gameobject

            if (!ApplyPhysicsBetweenPlayers)
                hitsm = hitsm.Where(val => (val.collider.gameObject.GetComponent<PlayerController>() == null)).ToArray(); // filter our own gameobject

            if (hitsm.Length == 0)
            {
                NeedToBeMoved = MoveResult.CanMove;
            }
            else
            {
                foreach (RaycastHit2D hit in hitsm)
                {
                    if (hit.collider.gameObject != this.gameObject) // not necessary should already be filtered
                    {
                        if (hit.collider != null)
                        {
                            // Detect if we are colliding with a player
                            // If the player asked for the same direction than us last event
                            // We dont execute the move and simply move this object because physic
                            // is executed in a certain order
                            var PC = hit.collider.GetComponent<PlayerController>();
                            var Mov = hit.collider.GetComponent<Movable>();
                            var mePC = GetComponent<PlayerController>();
                            if (Mov)
                            {
                                if (PC && mePC)
                                {
                                    if (Physics2D.GetIgnoreCollision(mePC.GetComponent<BoxCollider2D>(), PC.GetComponent<BoxCollider2D>()))
                                    {
                                        NeedToBeMoved = MoveResult.CanMove;
                                    }
                                    else
                                    {
                                        NeedToBeMoved = Mov.Move(D, true, true, MoveValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (NeedToBeMoved == MoveResult.CanMove) {
                UpdatePosition(Direction);
                if (RecordEvent && MoveValue != null) MoveValue.AddObserver(new MoveValue(this, D));
            }

            if (D != PlayerController.Direction.NONE && NeedToBeMoved == MoveResult.CannotMove)
            {
                // Do a bump into wall animation
                UpdatePositionBump(Direction);
            }
        }
        return NeedToBeMoved;
    }

    void UpdatePositionBump(Vector2 Direction)
    {
        LastPosition = this.gameObject.transform.position;
        NewPosition = LastPosition + new Vector2(Direction.x,Direction.y);

        AnimationTimer.Restart();
        TailsSpawm.Restart();
        BumpAnimation = true;
   }

    public void UpdatePosition(Vector2 Direction)
    {
        LastPosition = this.gameObject.transform.position;

        this.gameObject.transform.position += new Vector3(Direction.x,
                                           Direction.y,
                                            0);

        NewPosition = this.gameObject.transform.position;
        AnimationTimer.Restart();
        TailsSpawm.Restart();
    }

    // Start is called before the first frame update
    void Awake()
    {
        StartPosition = this.gameObject.transform.position;
        LastPosition = StartPosition;
        NewPosition = StartPosition;

        AnimationTimer = new Timer(AnimationTime);
    }

    public bool CanMove()
    {
        return !Freeze && !AnimationTimer.IsRunning();
    }

    void UpdateTimers()
    {
        AnimationTimer.Update(Time.deltaTime);
        TailsSpawm.Update(Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        AnimationTimer.SetEndTime(AnimationTime);
        TailsSpawm.SetEndTime(AnimationTime / TailsMultiplier);

        UpdateTimers();
        if (AnimationTimer.IsRunning())
        {
            if (BumpAnimation)
            {
                if (SR) SR.transform.position = Vector2.Lerp(LastPosition,
                                                             NewPosition,
                                                             Mathf.Sin( (AnimationTimer.GetTime() / AnimationTimer.Length())*Mathf.PI) * 0.2f);
            }
            else
            {
                if (IsSpawningTails)
                {
                    if (TailsSpawm.Ended())
                    {
                        // Spawn tail
                        // TODO : should not be in playercointreoller
                        var PC = GetComponent<PlayerController>();
                        if (PC)
                        {
                            PC.Tails.SpawnTail(PC.GetComponentInChildren<SpriteRenderer>().transform.position, PC.GetComponentInChildren<SpriteRenderer>().color);
                        }
                        TailsSpawm.Restart();
                    }
                }
                if (SR) SR.transform.position = Vector2.Lerp(LastPosition, NewPosition, AnimationTimer.GetTime() / AnimationTimer.Length());
            }
        }
        else
        {
            BumpAnimation = false;
            SR.transform.localPosition = Vector2.zero;
        }
    }
}
