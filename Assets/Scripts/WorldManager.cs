using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Reflection;
using UnityEngine;


static class Constraints
{
    static public int InputMode = 0;
    static public bool ShowDefaultTileOnCursor = true;
    static public bool ShowNextInputsOnTimelineOnReplay = true;
}

public class WorldManager : MonoBehaviour, IControllable, ISavable {
    // will be saved aka invariant
    [System.Serializable]
    public class Model : IModel
    {
        [System.NonSerialized]
        public List<GameObject> Players = new List<GameObject>();

        public int CurrentTick = 0;
        public float AutomaticReplayCurrentTime = 0;
        public float CurrentTime = 0;

        public bool IsRewinding = false;
        public bool IsGoingBackward = false;
        public bool FixedUpdatePassed = false;

        public bool AutomaticReplay = false;
        public bool ForwardTick = false;
        public bool BackwardTick = false;

        public float TickRate = 1f; // 2 seconds
        public float AutomaticReplayRate = 0.2f;
    }
    IModel ISavable.GetModel()
    {
        return Mdl;
    }
    public Model Mdl = new Model();
    // will not be saved
    public GameObject PlayerPrefab;
    public GameObject levelUI_GOref;
    private GameObject levelUI_GO;

    public Vector2 StartPosition; //. First player will appear at this position

    public InputManager IM;

    public MasterMixerControl MixerControl;
    public Timeline TL;

    public bool UpdatePlayers = false;
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
            // Not sure it is needed anymore but we deactivate all collision between players
            // as the first frame will be a player insisde another one (from which we break)
            foreach (var Player in Mdl.Players)
            {
                Physics2D.IgnoreCollision(Player.GetComponent<Collider2D>(), GO.GetComponent<Collider2D>());
            }
            if (TL != null)
               TL = TL.getNestedTimeline();
            else
               TL = new Timeline(0);

            // copy previous plqyer vqriqbles inside new one
            if (Mdl.Players.Count != 0)
            {
                var PlayerA = GetCurrentPlayer().GetComponent<PlayerController>();
                Mdl.Players.Add(GO);
                var PlayerB = GO.GetComponent<PlayerController>();
                PlayerB.Mdl.TL = TL;

                for (int i = 0; i < TL.last_tick; ++i)
                {
                    // IMPORTANT (mtn5): do not cache the count as we modify it during loop
                    for (int j = 0; j < TL.Rewind.GameObjects[i].Count; ++j)
                    {
                        if (TL.Rewind.GameObjects[i][j] == PlayerA.gameObject)
                        {
                            TL.Rewind.GameObjects[i].Insert(j + 1, PlayerB.gameObject);
                            TL.Rewind.Directions[i].Insert(j + 1, TL.Rewind.Directions[i][j]);
                            j++;
                        }
                    }
                }

                if (PlayerA && PlayerB)
                {
                    PlayerB.GetComponent<Movable>().StartPosition = PlayerA.GetComponent<Movable>().StartPosition;
                }
                // For now new players are randomly colored
                var SpriteRender = GO.GetComponentInChildren<SpriteRenderer>();
                if (SpriteRender)
                {
                    // new player and current loop will always be the bright color
                    var c = PlayerA.gameObject.GetComponentInChildren<SpriteRenderer>().color;
                    SpriteRender.color = new Color(c.r, c.g, c.b, 1);
                    // then we uodate every other player color to be darker
                    for (int i=0; i < Mdl.Players.Count-1; ++i)
                    {
                        Mdl.Players[i].GetComponentInChildren<SpriteRenderer>().color *= 0.7f;
                    }
                }
            }
            else
            {
                Mdl.Players.Add(GO);
                var PlayerB = GO.GetComponent<PlayerController>();
                PlayerB.Mdl.TL = TL;
            }
        }
        return GO;
    }
    // Start is called before the first frame update
    void Start()
    {
        Mdl = new Model();
        var GO = AddPlayer(StartPosition);
        var PC = GO.GetComponent<PlayerController>();
        IM.Attach(PC);
        IM.Attach(this);

        levelUI_GO = Instantiate(levelUI_GOref);
        levelUI_GO.GetComponent<LevelUI>().setModel(this);

        if (!MixerControl)
        {
            var Mixer = GameObject.Find("AudioMixerControl");
            if (Mixer) MixerControl = Mixer.GetComponent<MasterMixerControl>();
        }
    }

    public void AddRewindMove(GameObject go, PlayerController.Direction D)
    {
        var Tick = Mdl.CurrentTick;
        TL.Rewind.Record(Tick, go, D);
    }

    void FixedUpdate()
    {
        if (UpdatePlayers)
        {
            if (!Mdl.IsRewinding)
            {
                // Update physique here
                // it means that we can chose the order on which physics will be executed
                // Lets do it first loop to last loop
                if (!Mdl.IsGoingBackward)
                {
                    for (int i = 0; i < Mdl.Players.Count; ++i)
                    {
                        var PC = Mdl.Players[i].GetComponent<PlayerController>();
                        PC.ApplyPhysics(Mdl.IsGoingBackward);
                        // IMPORTANT: sync tranforms between physics to change positions of object for
                        // next physic tick
                        Physics2D.SyncTransforms();
                    }
                    Mdl.CurrentTick++;
                }
            }
            UpdatePlayers = false;
        }
        Mdl.FixedUpdatePassed = true;
    }

    void IControllable.ProcessInputs(Save.InputSaver.InputSaverEntry Entry)
    {
        var Up = Entry.Inputs["Up"].IsDown || Entry.isDpadUpPressed;
        var Down = Entry.Inputs["Down"].IsDown || Entry.isDpadDownPressed;
        var Right = Entry.Inputs["Right"].IsDown || Entry.isDpadRightPressed;
        var Left = Entry.Inputs["Left"].IsDown || Entry.isDpadLeftPressed;
        if (IM.CurrentMode == InputManager.Mode.RECORD)
        {
            if (Up || Down || Right || Left)
            {
                UpdatePlayers = true;
            }
        }
        else
        {
            bool needBreak = (Up || Down || Right || Left || Entry.Inputs["Break"].IsDown);
            if (needBreak && !Mdl.IsRewinding)
            {
                // create a new player at current position
                if (Mdl.Players.Count != 0)
                {
                    var CurrentPlayer = Mdl.Players[Mdl.Players.Count - 1];
                    var GO = AddPlayer(CurrentPlayer.transform.position);
                    IM.Detach(CurrentPlayer.GetComponent<PlayerController>());
                    CurrentPlayer.GetComponent<PlayerController>().IM = new InputManager();
                    CurrentPlayer.GetComponent<PlayerController>().IM.CurrentMode = InputManager.Mode.REPLAY;
                    IM.Attach(GO.GetComponent<PlayerController>());
                    IM.CurrentMode = InputManager.Mode.RECORD;
                }
            }
        }

        Mdl.ForwardTick = Entry.Inputs["Tick"].IsDown
            || (Entry.Inputs["Tick"].Down && Mdl.AutomaticReplayCurrentTime > Mdl.AutomaticReplayRate);
        Mdl.BackwardTick = Entry.Inputs["BackTick"].IsDown && Mdl.FixedUpdatePassed && Mdl.CurrentTick != -1;

        if (Mdl.Players.Count != 0)
        {
            PlayerController LastPlayer = GetCurrentPlayer();
            Mdl.BackwardTick = Mdl.BackwardTick && (IM.CurrentMode == InputManager.Mode.REPLAY || (TL.offset != Mdl.CurrentTick));
        }
    }

    public PlayerController GetCurrentPlayer()
    {
        if (Mdl.Players.Count != 0)
            return Mdl.Players[Mdl.Players.Count - 1].GetComponent<PlayerController>();
        else
            return null;
    }

    void RewindTimeline()
    {
        if (MixerControl) MixerControl.SetPhasedMode();
        Mdl.CurrentTime += Time.deltaTime;

        // If we were rewinding but it is finished, then we start the loops again
        if (TL.Rewind.IsEmpty())
        {
            if (MixerControl) MixerControl.SetNormalMode();
            Mdl.IsRewinding = false;
            Mdl.CurrentTick = 0;
            TL.reset();
        }
        else
        {
            if (Mdl.CurrentTime > Mdl.TickRate)
            {
                Mdl.CurrentTime = 0;
                UpdateTimeLines(TL.Rewind.Tick());
            }
            return;
        }

    }

    void UpdateTimeLines(int iTick)
    {
        foreach(GameObject PC in Mdl.Players)
        {
            PC.GetComponent<PlayerController>().Mdl.TL.setCurrentTick(iTick);
        }
    }

    void Update()
    {
        // todo: real timers
        Mdl.AutomaticReplayCurrentTime += Time.deltaTime;
        // Rewinding mechanic, deactivate all ticks
        if (Mdl.IsRewinding)
        {
            RewindTimeline();
        }
        // applytick
        else if (Mdl.ForwardTick || Mdl.BackwardTick)
        {
            Mdl.FixedUpdatePassed = false;
            Mdl.IsGoingBackward = Mdl.BackwardTick;

            if (Mdl.BackwardTick)
                Mdl.CurrentTick--;

            Mdl.AutomaticReplayCurrentTime = 0;

            if (Mdl.IsGoingBackward)
            {
                TL.Rewind.Tick(Mdl.CurrentTick);
                TL.Rewind.DeleteRecord(Mdl.CurrentTick);
            }
            else
            {
                UpdateTimeLines(Mdl.CurrentTick);
                if (TL.isTimelineOver())
                {
                    Mdl.IsRewinding = true;
                    Mdl.CurrentTime = 0;
                }
                UpdatePlayers = true;
            }
        }
        else
        {
            Mdl.IsGoingBackward = false;
            UpdateTimeLines(Mdl.CurrentTick);
            if (TL.isTimelineOver())
            {
                Mdl.IsRewinding = true;
                IM.CurrentMode = InputManager.Mode.REPLAY;
            }
        }
        if (Constraints.InputMode == 1)
        {
            // modal mode

        }

        levelUI_GO.GetComponent<LevelUI>().refresh(TL, IM.CurrentMode);
    }
}
