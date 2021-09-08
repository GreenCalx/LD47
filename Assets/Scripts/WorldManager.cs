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
    bool CanTick();
    bool CanFixedTick();
    bool NeedFixedTick();
    bool Tick();
    bool FixedTick();
    bool BackTick();
    bool FixedBackTick();
    void AddObserver(ITickObserver Obs);
    void AddObserver(int Index, ITickObserver Obs);
    void RemoveObserver(ITickObserver Obs);
    void RemoveAllObservers();
    List<ITickObserver> GetObservers();
}

public interface ITickObserver
{
    void OnBackTick();
    void OnFixedBackTick();
    void OnTick();
    void OnFixedTick();
    void SetControler(ITickController Controler);
}

public class TickClock : ITickController
{
    public List<ITickObserver> _Listeners = new List<ITickObserver>();
    public List<ITickObserver> GetObservers()
    {
        return _Listeners;
    }
    // this bool is there to be able to avoid multiple tick without their
    // fixedupdate counterpart if needed
    // as of now this is the default behavior but might not be necessary
    public bool IsSyncWithFixedUpdate = true;
    private bool _NeedFixedTick = false;
    private bool _SyncMutex = false;
    public void RemoveAllObservers()
    {
        for ( int i = _Listeners.Count -1; i >= 0; --i)
        {
            if ((_Listeners[i] as BackTickSentinelValue) == null)
            {
                _Listeners.RemoveAt(i);
            }
            else
            {
                break;
            }
        }
    }
    public bool NeedFixedTick()
    {
        return _NeedFixedTick;
    }
    public virtual bool CanTick()
    {
        if (IsSyncWithFixedUpdate && _SyncMutex) return false;
        else return true;
    }
    public virtual bool CanFixedTick()
    {
        return true;
    }
    public void AddObserver(int Index, ITickObserver Listener)
    {
        // TODO(toffa): add all necessary checks
        if (_Listeners.Find((ITickObserver obj) => Listener == obj) != null) return;
        if (Index >= _Listeners.Count - 1)
        {
            AddObserver(Listener);
            return;
        }
        Listener.SetControler(this);
        _Listeners.Insert(Index, Listener);
    }
    public void AddObserver(ITickObserver Listener)
    {
        // TODO(toffa): add all necessary checks
        if (_Listeners.Find((ITickObserver obj) => Listener == obj) != null) return;
        Listener.SetControler(this);
        _Listeners.Add(Listener);
    }
    public void AddObserver(ITickObserver Index, ITickObserver Listener)
    {
        AddObserver(_Listeners.FindIndex((ITickObserver obj) => obj == Index), Listener);
    }
    public void RemoveObserver(ITickObserver Obs)
    {
        _Listeners.Remove(Obs);
    }

    public virtual bool Tick()
    {
        if (!CanTick()) return false;

        _SyncMutex = true;
        _NeedFixedTick = true;

        foreach (var Obs in _Listeners) Obs.OnTick();

        return true;
    }
    public virtual bool FixedTick()
    {
        if (!CanFixedTick()) return false;
        _SyncMutex = false;
        _NeedFixedTick = false;
        for (int i = 0; i < _Listeners.Count; ++i)
        {
            var Obs = _Listeners[i];
            Obs.OnFixedTick();
            Physics2D.SyncTransforms();
        }
        return true;
    }
    public virtual bool BackTick()
    {
        if (!CanTick()) return false;

        _SyncMutex = true;
        _NeedFixedTick = true;
        foreach (var Obs in _Listeners) Obs.OnBackTick();
        return true;
    }
    public virtual bool FixedBackTick()
    {
        if (!CanFixedTick()) return false;

        _SyncMutex = false;
        _NeedFixedTick = false;
        foreach (var Obs in _Listeners)
        {
            Obs.OnFixedBackTick();
            Physics2D.SyncTransforms();
        }
        return true;
    }
}

public class TickClockBehaviour : MonoBehaviour, ITickController {
    public List<ITickObserver> _Listeners = new List<ITickObserver>();
    public List<ITickObserver> GetObservers()
    {
        return _Listeners;
    }
    // this bool is there to be able to avoid multiple tick without their
    // fixedupdate counterpart if needed
    // as of now this is the default behavior but might not be necessary
    public bool IsSyncWithFixedUpdate = true;
    private bool _NeedFixedTick = false;
    private bool _SyncMutex = false;

    public void RemoveAllObservers()
    {
        _Listeners.Clear();
    }

    public bool NeedFixedTick()
    {
        return _NeedFixedTick;
    }
    public virtual bool CanTick()
    {
        if (IsSyncWithFixedUpdate && _SyncMutex) return false;
        else return true;
    }

    public virtual bool CanFixedTick()
    {
        return true;
    }

    public void AddObserver(int Index, ITickObserver Listener) {
        // TODO(toffa): add all necessary checks
        if (_Listeners.Find((ITickObserver obj) => Listener == obj) != null) return;
        if (Index >= _Listeners.Count - 1)
        {
            AddObserver(Listener);
            return;
        }
        Listener.SetControler(this);
        _Listeners.Insert(Index, Listener);
    }
    public void AddObserver(ITickObserver Listener) {
        // TODO(toffa): add all necessary checks
        if (_Listeners.Find((ITickObserver obj) => Listener == obj) != null) return;
        Listener.SetControler(this);
        _Listeners.Add(Listener);
    }
    public void AddObserver(ITickObserver Index, ITickObserver Listener)
    {
       AddObserver( _Listeners.FindIndex((ITickObserver obj) => obj == Index), Listener);
    }
    public void RemoveObserver(ITickObserver Obs)
    {
        _Listeners.Remove(Obs);
    }

    public virtual bool Tick() {
        if (!CanTick()) return false;
        _SyncMutex = true;
        _NeedFixedTick = true;

        foreach (var Obs in _Listeners) Obs.OnTick();
        return true;
    }

    public virtual bool FixedTick() {
        if (!CanFixedTick()) return false;
        _SyncMutex = false;
        _NeedFixedTick = false;
        foreach (var Obs in _Listeners)
        {
            Obs.OnFixedTick();
            Physics2D.SyncTransforms();
        }
         return true;
    }

    public virtual bool BackTick() {
        if (!CanTick()) return false;

        _SyncMutex = true;
        _NeedFixedTick = true;
        foreach (var Obs in _Listeners) Obs.OnBackTick();
        return true;
    }

    public virtual bool FixedBackTick() {
        if (!CanFixedTick()) return false;

        _SyncMutex = false;
        _NeedFixedTick = false;
        foreach (var Obs in _Listeners)
        {
            Obs.OnFixedBackTick();
            Physics2D.SyncTransforms();
        }
        return true;
    }

}

public class TickBasedBehaviour : MonoBehaviour, ITickObserver { 
    protected ITickController _Controler;
    public void SetControler(ITickController Controler)
    {
        _Controler = Controler;
    }

    public virtual void OnTick() { }
    public virtual void OnFixedTick() { }
    public virtual void OnBackTick() { }
    public virtual void OnFixedBackTick() { }
}


public class TickBased : ITickObserver { 
    protected ITickController _Controler;
    public void SetControler(ITickController Controler)
    {
        _Controler = Controler;
    }

    public virtual void OnTick() { }
    public virtual void OnFixedTick() { }
    public virtual void OnBackTick() { }
    public virtual void OnFixedBackTick() { }
}

public class TickBasedAndClock : TickClock, ITickObserver
{
    protected ITickController _Controler;
    public void SetControler(ITickController Controler)
    {
        _Controler = Controler;
    }

    public virtual void OnTick() { Tick(); }
    public virtual void OnFixedTick() { FixedTick(); }
    public virtual void OnBackTick() { BackTick(); }
    public virtual void OnFixedBackTick() { FixedBackTick(); }
}

public class BackTickSentinelValue : FixedTickValue{
}

public class WorldManager : TickClockBehaviour, IControllable, ISavable {
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

    // NOTE toffa : this is the general timeline that is used to dispatch to every other observer the tick
    // basically the only obs of the WorldTick. On each value there zill be a list of events that happened during the tick
    // and this will be used to rewind
    public GeneralTimeline TL = GeneralTimeline.Create(25);
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

        NewPlayer.Mdl.TL = (GetCurrentPlayer() != null) ? GetCurrentPlayer()?.Mdl.TL?.GetNestedTimeline() : PlayerTimeline.Create(0);
        // NOTE toffa : the player timeline is obsed by the player. When the playertimeline receive a tick it will dispatch
        // to the player controller if we are in record mode to make the player move. Each event happening will then be
        // recorded in this.TL
        NewPlayer.Mdl.TL.AddObserver(NewPlayer);

        Mdl.Players.Add(NewPlayer);
        TL.AddObserver(NewPlayer);


        // Add Recording of this player value as moveValue in our timeline
        for (int i = 0; i < 25; ++i)
        {
            if (i <= TL.GetCursorIndex())
            {
                var TLValue = TL.GetCursorValue(i);
                foreach(var V in TLValue.GetObservers())
                {
                    var TransientObs = (V as PlayerMoveValue)?.GetObservers();
                    for (int j = 0; j < TransientObs?.Count; ++j)
                    {
                        if ( (TransientObs[j] as MoveValue)._go.gameObject == PreviousPlayer.gameObject)
                        {
                            (V as PlayerMoveValue).AddObserver(new MoveValue(NewPlayer.GetComponent<Movable>(), (TransientObs[j] as MoveValue)._dir));
                        }
                    }
                }

                var PreviousPlayerMove = (PreviousPlayer.Mdl.TL.GetCursorValue(i) as PlayerTimelineValue);
                var NewPlayerMove = new PlayerMoveValue(NewPlayer.GetComponent<Movable>(), PreviousPlayerMove.GetValue());

                TLValue?.AddObserver(NewPlayerMove);
            }
            else
                TL.GetCursorValue(i)?.AddObserver(new BackTickSentinelValue());
        }
        // IMPORTANT toffa this has to be at the end of the event list, it is used to avoid to ba able to tick back further than this index in record mode
        TL.GetCursorValue(TL.GetCursorIndex())?.AddObserver(new BreakValue());
        TL.GetCursorValue(TL.GetCursorIndex() + 1)?.AddObserver(new CollisionManagerValue(this, NewPlayer));

        return NewPlayer;
    }
    // Start is called before the first frame update
    void Start()
    {
        AddObserver(TL);

        var GO = AddPlayer( StartTile.transform.position );
        var PC = GO.GetComponent<PlayerController>();
        IM.Attach(PC);
        IM.Attach(this);

        TL.Mode = IM.CurrentMode;

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
    
    void FixedUpdate()
    {
        if (NeedFixedTick())
        {
            if (Mdl.IsGoingBackward) FixedBackTick();
            else FixedTick();
        }
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

        var Obs = TL.GetCursorValue()?.GetObservers();
        bool T = Obs != null ? ((Obs.Count != 0) && (Obs[Obs.Count - 1] as BreakValue == null)) : false;

        bool BackwardTick = Entry.Inputs["BackTick"].IsDown && !TL.IsTimelineAtBeginning();
        if(BackwardTick)
        {
            Mdl.IsGoingBackward = true;
            BackTick();
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
            TL.Mode = InputManager.Mode.RECORD;

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

    public override bool CanTick()
    {
        if (!base.CanTick()) return false;
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
            Mdl.IsGoingBackward = false;
            Mdl.IsRewinding = false;
            return;
        }

        BackTick();
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
            Mdl.IsRewinding = true;
            Mdl.IsGoingBackward = true;
            IM.CurrentMode = InputManager.Mode.REPLAY;
            TL.Mode = InputManager.Mode.REPLAY;
        }

        levelUI_GO?.GetComponent<UITimeline>()?.refresh(IM.CurrentMode);
    }

}
