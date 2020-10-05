﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using UnityEngine;

public class Movable : MonoBehaviour
{
    [SerializeField] private LayerMask wallmask;
    [SerializeField] private LayerMask movablemask;
    public float Speed = 1f;

    public bool ResetBetweenLoops = true;

    public Vector2 StartPosition;

    public Vector2 LastPosition;
    public Vector2 NewPosition;

    public bool IsMoving = false;

    public WorldManager WM;

    public SpriteRenderer SR;

    public float AnimationTime = 1;
    public float CurrentTime = 1;
    public bool EndAnimation = true;

    public float SpawnTailTime = 0.1f;
    public float CurrentSpawnTailTime = 0.1f;

    public bool Move(PlayerController.Direction D)
    {
        var Direction = PlayerController.Directionf[(int)D];

        Vector3 Dir3 = new Vector3(Direction.x, Direction.y, 0);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, wallmask);
        if (hits.Length != 0) return false;

        if (!WM.IsRewinding)
        {
            RaycastHit2D[] hitsm = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, movablemask);
            hitsm = hitsm.Where(val => (val.collider.gameObject != this.gameObject)).ToArray();
            if (hitsm.Length == 0)
            {
                UpdatePosition(Direction, D);
                if(ResetBetweenLoops) WM.AddRewindMove(this.gameObject, D);
                return true;
            }

            foreach (RaycastHit2D hit in hitsm)
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    if (hit.collider != null)
                    {
                        // Detect if we are colliding with a player
                        // If the player asked for the same direction than us last event
                        // We dont execute the move and simply move this object because physic
                        // is executed in a certain order
                        var PC = hit.collider.GetComponent<PlayerController>();
                        var mePC = GetComponent<PlayerController>();

                        var Mov = hit.collider.GetComponent<Movable>();
                        if (Mov)
                        {
                            if (PC && mePC && !PC.L.IsRecording)
                            {

                                if (PC.L.Events[PC.L.CurrentIdx] != mePC.L.Events[mePC.L.CurrentIdx])
                                {
                                    //execute move on player which colide
                                    if (Mov.Move(D))
                                    {
                                        UpdatePosition(Direction, D);
                                        if(ResetBetweenLoops) WM.AddRewindMove(this.gameObject, D);
                                        return true;
                                    }
                                }
                                else
                                {
                                    UpdatePosition(Direction, D);
                                    if(ResetBetweenLoops) WM.AddRewindMove(this.gameObject, D);
                                    return true;
                                }
                            }
                            else
                            {
                                if (Mov.Move(D))
                                {
                                    UpdatePosition(Direction, D);
                                    if(ResetBetweenLoops) WM.AddRewindMove(this.gameObject, D);
                                    return true;
                                }
                            }
                        }
                    }


                }
            }
        }
        else
        {
            UpdatePosition(Direction, D);
            return true;
        }
        return false;
    }

    void UpdatePosition(Vector2 Direction, PlayerController.Direction D)
    {
        LastPosition = this.gameObject.transform.position;

        this.gameObject.transform.position += new Vector3(Speed * Direction.x,
                                            Speed * Direction.y,
                                            0);

        NewPosition = this.gameObject.transform.position;
        EndAnimation = false;
    }

    // Start is called before the first frame update
    void Awake()
    {
        StartPosition = this.gameObject.transform.position;
        LastPosition = StartPosition;
        NewPosition = StartPosition;
        CurrentTime = AnimationTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!EndAnimation)
        {
            CurrentSpawnTailTime -= Time.deltaTime;

            if (CurrentSpawnTailTime <= 0)
            {
                // Spawn tail
                var PC = GetComponent<PlayerController>();
                if (PC)
                {
                    PC.SpawnTail();
                }
                CurrentSpawnTailTime = SpawnTailTime;
            }

            CurrentTime -= (Time.deltaTime / AnimationTime);
            if (CurrentTime <= 0)
            {
                EndAnimation = true;
                CurrentTime = AnimationTime;
            }

            if(SR) SR.transform.position = Vector2.Lerp(LastPosition, NewPosition, 1 - CurrentTime);
        }
        else
        {

            SR.transform.localPosition = Vector2.zero;
        }

        if (WM.NeedReset && ResetBetweenLoops && !WM.IsRewinding)
        {
            this.gameObject.transform.position = StartPosition;
            SR.transform.localPosition = Vector2.zero;
        }
    }
}
