using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using UnityEngine;

public class Movable : MonoBehaviour
{
    [SerializeField] public LayerMask wallmask;
    [SerializeField] public LayerMask movablemask;
    public float Speed = 1f;

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

    public bool Move(PlayerController.Direction D, bool ApplyPhysicsBetweenPlayers = true)
    {
        if (Freeze) return false;
        bool NeedToBeMoved = false;
        if (D != PlayerController.Direction.NONE)
        {
            var Direction = PlayerController.Directionf[(int)D];
            Vector3 Dir3 = new Vector3(Direction.x, Direction.y, 0);
            // WALL HITS
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, wallmask);
            if (hits.Length != 0)
            {
                UpdatePositionBump(Direction, D);
                return false;
            }
            // MOVABLE HITS
            RaycastHit2D[] hitsm = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, movablemask);
            hitsm = hitsm.Where(val => (val.collider.gameObject != this.gameObject)).ToArray(); // filter our own gameobject

            if (!ApplyPhysicsBetweenPlayers)
                hitsm = hitsm.Where(val => (val.collider.gameObject.GetComponent<PlayerController>() == null)).ToArray(); // filter our own gameobject

            if (hitsm.Length == 0)
            {
                NeedToBeMoved = true;
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
                                if (PC && mePC && !(PC.IM.CurrentMode == InputManager.Mode.RECORD))
                                {
                                    if (Physics2D.GetIgnoreCollision(mePC.GetComponent<BoxCollider2D>(), PC.GetComponent<BoxCollider2D>()))
                                    {
                                        NeedToBeMoved = true;
                                    }
                                    else
                                    {
                                        NeedToBeMoved = Mov.Move(D);
                                    }
                                }
                                else
                                {
                                    NeedToBeMoved = Mov.Move(D);
                                }
                            }
                        }
                    }
                }
            }
            if (NeedToBeMoved) UpdatePosition(Direction, D);

            if (D != PlayerController.Direction.NONE && !NeedToBeMoved)
            {
                // Do a bump into wall animation
                UpdatePositionBump(Direction, D);
            }
        }
        if (!WM.Mdl.IsRewinding && ResetBetweenLoops &&!WM.Mdl.IsGoingBackward) WM.AddRewindMove(this.gameObject, D);
        return NeedToBeMoved;
    }

    void UpdatePositionBump(Vector2 Direction, PlayerController.Direction D)
    {
        LastPosition = this.gameObject.transform.position;
        NewPosition = LastPosition + new Vector2(Speed * Direction.x, Speed * Direction.y);

        AnimationTimer.Restart();
        TailsSpawm.Restart();
        BumpAnimation = true;
   }

    void UpdatePosition(Vector2 Direction, PlayerController.Direction D)
    {
        LastPosition = this.gameObject.transform.position;

        this.gameObject.transform.position += new Vector3(Speed * Direction.x,
                                            Speed * Direction.y,
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
    }

    public bool CanMove()
    {
        return !Freeze && (AnimationTimer.GetTime() == 0 || AnimationTimer.Ended());
    }

    void UpdateTimers()
    {
        AnimationTimer.Update(Time.deltaTime);
        TailsSpawm.Update(Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimers();
        if (!AnimationTimer.Ended())
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
            if (WM.Mdl.CurrentTick == 0 && ResetBetweenLoops && !WM.Mdl.IsRewinding)
            {
                this.gameObject.transform.position = StartPosition;
                SR.transform.localPosition = Vector2.zero;
            }
        }

    }
}
