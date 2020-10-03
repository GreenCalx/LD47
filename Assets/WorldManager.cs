using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    List<GameObject> Players = new List<GameObject>();
    public GameObject PlayerPrefab;


    int CurrentTick = 0;
    public float TickRate = 2f; // 2 seconds
    float CurrentTime = 0;

    public Vector2 StartPosition; //. First player will appear at this position

    /// <summary>
    /// This function will create the prefab of player
    /// and add it to the current world manager to be managed by it for ticks
    /// </summary>
    /// <param name="P"></param>
    /// <returns></returns>
    public GameObject AddPlayer( Vector2 P)
    {
        var GO = Instantiate(PlayerPrefab, P, Quaternion.identity);
        if (GO)
        {
            // prefab is deactivated on spectree as it is used only as prefab
            // and we dont want it to work
            GO.SetActive(true);

            foreach(var Player in Players)
            {
                Physics2D.IgnoreCollision(Player.GetComponent<Collider2D>(), GO.GetComponent<Collider2D>());
            }

            Players.Add(GO);
        }
        return GO;
    }
    // Start is called before the first frame update
    void Start()
    {
        var GO = AddPlayer(StartPosition);
        var PC = GO.GetComponent<PlayerController>();
        PC.Start();
        PC.L.StartRecording();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        // For now ticks are done by hand!
        if (Input.GetKeyDown(KeyCode.M) || CurrentTime > TickRate)
        {
            CurrentTime = 0;
            // See if we arrived to the longest loop end
            // if thats the case we reset all loops to be started again frame 0
            bool NeedReset = false;
            if (Players.Count >= 1) // No need if no players
            {
                var P = Players[0].GetComponent<PlayerController>(); // supposed to be longest
                if (P.L.IsRunning) // Only if Running ?
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
                if (NeedReset) go.L.ReStart();
                else if (go) go.RequireTick(CurrentTick);
            }
            // Only increment Curenttick if we didn't need to reset
            if( !NeedReset ) CurrentTick++;
        }
    }
}
