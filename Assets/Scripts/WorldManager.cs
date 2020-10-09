using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    // for rewind just record everything
    public List<List<GameObject>> RewindGO = new List<List<GameObject>>();
    public List<List<PlayerController.Direction>> RewindDirection = new List<List<PlayerController.Direction>>();

    List<GameObject> Players = new List<GameObject>();
    public GameObject PlayerPrefab;
    public GameObject levelUI_GOref;


    public int CurrentTick = 0;
    public float TickRate = 1f; // 2 seconds
    float CurrentTime = 0;

    public Vector2 StartPosition; //. First player will appear at this position

    public bool NeedTick; // When player has chose a direction
    public bool WaitForInput; // Playre is controlling so we wait for hi inputs
    public bool NeedReset;

    public bool IsRewinding = false;
    /// <summary>
    /// This function will create the prefab of player
    /// and add it to the current world manager to be managed by it for ticks
    /// </summary>
    /// <param name="P"></param>
    /// <returns></returns>
    public GameObject AddPlayer(Vector2 P)
    {
        var GO = Instantiate(PlayerPrefab, P, Quaternion.identity);
        if (GO)
        {
            // prefab is deactivated on spectree as it is used only as prefab
            // and we dont want it to work
            GO.SetActive(true);

            WaitForInput = true;
            NeedTick = false;

            foreach (var Player in Players)
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
        PC.has_active_ui = true;
        PC.L.StartRecording();
    }

    public void AddRewindMove(GameObject go, PlayerController.Direction D)
    {
        // currenttick was unfiortunately already advanced when called here
        // therefore remove 1
        var Tick = CurrentTick - 1;
        while (RewindGO.Count - 1 < Tick)
        {
            RewindGO.Add(new List<GameObject>());
            RewindDirection.Add(new List<PlayerController.Direction>());
        }

        Debug.Log(RewindGO.Count);
        RewindGO[Tick].Add(go);
        RewindDirection[Tick].Add(D);
    }

    // FixedUpdate
    void FixedUpdate()
    {
         if(!IsRewinding)
        {
            // Update physique here
            // it means that we can chose the order on which physics will be executed
            // Lets do it first loop to last loop
            for (int i = 0; i < Players.Count; ++i)
            {
                var PC = Players[i].GetComponent<PlayerController>();
                PC.ApplyPhysics(IsRewinding);
                Physics2D.SyncTransforms();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!WaitForInput)
        {
            CurrentTime += Time.deltaTime;
        }

        if (IsRewinding)
        {
            MasterMixerControl control = null;
            var mixer = GameObject.Find("AudioMixerControl");
            if( mixer)
            {
                control = mixer.GetComponent<MasterMixerControl>();
                if (control)
                {
                    control.SetPhasedMode();
                }
            }

            NeedTick = false;
            WaitForInput = false;
            NeedReset = false;

            if (RewindGO.Count == 0)
            {
                if (control) control.SetNormalMode();

                NeedReset = true;
                IsRewinding = false;
                CurrentTick = 0;

                foreach (var Player in Players)
                {
                    var go = Player.GetComponent<PlayerController>();


                    if (go) {

                        var m= go.GetComponent<Movable>();
                       // m.CurrentTime = 0.5f;
                       // m.AnimationTime = 0.5f;
                        go.L.StartRunning();
                    }

                }
            }
            else
            {
                if (CurrentTime > TickRate)
                {
                    CurrentTime = 0;

                    var go_list = RewindGO[RewindGO.Count - 1];
                    var d_list = RewindDirection[RewindDirection.Count - 1];

                    go_list.Reverse();
                    d_list.Reverse();

                    for (int i = 0; i < go_list.Count; ++i)
                    {
                        var go = go_list[i];
                        var d = d_list[i];
                        var m = go.GetComponent<Movable>();

                        if (d == PlayerController.Direction.UP) d = PlayerController.Direction.DOWN;
                        else if (d == PlayerController.Direction.DOWN) d = PlayerController.Direction.UP;
                        else if (d == PlayerController.Direction.RIGHT) d = PlayerController.Direction.LEFT;
                        else if (d == PlayerController.Direction.LEFT) d = PlayerController.Direction.RIGHT;

                        if (m)
                        {
                            //m.AnimationTime = TickRate;
                            //m.CurrentTime = TickRate;
                            m.Move(d);
                        }
                    }

                    RewindDirection.RemoveAt(RewindDirection.Count - 1);
                    RewindGO.RemoveAt(RewindGO.Count - 1);
                }
                return;
            }
        }

        // For now ticks are done by hand!
        if (Input.GetButtonDown("Tick")
            //|| (!WaitForInput && (CurrentTime > TickRate)) 
            || (WaitForInput && NeedTick))
        {
            NeedTick = false;
            //CurrentTime = 0;
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
                        //CurrentTick = 0;
                    }
                }
            }

            // try consume energy for last player and update its ui
            int n_players = Players.Count;
            if (n_players > 0)
            {
                PlayerController curr_pc = Players[n_players - 1].GetComponent<PlayerController>();
                //EnergyCounter ec = curr_pc.energyCounter;
                Timeline tl = curr_pc.timeline;
                if (!NeedReset)
                {

                    LevelUI ui = curr_pc.levelUI;

                    //bool has_energy_left = ec.tryConsume();
                    bool has_move       = tl.getAt(CurrentTick);
                    bool timeline_over  = tl.isTimelineOver();
                    tl.last_tick = CurrentTick;
                    if (!!ui)
                        ui.refresh(); //update if new cell
                    if (timeline_over)
                    {
                        curr_pc.L.StopRecording();
                        WaitForInput = false;
                        NeedReset    = true;
                        tl.reset();
                    } else {
                        curr_pc.WAIT_ORDER = !has_move;
                    }
                    if (!!ui)
                        ui.refresh();
                }
                else
                {
                    tl.reset();
                }
            }
            // require world tick, update all loops, etc
            foreach (var Player in Players)
            {
                var go = Player.GetComponent<PlayerController>();
                if (NeedReset)
                {
                    //go.L.ReStart();
                    go.L.IsRunning = true;
                    go.IsLoopedControled = true;
                    IsRewinding = true;
                }
                else if (go) go.RequireTick(CurrentTick);
            }
            // Only increment Curenttick if we didn't need to reset
            if (!NeedReset) CurrentTick++;
        }
    }
}
