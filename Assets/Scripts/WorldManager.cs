﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Reflection;
using UnityEngine;


static class Constants {
    // Debug
    static public int InputMode = 0;
    static public bool ShowDefaultTileOnCursor = true;
    static public bool ShowNextInputsOnTimelineOnReplay = true;
    // Global state
    static public float MoveAnimationTime = 0.2f;
    static public float RewindAnimationTime = 0.1f;

    // Names
    static public readonly string MAIN_CAMERA_NAME = "Main Camera";
}


public class WorldManager : MonoBehaviour, IControllable, ISavable {
    public WorldManager()
    {
        this.Mdl = new Model();
    }
    // will be saved aka invariant
    [System.Serializable]
    public class Model : IModel
    {
        [System.NonSerialized]
        public List<GameObject> Players = new List<GameObject>();

        public int CurrentTick = 0;
        public Timer AutoReplayTick;
        public Timer AutoRewindTick;


        public bool IsRewinding = false;
        public bool IsGoingBackward = false;
        public bool FixedUpdatePassed = false;

        public bool AutomaticReplay = false;
        public bool ForwardTick = false;
        public bool BackwardTick = false;

    }
    IModel ISavable.GetModel()
    {
        return Mdl;
    }
    public Model Mdl;
    // will not be saved
    public GameObject PlayerPrefab;
    public GameObject levelUI_GOref;
    private GameObject levelUI_GO;
    public GameObject StartTile;
    public Camera CurrentCamera;
    public StageSelector CurrentStageSelector;


    public InputManager IM;
    public MasterMixerControl MixerControl;

    private bool UpdatePlayers = false;
    public Timeline TL = null;

    private bool IsWaitingForAnimation = false;
    private bool __switchTLToLast = false;
    private int SavedIdx = 0;
    /// <summary>
    /// This function will create the prefab of player
    /// and add it to the current world manager to be managed by it for ticks
    /// </summary>
    /// <param name="P"></param>
    /// <returns></returns>
    public GameObject AddPlayer(Vector2 StartPosition)
    {
        var GO = Instantiate(PlayerPrefab, StartPosition, Quaternion.identity, this.gameObject.transform);
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
                GO.name = "Player " + Mdl.Players.Count;
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
        var GO = AddPlayer( StartTile.transform.position );
        var PC = GO.GetComponent<PlayerController>();
        IM.Attach(PC);
        IM.Attach(this);

        // NOTE(Toffa): We dont need to start the animation as we want to start white
        //CurrentCamera?.GetComponent<PostFXRenderer>().StartAnimation(GO.transform.position);

        levelUI_GO = Instantiate(levelUI_GOref, this.gameObject.transform);
        levelUI_GO.GetComponent<UITimeline>().setModel(this);
        levelUI_GO.GetComponent<UITimeline>().setDisplayedTimeline( PC.Mdl.TL );

        if (!MixerControl)
        {
            var Mixer = GameObject.Find("AudioMixerControl");
            if (Mixer) MixerControl = Mixer.GetComponent<MasterMixerControl>();
        }

        Mdl.AutoReplayTick.SetEndTime(Constants.RewindAnimationTime);
        Mdl.AutoRewindTick.SetEndTime(Constants.RewindAnimationTime);
        Movable.AnimationTime = Constants.MoveAnimationTime;
    }

    public void AddRewindMove(GameObject go, PlayerController.Direction D)
    {
        var Tick = Mdl.CurrentTick;
        TL.Rewind.Record(Tick, go, D);
    }

    void FixedUpdate()
    {
        if (UpdatePlayers && GetCurrentPlayer().GetComponent<Movable>().CanMove())
        {
            if (!Mdl.IsRewinding)
            {
                // Update physique here
                // it means that we can chose the order on which physics will be executed
                // Lets do it first loop to last loop
                if (!Mdl.IsGoingBackward)
                {
                    for (int i = SavedIdx; i < Mdl.Players.Count; ++i)
                    {
                        // deactivate physics if before break point to avoid any confusion
                        // when resolving physics between two loops not already breaken
                        //from
                        var PC = Mdl.Players[i].GetComponent<PlayerController>();

                        if(PC)
                        {
                            var BreakTick = PC.Mdl.TL.offset;
                            if(TL.last_tick < BreakTick)
                            {
                                // deactivate physics with all previous players
                                for (int j = 0; j < i; ++j)
                                {
                                    var PreviousPC = Mdl.Players[j].GetComponent<PlayerController>();
                                    Physics2D.IgnoreCollision(PC.GetComponent<BoxCollider2D>(), PreviousPC.GetComponent<BoxCollider2D>());
                                }
                            }
                            else
                            {
                                // activate all collisions
                                for (int j = 0; j < i; ++j)
                                {
                                    var PreviousPC = Mdl.Players[j].GetComponent<PlayerController>();
                                    Physics2D.IgnoreCollision(PC.GetComponent<BoxCollider2D>(), PreviousPC.GetComponent<BoxCollider2D>(), false);
                                }
                            }
                        }

                        Movable.MoveResult Result = PC.ApplyPhysics();
                        if(Result == Movable.MoveResult.IsAnimating)
                        {
                            // halt physics execution until animation of the object causing trouble is finished
                            IsWaitingForAnimation = true;
                            SavedIdx = i;
                            return;
                        }
                        // IMPORTANT: sync tranforms between physics to change positions of object for
                        // next physic tick
                        Physics2D.SyncTransforms();
                    }
                    Mdl.CurrentTick++;
                }
            }
            UpdatePlayers = false;
            IsWaitingForAnimation = false;
            SavedIdx = 0;
        }
        Mdl.FixedUpdatePassed = true;
    }

    void IControllable.ProcessInputs(Save.InputSaver.InputSaverEntry Entry)
    {
        var Up = Entry.Inputs["Up"].IsDown || Entry.isDpadUpPressed;
        var Down = Entry.Inputs["Down"].IsDown || Entry.isDpadDownPressed;
        var Right = Entry.Inputs["Right"].IsDown || Entry.isDpadRightPressed;
        var Left = Entry.Inputs["Left"].IsDown || Entry.isDpadLeftPressed;
        var SwitchTL =  Entry.Inputs["SwitchTL"].IsDown;

        var SwitchToWorld = Entry.Inputs["Cancel"].IsDown;
        if(SwitchToWorld)
        {
            CurrentStageSelector.UI.switchWorldToFullScreen();
            CurrentStageSelector.IM.Activate();
        }

        var ResetWorld = Entry.Inputs["Restart"].IsDown;
        if ( ResetWorld )
        {
            // TODO (mtn5): Add reset behavior again
            // oit was broken after coming from Scene based to GameObject based design :(
        }

        if (IM.CurrentMode == InputManager.Mode.RECORD)
        {
            if (Up || Down || Right || Left)
            {
                UpdatePlayers = true;

                // switch to latest timeline when moving
                __switchTLToLast = true;
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

                    CurrentCamera?.GetComponent<PostFXRenderer>()?.StartAnimation(CurrentPlayer.transform.position, Mdl.Players.Count-2, Mdl.Players.Count-1);
                }

                // switch to new timeline at break
                __switchTLToLast = true;
            }
        }


        Mdl.ForwardTick = Entry.Inputs["Tick"].IsDown
            || (Entry.Inputs["Tick"].Down && Mdl.AutoReplayTick.Ended());
        if (Mdl.ForwardTick) Mdl.AutoReplayTick.Restart();
        Mdl.BackwardTick = Entry.Inputs["BackTick"].IsDown && Mdl.FixedUpdatePassed && Mdl.CurrentTick != 0;

        if (Mdl.Players.Count != 0)
        {
            Mdl.BackwardTick = Mdl.BackwardTick && (IM.CurrentMode == InputManager.Mode.REPLAY || (TL.offset != Mdl.CurrentTick));
            Mdl.BackwardTick = Mdl.BackwardTick && (GetCurrentPlayer().GetComponent<Movable>().CanMove());
        }

        // Timeline switch
        //var SwitchTL = Entry.Inputs["SwitchTL"].IsDown;

        if ( SwitchTL )
        {
            switch_timeline();
        }

    }

    public PlayerController GetCurrentPlayer()
    {
        if (Mdl.Players.Count != 0)
            return Mdl.Players[Mdl.Players.Count - 1].GetComponent<PlayerController>();
        else
            return null;
    }

    private void switch_timeline()
    {
        UITimeline tl_ui        = levelUI_GO.GetComponent<UITimeline>();
        int next_loop_level = tl_ui.getDisplayedLoopLevel() + 1;
        if ( next_loop_level >= Mdl.Players.Count )
            next_loop_level = 0;


        CurrentCamera?.GetComponent<PostFXRenderer>()?.StartAnimation(Vector2.zero, Mathf.Max(0, tl_ui.getDisplayedLoopLevel()), Mathf.Max(0,next_loop_level));

        GameObject selected_player_for_tl = Mdl.Players[next_loop_level];
        Timeline tl_to_display = selected_player_for_tl.GetComponent<PlayerController>().Mdl.TL;
        tl_ui.trySwitchTimeline( tl_to_display);
    }

    private void switch_timeline_to_last()
    {
        UITimeline tl_ui         = levelUI_GO.GetComponent<UITimeline>();
        PlayerController last_pc = GetCurrentPlayer();
        tl_ui.trySwitchTimeline( last_pc.Mdl.TL );
        __switchTLToLast = false;
    }

    void RewindTimeline()
    {
        Movable.AnimationTime = Constants.RewindAnimationTime;
        if (MixerControl) MixerControl.SetPhasedMode();
        // If we were rewinding but it is finished, then we start the loops again
        if (TL.Rewind.IsEmpty())
        {
            if (MixerControl) MixerControl.SetNormalMode();
            Mdl.IsRewinding = false;
            Mdl.CurrentTick = 0;
            TL.reset();
            Movable.AnimationTime = Constants.MoveAnimationTime;
        }
        else
        {
            if (IsWaitingForAnimation || Mdl.AutoRewindTick.Ended())
            {
                Mdl.AutoRewindTick.Restart();

                SavedIdx = TL.Rewind.Tick(TL.Rewind.GameObjects.Count-1, SavedIdx);
                if (SavedIdx == -1)
                {
                    SavedIdx = 0;
                    TL.Rewind.DeleteRecord(TL.Rewind.GameObjects.Count-1, true);
                    IsWaitingForAnimation = false;
                    UpdateTimeLines(TL.Rewind.GameObjects.Count);
                }
                else IsWaitingForAnimation = true;
                return;
            }
        }
    }

    void UpdateTimeLines(int iTick)
    {
        foreach(GameObject PC in Mdl.Players)
        {
            PC.GetComponent<PlayerController>().Mdl.TL.setCurrentTick(iTick);
        }
    }

    void UpdateTimers()
    {
        Mdl.AutoReplayTick.Update(Time.deltaTime);
        Mdl.AutoRewindTick.Update(Time.deltaTime);
    }

    void Update()
    {
        if(IsWaitingForAnimation)
        {
            if(Mdl.IsRewinding)
            {
                RewindTimeline();
                return;
            }

            if (!Mdl.IsGoingBackward)
                return;
            else
            {
                SavedIdx = TL.Rewind.Tick(Mdl.CurrentTick, SavedIdx);
                if (SavedIdx == -1)
                {
                    SavedIdx = 0;
                    TL.Rewind.DeleteRecord(Mdl.CurrentTick, false);
                    IsWaitingForAnimation = false;
                }
                return;
            }
        }

        UpdateTimers();
        // Rewinding mechanic, deactivate all ticks
        if (Mdl.IsRewinding)
            RewindTimeline();
        // applytick
        else if (Mdl.ForwardTick || Mdl.BackwardTick)
        {
            Mdl.FixedUpdatePassed = false;
            Mdl.IsGoingBackward = Mdl.BackwardTick;

            if (Mdl.BackwardTick)
                Mdl.CurrentTick--;

            if (Mdl.IsGoingBackward)
            {
                SavedIdx = TL.Rewind.Tick(Mdl.CurrentTick, SavedIdx);
                if (SavedIdx == -1)
                {
                    SavedIdx = 0;
                    TL.Rewind.DeleteRecord(Mdl.CurrentTick, false);
                    IsWaitingForAnimation = false;
                }
                else
                {
                    IsWaitingForAnimation = true;
                }
            }
            else
            {
                UpdateTimeLines(Mdl.CurrentTick);
                if (TL.isTimelineOver())
                {
                    Mdl.IsRewinding = true;
                }
                UpdatePlayers = true;
            }
        }
        // move from player or else
        else
        {
            Mdl.IsGoingBackward = false;
            UpdateTimeLines(Mdl.CurrentTick);
            if (TL != null && TL.isTimelineOver())
            {
                Mdl.IsRewinding = true;
                IM.CurrentMode = InputManager.Mode.REPLAY;
                Mdl.AutoRewindTick.Restart();
            }
        }
        if (Constants.InputMode == 1)
        {
            // modal mode
        }

        if (__switchTLToLast)
            switch_timeline_to_last();

        if ( !!levelUI_GO )
            levelUI_GO.GetComponent<UITimeline>().refresh(IM.CurrentMode);
    }
}
