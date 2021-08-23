using System.Collections;
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

public interface ITickController
{
    void Tick();
    void AddListener(int Index, ITickObserver Listener); 
}

public interface ITickObserver
{
    void OnTick();
    void SetControler(ITickController Controler);
}

public class TickClock : MonoBehaviour, ITickController {
    public List<ITickObserver> _Listeners = new List<ITickObserver>();
    public void AddListener(int Index, ITickObserver Listener) {
        // TODO(toffa): add all necessary checks
        if (_Listeners.Find((ITickObserver obj) => Listener == obj) != null) return;
        if (Index >= _Listeners.Count - 1)
        {
            AddListener(Listener);
            return;
        }
        Listener.SetControler(this);
        _Listeners.Insert(Index, Listener);
    }
    public void AddListener(ITickObserver Listener) {
        // TODO(toffa): add all necessary checks
        if (_Listeners.Find((ITickObserver obj) => Listener == obj) != null) return;
        Listener.SetControler(this);
        _Listeners.Add(Listener);
    }
    public void AddListener(ITickObserver Index, ITickObserver Listener)
    {
       AddListener( _Listeners.FindIndex((ITickObserver obj) => obj == Index), Listener);
    }

    public virtual void Tick() { }
}

public class TickBased : MonoBehaviour, ITickObserver { 
    private ITickController _Controler;
    public void SetControler(ITickController Controler)
    {
        _Controler = Controler;
    }

    public virtual void OnTick() { }
}

public class WorldManager : TickClock, IControllable, ISavable {
    // Save boilerplate
    public WorldManager()
    {
        this.Mdl = new Model();
    }
    [System.Serializable]
    public class Model : IModel
    {
        [System.NonSerialized]
        public List<PlayerController> Players = new List<PlayerController>();

        public Timer AutoReplayTick;
        public Timer AutoRewindTick;


        public bool IsRewinding = false;
        public bool IsGoingBackward = false;

        public bool AutomaticReplay = false;
        public bool ForwardTick = false;
        public bool BackwardTick = false;

    }
    IModel ISavable.GetModel() { return Mdl; }
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

    public PlayerTimeline TL = new PlayerTimeline(0);
    public bool needTick = false;
    /// <summary>
    /// Add a new player to the list of current players in the wolrld.
    /// The list will remain sorted.
    /// It will also add the player as a TickListener of this world, and will be called at each tick.
    /// </summary>
    /// <param name="StartPosition"></param>
    /// <returns></returns>
    public PlayerController AddPlayer(Vector2 StartPosition)
    {
        var NewPlayer_GO = Instantiate(PlayerPrefab, StartPosition, Quaternion.identity, this.gameObject.transform);
        if (!NewPlayer_GO) return null;
        var NewPlayer = NewPlayer_GO.GetComponent<PlayerController>();
        var PreviousPlayer = GetCurrentPlayer();
        // prefab is deactivated on spectree as it is used only as prefab
        // and we dont want it to work
        NewPlayer_GO.SetActive(true);
        // if we create a new player it means that we have to wait for inputs now
        // Not sure it is needed anymore but we deactivate all collision between players
        // as the first frame will be a player insisde another one (from which we break)
        foreach (var Player in Mdl.Players) { Physics2D.IgnoreCollision(Player.GetComponent<Collider2D>(), NewPlayer_GO.GetComponent<Collider2D>()); }
        // Create the new timeline. It will not be done the first time as it will be null
        // copy previous player variables inside new one
        if (Mdl.Players.Count != 0) {
            TL = TL?.GetNestedTimeline();
            NewPlayer.GetComponent<Movable>().StartPosition = PreviousPlayer.GetComponent<Movable>().StartPosition;
            // For now new players are randomly colored
            var SpriteRender = NewPlayer.GetComponentInChildren<SpriteRenderer>();
            if (SpriteRender) {
                // new player and current loop will always be the bright color
                var PreviousColor = PreviousPlayer.gameObject.GetComponentInChildren<SpriteRenderer>().color;
                SpriteRender.color = new Color(PreviousColor.r, PreviousColor.g, PreviousColor.b, 1);
                // then we uodate every other player color to be darker
                for (int i = 0; i < Mdl.Players.Count - 1; ++i) { Mdl.Players[i].GetComponentInChildren<SpriteRenderer>().color *= 0.7f; }
            }
        } 
        NewPlayer.Mdl.TL = TL;
        TL.SetPlayer(NewPlayer);
        Mdl.Players.Add(NewPlayer);
        AddListener(GetCurrentPlayer() as ITickObserver);
        return NewPlayer;
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

        GameObject stage_selec_go = GameObject.Find("stage_selector");
        CurrentStageSelector = stage_selec_go.GetComponent<StageSelector>();
    }

    public override void Tick()
    {
        needTick = true;
        UpdateTimelines();
    }

    void FixedUpdate()
    {
        if (!needTick) return;

        FixedUpdateTimelines();
#if false
        if (!Mdl.IsGoingBackward)
        {
            foreach (ITickObserver Obs in _Listeners)
            {
                Obs.OnTick();
                // IMPORTANT: sync tranforms between physics to change positions of object for
                // next physic tick
                Physics2D.SyncTransforms();
            }
        } else
        {
            _Listeners.Reverse();
            foreach (ITickObserver Obs in _Listeners)
            {
                Obs.OnTick();
                // IMPORTANT: sync tranforms between physics to change positions of object for
                // next physic tick
                Physics2D.SyncTransforms();
            }
            _Listeners.Reverse();
        }
#endif

        needTick = false;
    }

    /// <summary>
    /// Every input goes there. If you have another input it goes there!
    /// This function should NOT HAVE ANY EFFECT if possible, see this as a new "Update"
    /// This functino should NOT HAVE ANY PHYSICS
    /// Right now we simply dont get any inputs if 
    /// </summary>
    /// <param name="Entry"></param>
    void IControllable.ProcessInputs(Save.InputSaver.InputSaverEntry Entry)
    {
        if (!CanTick()) return;
        if (Mdl.IsRewinding) return;

        var Up = Entry.Inputs["Up"].IsDown || Entry.isDpadUpPressed;
        var Down = Entry.Inputs["Down"].IsDown || Entry.isDpadDownPressed;
        var Right = Entry.Inputs["Right"].IsDown || Entry.isDpadRightPressed;
        var Left = Entry.Inputs["Left"].IsDown || Entry.isDpadLeftPressed;
        var SwitchTL =  Entry.Inputs["SwitchTL"].IsDown;
        var SwitchWorldLevel = Entry.Inputs["Cancel"].IsDown;
        var ResetWorld = Entry.Inputs["Restart"].IsDown;
        var Break = Entry.Inputs["Break"].IsDown;

        if(SwitchWorldLevel)
        {
            CurrentStageSelector.UI.switchWorldToFullScreen();
            CurrentStageSelector.IM.Activate();
            return;
        }

        if ( ResetWorld )
        {
            // TODO (mtn5): Add reset behavior again
            // oit was broken after coming from Scene based to GameObject based design :(
        }

        bool AnyDirection = (Up || Down || Right || Left);
        bool ForwardTick = (IM.CurrentMode == InputManager.Mode.RECORD && AnyDirection) 
            || (Entry.Inputs["Tick"].IsDown) 
            || (Entry.Inputs["Tick"].Down && Mdl.AutoReplayTick.Ended());
        if (ForwardTick)
        {
            Mdl.IsGoingBackward = false;
            Mdl.AutoReplayTick.Restart();
            Tick();
        }

        bool BackwardTick = Entry.Inputs["BackTick"].IsDown && !needTick && !TL.IsTimelineAtBeginning();
        if(BackwardTick)
        {
            Mdl.IsGoingBackward = true;
            Tick();
        }

        bool needBreak = (IM.CurrentMode == InputManager.Mode.REPLAY) && (Mdl.Players.Count != 0) && !Mdl.IsRewinding && (AnyDirection || Break);
        if (needBreak)
        {
            // create a new player at current position
            var CurrentPlayer = Mdl.Players[Mdl.Players.Count - 1];
            var GO = AddPlayer(CurrentPlayer.transform.position);
            IM.Detach(CurrentPlayer.GetComponent<PlayerController>());
            CurrentPlayer.GetComponent<PlayerController>().IM = new InputManager();
            CurrentPlayer.GetComponent<PlayerController>().IM.CurrentMode = InputManager.Mode.REPLAY;
            IM.Attach(GO.GetComponent<PlayerController>());
            IM.CurrentMode = InputManager.Mode.RECORD;

            CurrentCamera?.GetComponent<PostFXRenderer>()?.StartAnimation(CurrentPlayer.transform.position, Mdl.Players.Count - 2, Mdl.Players.Count - 1);

            SwitchToCurrentPlayerTL();
        }

        if ( SwitchTL )
        {
            SwitchToNextTL();
        }

    }
    /// <summary>
    /// Return the current player in the list of players.
    /// It should correspond to the current player managed by the user.
    /// </summary>
    /// <returns></returns>
    public PlayerController GetCurrentPlayer()
    {
        if (Mdl.Players.Count == 0) return null;
        return Mdl.Players[Mdl.Players.Count - 1];
    }

    private void SwitchToNextTL()
    {
        UITimeline tl_ui        = levelUI_GO.GetComponent<UITimeline>();
        int next_loop_level = tl_ui.getDisplayedLoopLevel() + 1;
        if ( next_loop_level >= Mdl.Players.Count )
            next_loop_level = 0;


        CurrentCamera?.GetComponent<PostFXRenderer>()?.StartAnimation(Vector2.zero, Mathf.Max(0, tl_ui.getDisplayedLoopLevel()), Mathf.Max(0,next_loop_level));

        var selected_player_for_tl = Mdl.Players[next_loop_level];
        ITimeline tl_to_display = selected_player_for_tl.GetComponent<PlayerController>().Mdl.TL;
        tl_ui.trySwitchTimeline( tl_to_display);
    }

    private void SwitchToCurrentPlayerTL()
    {
        UITimeline tl_ui         = levelUI_GO.GetComponent<UITimeline>();
        PlayerController last_pc = GetCurrentPlayer();
        tl_ui.trySwitchTimeline( last_pc.Mdl.TL );
    }

    bool CanTick()
    {
        if (needTick) return false;
        // If any player is still animating we cannot move the timeline
        if (!(GetCurrentPlayer().GetComponent<Movable>().CanMove())) return false;
        foreach (PlayerController GO in Mdl.Players) {
            var Mover = gameObject.GetComponent<Movable>();
            if (!Mover) continue;
            if (Mover.Freeze) return false;
        }
        return true;
    }

    void RewindTimeline()
    {
        if (!CanTick()) return;
        
        if(TL.IsTimelineAtBeginning())
        {
            Tick();
            Mdl.IsRewinding = false;
            return;
        }

        Tick();
    }

    void UpdateTimelines()
    {
        var Reverse = Mdl.IsGoingBackward || Mdl.IsRewinding;
        if (Reverse) Mdl.Players.Reverse();
        foreach(PlayerController PC in Mdl.Players)
        {
            // NOTE toffa: for now all timelines are reversed at the same time
            PC.Mdl.TL.Reverse(Reverse);
            // NOTE toffa: if paying in reverse we have to apply before doing the increment, doing it in fixed update
            if (!Reverse) PC.Mdl.TL.Increment();
            PC.Mdl.TL.GetCursorValue()?.Apply(Mdl.IsGoingBackward);
        }
        if (Reverse) Mdl.Players.Reverse();

        ConnectorGraph cg = CurrentStageSelector.selected_stage.get_connector_graph();
        foreach( Wire w in cg.wires)
        {
            // NOTE toffa: for now all timelines are reversed at the same time
            w.TL.Reverse(Reverse);
            // NOTE toffa: if paying in reverse we have to apply before doing the increment, doing it in fixed update
            if (!Reverse) w.TL.Increment();
            w.TL.GetCursorValue()?.Apply(Mdl.IsGoingBackward);
        }

    }
    
    void FixedUpdateTimelines()
    {
        var Reverse = Mdl.IsGoingBackward || Mdl.IsRewinding;
        if (Reverse) Mdl.Players.Reverse();
        foreach(PlayerController PC in Mdl.Players)
        {
            PC.Mdl.TL.GetCursorValue()?.ApplyPhysics(Reverse);
            if (Reverse) PC.Mdl.TL.Increment(); 
        }
        if (Reverse) Mdl.Players.Reverse();

        ConnectorGraph cg = CurrentStageSelector.selected_stage.get_connector_graph();
        foreach( Wire w in cg.wires)
        {
            w.TL.GetCursorValue()?.Apply(Mdl.IsGoingBackward);
            if (Reverse) w.TL.Increment();
        }
    }

    void UpdateTimers()
    {
        Mdl.AutoReplayTick.Update(Time.deltaTime);
        Mdl.AutoRewindTick.Update(Time.deltaTime);
    }

    void Update()
    {
        UpdateTimers();

        if (Mdl.IsRewinding) RewindTimeline();
        else if (TL.IsTimelineAtEnd())
        {
            // NOTE toffa: we need to set the needtick variable to false because the last tick to cause
            // for the increment en hance the end of the timeline has set it to true and it can lead to some
            // weird behaviors. Even this is probably not a good solution and needTick should not be set
            // to true if trying to tick outside of the timeline boundaries.
            needTick = false;
            Mdl.IsRewinding = true;
            IM.CurrentMode = InputManager.Mode.REPLAY;
        }

        levelUI_GO?.GetComponent<UITimeline>()?.refresh(IM.CurrentMode);
    }

}
