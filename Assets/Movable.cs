using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;

public class Movable : MonoBehaviour
{
    [SerializeField] private LayerMask wallmask;
    [SerializeField] private LayerMask movablemask;
    public float Speed = 1f;

    public bool Move( Vector2 Direction )
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Direction, 1f, wallmask);
        if (hits.Length != 0) return false;

        RaycastHit2D[] hitsm = Physics2D.RaycastAll(transform.position, Direction, 1f, movablemask);
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
                    var Move = hit.collider.GetComponent<Movable>();
                    if (Move)
                    {
                        if (Move.Move(Direction))
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
