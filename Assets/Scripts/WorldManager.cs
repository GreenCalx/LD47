using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    List<GameObject> Players = new List<GameObject>();
    public GameObject PlayerPrefab;
    public GameObject levelUI_GOref;


    int CurrentTick = 0;
    public float TickRate = 1f; // 2 seconds
    float CurrentTime = 0;

    public Vector2 StartPosition; //. First player will appear at this position

    public bool NeedTick; // When player has chose a direction
    public bool WaitForInput; // Playre is controlling so we wait for hi inputs
    public bool NeedReset;
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

            WaitForInput = true;
            NeedTick = false;

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
        if (!!levelUI_GOref)
            PC.initUI(levelUI_GOref);
        else
            Debug.Log("WorldManager : NO UI FOUND.");
        PC.Start();
        PC.L.StartRecording();
    }

    // FixedUpdate
    void FixedUpdate()
    {
        // Update physique here
        // it means that we can chose the order on which physics will be executed
        // Lets do it first loop to last loop
        for(int i = 0; i < Players.Count; ++i)
        {
            var PC = Players[i].GetComponent<PlayerController>();
            PC.ApplyPhysics();
            Physics2D.SyncTransforms();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!WaitForInput) {
            CurrentTime += Time.deltaTime;
        }
        // For now ticks are done by hand!
        if (Input.GetKeyDown(KeyCode.M) || (!WaitForInput && (CurrentTime > TickRate)) || (WaitForInput && NeedTick))
        {
            NeedTick = false;
            CurrentTime = 0;
            // See if we arrived to the longest loop end
            // if thats the case we reset all loops to be started again frame 0
            NeedReset = false;
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

            // try consume energy for last player and update its ui
            int n_players = Players.Count;
            if (n_players>0)
            {
                PlayerController curr_pc = Players[n_players-1].GetComponent<PlayerController>();
                EnergyCounter ec = curr_pc.energyCounter;
                LevelUI ui = curr_pc.levelUI;

                bool has_energy_left = ec.tryConsume();
                if (!!ui)
                    ui.refresh(); //update if new cell
                bool is_energy_locked   = ec.has_locked_energy;
                if (!has_energy_left && !is_energy_locked)
                {
                    curr_pc.L.StopRecording();
                    curr_pc.L.StartRunning();
                    ec.refillAllCells();

                } else { curr_pc.WAIT_ORDER = is_energy_locked; }
                if (!!ui)
                    ui.refresh();
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
