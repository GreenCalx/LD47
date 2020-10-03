using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    List<GameObject> Players = new List<GameObject>();
    public GameObject PlayerPrefab;

    int CurrentTick = 0;

    public GameObject AddPlayer( Vector2 P)
    {
        var GO = Instantiate(PlayerPrefab, P, Quaternion.identity);
        Players.Add(GO);
        var PC = GO.GetComponent<PlayerController>();
        GO.SetActive( true );
        return GO;
    }
    // Start is called before the first frame update
    void Start()
    {
        AddPlayer(new Vector2());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool NeedReset = false;
            if (Players.Count >= 1)
            {
                var P = Players[0].GetComponent<PlayerController>(); // supposed to be longest
                if (P.L.IsRunning)
                {
                    if (CurrentTick >= P.L.Events.Count)
                    {
                        NeedReset = true;
                        CurrentTick = 0;
                    }
                }
            }

            // require world tick, update all loops, etc
            foreach ( var Player in Players )
            {
                var go = Player.GetComponent<PlayerController>();
                if (NeedReset)
                {
                    go.L.Reset();
                }
                else
                {
                    if (go)
                    {
                        go.RequireTick(CurrentTick);
                    }
                }
            }
            
            if( !NeedReset ) CurrentTick++;
        }
    }
}
