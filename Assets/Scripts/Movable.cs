using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class Movable : MonoBehaviour
{
    [SerializeField] private LayerMask wallmask;
    [SerializeField] private LayerMask movablemask;
    public float Speed = 1f;

    public bool Move(Vector2 Direction)
    {
        Vector3 Dir3 = new Vector3(Direction.x, Direction.y, 0);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, wallmask);
        if (hits.Length != 0) return false;

        RaycastHit2D[] hitsm = Physics2D.RaycastAll(transform.position + (0.6f * Dir3), Dir3, 0.5f, movablemask);
        hitsm = hitsm.Where(val => (val.collider.gameObject != this.gameObject)).ToArray();
        if (hitsm.Length == 0)
        {
            this.gameObject.transform.position += new Vector3(Speed * Direction.x,
                                                Speed * Direction.y,
                                                0);
            return true;
        }

        if (hitsm.Length > 1) Debug.Log("weird shit going on: founding too many collisions");

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
                        if (PC && mePC)
                        {
                            if (PC.L.Events[PC.L.CurrentIdx] != mePC.L.Events[mePC.L.CurrentIdx])
                            {
                                //execute move on player which colide
                                if (Mov.Move(Direction))
                                {
                                    this.gameObject.transform.position += new Vector3(Speed * Direction.x,
                                                            Speed * Direction.y,
                                                            0);
                                    return true;
                                }
                            }
                            else
                            {
                                this.gameObject.transform.position += new Vector3(Speed * Direction.x,
                                                            Speed * Direction.y,
                                                            0);
                                return true;
                            }
                        } else
                        {
                            if (Mov.Move(Direction))
                            {
                                this.gameObject.transform.position += new Vector3(Speed * Direction.x,
                                                        Speed * Direction.y,
                                                        0);
                                return true;
                            }
                        }
                    }
                }


            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
