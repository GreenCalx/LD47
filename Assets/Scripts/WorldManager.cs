using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    // for rewind just record everything
    // TODO: do we really need a new class? cannot use a looper for this?
    // well not really because we have to record everything happening in a tick for every
    // players
    // This is a shitty design as it is not easy to print the current timeline value while rewinding for instance
    // the problem is that is can be hard to have something work well in reverse wioth physics and shit
    class Recorder
    {
        public void Record(int Tick, GameObject Go, PlayerController.Direction D)
        {
            // We are using a while loop because it is possible to 'leap' a tick
            // and have to add 2 ticks instead of one
            while (GameObjects.Count - 1 < Tick)
            {
                GameObjects.Add(new List<GameObject>());
                Directions.Add(new List<PlayerController.Direction>());
            }

            GameObjects[Tick].Add(Go);
            Directions[Tick].Add(D);
        }

        public void Tick()
        {
            var TickGameObjects = GameObjects[GameObjects.Count - 1];
            var TickDirections = Directions[Directions.Count - 1];

            TickGameObjects.Reverse();
            TickDirections.Reverse();

            for (int i = 0; i < TickGameObjects.Count; ++i)
            {
                var GO = TickGameObjects[i];
                var D = TickDirections[i];
                var Mover = GO.GetComponent<Movable>();
                if (Mover) Mover.Move(PlayerController.InverseDirection(D));
            }

            GameObjects.RemoveAt(GameObjects.Count - 1);
            Directions.RemoveAt(Directions.Count - 1);
        }

        public bool IsEmpty()
        {
            return GameObjects.Count == 0;
        }

        List<List<GameObject>> GameObjects = new List<List<GameObject>>();
        List<List<PlayerController.Direction>> Directions = new List<List<PlayerController.Direction>>();
    }
    Recorder Rewind = new Recorder();

    List<GameObject> Players = new List<GameObject>();
    public GameObject PlayerPrefab;
    public GameObject levelUI_GOref;


    public int CurrentTick = -1;
    public float TickRate = 1f; // 2 seconds
    float AutomaticReplayRate = 0.2f;
    float AutomaticReplayCurrentTime = 0;
    float CurrentTime = 0;

    public Vector2 StartPosition; //. First player will appear at this position

    public bool NeedTick; // When player has chose a direction
    public bool WaitForInput; // Playre is controlling so we wait for hi inputs
    public bool NeedReset;

    public bool IsRewinding = false;
    public bool IsGoingBackward = true;
    public bool FixedUpdatePassed = false;


    public MasterMixerControl MixerControl;


    // TODO: StateMachine

    /// <summary>
    /// This function will create the prefab of player
    /// and add it to the current world manager to be managed by it for ticks
    /// </summary>
    /// <param name="P"></param>
    /// <returns></returns>
    public GameObject AddPlayer(Vector2 StartPosition)
    {
        var GO = Instantiate(PlayerPrefab, StartPosition, Quaternion.identity);
        if (GO)
        {
            // prefab is deactivated on spectree as it is used only as prefab
            // and we dont want it to work
            GO.SetActive(true);

            // if we create a new player it means that we have to wait for inputs now
            WaitForInput = true;
            NeedTick = false;

            // Not sure it is needed anymore but we deactivate all collision between players
            // as the first frame will be a player insisde another one (from which we break)
            foreach (var Player in Players)
            {
                Physics2D.IgnoreCollision(Player.GetComponent<Collider2D>(), GO.GetComponent<Collider2D>());
            }
            Players.Add(GO);

            // copy previous plqyer vqriqbles inside new one
            if (Players.Count >= 2)
            {
                var PlayerA_Go = Players[Players.Count - 2];
                var PlayerA = PlayerA_Go.GetComponent<PlayerController>();
                var PlayerB = GO.GetComponent<PlayerController>();
                if (PlayerA && PlayerB)
                {
                    // update energy loop to get nested counter
                    //P.energyCounter = energyCounter.getNestedCounter();
                    PlayerB.timeline = PlayerA.timeline.getNestedTimeline();
                    if (!!PlayerA.levelUI)
                    {
                        PlayerB.levelUI = PlayerA.levelUI;
                        PlayerB.levelUI.updatePlayerRef(GO);
                        PlayerB.levelUI.refresh();
                        PlayerB.has_active_ui = true; // switch ui
                        PlayerA.has_active_ui = false;
                    }

                    // Update newly created looper with current loop previous
                    // frames
                    if (PlayerA.L.CurrentIdx != 0)
                    {
                        PlayerB.L.Events = PlayerA.L.Events.GetRange(0, PlayerA.L.CurrentIdx + 1);

                        // TODO : copy into world recording for rewind
                    }
                    PlayerB.L.StartRecording();
                    // IMPORTANT : this nees to be done after StartRecording as it will take current 
                    // position as start position and we dont want that
                    PlayerB.L.StartPosition = PlayerA.L.StartPosition;
                    PlayerB.GetComponent<Movable>().StartPosition = PlayerB.L.StartPosition;
                }

                // For now new players are randomly colored
                var SpriteRender = GO.GetComponentInChildren<SpriteRenderer>();
                if (SpriteRender)
                {
                    // new player and current loop will always be the bright color
                    var c = PlayerA.gameObject.GetComponentInChildren<SpriteRenderer>().color;
                    SpriteRender.color = new Color(c.r, c.g, c.b, 1);

                    // then we uodate every other player color to be darker
                    for (int i=0; i < Players.Count-1; ++i)
                    {
                        Players[i].GetComponentInChildren<SpriteRenderer>().color *= 0.7f;
                    }


                }

            }
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

        // Todo : do we really need this?
        // maybe we should use Awake?
        PC.Start();
        PC.has_active_ui = true;
        PC.L.StartRecording();

        if (!MixerControl)
        {
            var Mixer = GameObject.Find("AudioMixerControl");
            if (Mixer) MixerControl = Mixer.GetComponent<MasterMixerControl>();
        }

    }

    public void AddRewindMove(GameObject go, PlayerController.Direction D)
    {
        var Tick = CurrentTick;
        Rewind.Record(Tick, go, D);
    }

    // FixedUpdate
    void FixedUpdate()
    {
        if (!IsRewinding)
        {
            // Update physique here
            // it means that we can chose the order on which physics will be executed
            // Lets do it first loop to last loop
            if (!IsGoingBackward)
            {
                for (int i = 0; i < Players.Count; ++i)
                {
                    var PC = Players[i].GetComponent<PlayerController>();
                    PC.ApplyPhysics(IsGoingBackward);
                    // IMPORTANT: sync tranforms between physics to change positions of object for
                    // next physic tick
                    Physics2D.SyncTransforms();
                }
            } else
            {
                for (int i = Players.Count-1; i >= 0; --i)
                {
                    var PC = Players[i].GetComponent<PlayerController>();
                    PC.ApplyPhysics(IsGoingBackward);
                    // IMPORTANT: sync tranforms between physics to change positions of object for
                    // next physic tick
                    Physics2D.SyncTransforms();
                }

            }
        }
        FixedUpdatePassed = true;
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
            if (MixerControl) MixerControl.SetPhasedMode();

            NeedTick = false;
            WaitForInput = false;
            NeedReset = false;

            // If we were rewinding but it is finished, then we start the loops again
            if (Rewind.IsEmpty())
            {
                if (MixerControl) MixerControl.SetNormalMode();

                NeedReset = true;
                IsRewinding = false;
                CurrentTick = -1;

                foreach (var Player in Players)
                {
                    var Controller = Player.GetComponent<PlayerController>();
                    if (Controller) Controller.L.StartRunning();
                }
            }
            else
            {
                if (CurrentTime > TickRate)
                {
                    CurrentTime = 0;
                    Rewind.Tick();
                }
                return;
            }
        }

        // For now ticks are done by hand!

        if (Input.GetButton("Tick"))
        {
            AutomaticReplayCurrentTime += Time.deltaTime;
        }
        else
        {
            AutomaticReplayCurrentTime = 0;
        }
        bool ForwardTick = Input.GetButtonDown("Tick") || (WaitForInput && NeedTick) || ( Input.GetButton("Tick") && AutomaticReplayCurrentTime > AutomaticReplayRate);
        bool BackwardTick = Input.GetKeyDown(KeyCode.X) && FixedUpdatePassed && CurrentTick != -1;

        

        if (ForwardTick || BackwardTick)
        {

            FixedUpdatePassed = false;

            IsGoingBackward = BackwardTick;


            if (BackwardTick)
                CurrentTick--;
            if (ForwardTick)
                CurrentTick++;

            AutomaticReplayCurrentTime = 0;

            NeedTick = false;
            //CurrentTime = 0;
            // See if we arrived to the longest loop end
            // if thats the case we reset all loops to be started again frame 0
            NeedReset = false;
            // now this is done by the timeline
#if false
            if (Players.Count >= 1) // No need if no players
            {
                var Controller = Players[0].GetComponent<PlayerController>(); // supposed to be longest
                if (Controller.L.IsRunning) // Only if Running ?
                {
                    if (CurrentTick >= Controller.L.Events.Count)
                    {
                        NeedReset = true;
                    }
                }
            }
#endif
            // try consume energy for last player and update its ui
            int PlayersCount = Players.Count;
            if (PlayersCount > 0)
            {
                PlayerController CurrentController = Players[PlayersCount - 1].GetComponent<PlayerController>();
                //EnergyCounter ec = curr_pc.energyCounter;
                Timeline TL = CurrentController.timeline;
                if (!NeedReset)
                {
                    LevelUI UI = CurrentController.levelUI;

                    bool CanMove = TL.getAt(CurrentTick);
                    bool TLOver = TL.isTimelineOver();
                    TL.setCurrentTick(CurrentTick);
                    Debug.Log("CURRENT TICK: " + CurrentTick);
                    if (!!UI)
                        UI.refresh(); //update if new cell
                    if (TLOver)
                    {
                        CurrentController.L.StopRecording();
                        WaitForInput = false;
                        NeedReset = true;
                        TL.reset();
                    }
                    else
                    {
                        CurrentController.WAIT_ORDER = !CanMove;
                    }
                    if (!!UI)
                        UI.refresh();
                }
                else
                {
                    TL.reset();
                }
            }
            // require world tick, update all loops, etc
            foreach (var Player in Players)
            {
                var Controller = Player.GetComponent<PlayerController>();
                if (Controller)
                {
                    if (NeedReset)
                    {
                        Controller.L.IsRunning = true;
                        Controller.IsLoopedControled = true;
                        IsRewinding = true;
                    }
                }
            }
        }
    }
}
