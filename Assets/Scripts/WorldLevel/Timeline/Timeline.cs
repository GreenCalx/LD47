using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimeline
{
    void SetCursor(int CursorIdx);
    int  GetCursorIndex();
    ITickController GetCursorValue();
    ITickController GetCursorValue(int CursorIdx);
    void Increment();
    void Decrement();
    bool IsTimelineAtEnd();
    bool IsTimelineAtBeginning();
    bool IsCursorValuable(int CursorIndexToCheck);
    bool IsCursorValuable();
}

public class TimelineValue : TickClock {
    // todo toffa : make this betta PLASE
    WorldManager WM;
    public TimelineValue() : base()
    {
        IsSyncWithFixedUpdate = false;
    }

    void Update()
    {
        WM = GameObject.Find("GameLoop")?.GetComponent<WorldManager>();
    }

    public override bool FixedTick()
    {
        Update();
        if (WM)
        {
            if (WM.TL.Mode == InputManager.Mode.RECORD)
            {
                // We need to really execute the physic! not only do the update position
                // need to copy the list as it can be modified during physic play
                // anyway we redo everything
                foreach (var Obs in _Listeners)
                {
                    if ((Obs as FixedTickValue) == null) Debug.Log("This is weird");

                    var FixedObs = (Obs as FixedTickValue);
                    FixedObs.RemoveAllObservers();
                }
            }
        }
        return base.FixedTick();
    }
}

public class GeneralTimeline : Timeline<TimelineValue> {
    public new static GeneralTimeline Create(int Size)
    {
        var Result = new GeneralTimeline(Size);
        Result.Init();
        return Result;
    }
    protected GeneralTimeline(int Size) : base(Size) { } 

    public override void OnFixedTick()
    {
        base.OnFixedTick();
        if (CanFixedTick()) GetCursorValue()?.FixedTick();
    }

    public override void OnFixedBackTick()
    {
        var CurrentObs = GetCursorValue()?.GetObservers();
        CurrentObs?.Reverse();
        if (CanFixedTick()) GetCursorValue()?.FixedBackTick();
        base.OnFixedBackTick();
        CurrentObs?.Reverse();
    }
}

public class Timeline<T> : TickBasedAndClock, ITimeline where T : ITickController, new() {
    protected int _Size = -1;
    protected int _Cursor = -1;

    protected BitArray _TimelineValuable;
    protected T[] _CursorValues;

    public bool IsReversed = false;

    public InputManager.Mode Mode = InputManager.Mode.DEACTIVATED;

    protected void RemoveAllObserversOnValue() {
        GetCursorValue()?.RemoveAllObservers();
    }

    public override void OnTick()
    {
        if (CanTick())
        {
            Increment();
        }
        base.OnTick();
    }

    public override void OnBackTick()
    {
        base.OnBackTick();
    }

    public override void OnFixedBackTick()
    {
        base.OnFixedBackTick();
        if (CanFixedTick())
        {
            if (Mode == InputManager.Mode.RECORD) RemoveAllObserversOnValue();
            Decrement();
        }
    }

    virtual protected void Init() {
        _TimelineValuable = new BitArray(_Size, true);
        _CursorValues = new T[_Size];
        for (int i = 0; i < _Size; ++i) _CursorValues[i] = new T();
    }

    public static Timeline<T> Create()
    {
        var Result = new Timeline<T>();
        Result.Init();
        return Result;
    }
    protected Timeline() { }
    public static Timeline<T> Create(int Size)
    {
        var Result = new Timeline<T>(Size);
        Result.Init();
        return Result;
    }
    public static Timeline<T> Create(int Size, int Cursor)
    {
        var Result = new Timeline<T>(Size, Cursor);
        Result.Init();
        return Result;
    }

    protected Timeline(int Size)
    {
        _Size = Size;
    }

    protected Timeline(int Size, int Cursor)
    {
        _Size = Size;
        _Cursor = Cursor;
    }

    public void SetCursor(int CursorIdx)
    {
        if (!CheckIndexIsValid(CursorIdx)) return;
        _Cursor = CursorIdx;
    }

    public int GetCursorIndex()
    {
        if (!CheckIndexIsValid(_Cursor)) return -1;
        return _Cursor;
    }

    public ITickController GetCursorValue()
    {
        if (!CheckIndexIsValid(_Cursor)) return null;
        return _CursorValues[_Cursor] ;
    }

    public ITickController GetCursorValue( int CursorIdx )
    {
        if (!CheckIndexIsValid(CursorIdx)) return null;
        return _CursorValues[CursorIdx] ;
    }

    public void Reverse(bool Value)
    {
        IsReversed = Value;
    }

    public void Increment()
    {
        // Do not check boundaries value here as we want it to go past it
        //_Cursor = IsReversed ? _Cursor-1 : _Cursor+1;
        ++_Cursor;
    }

    public void Decrement()
    {
        // Do not check boundaries value here as we want it to go past it
        //_Cursor = IsReversed ? _Cursor+1 : _Cursor-1;
        --_Cursor;
    }

    public bool IsTimelineAtEnd()
    {
        return _Cursor >= _Size;
    }

    public bool IsTimelineAtBeginning()
    {
        return _Cursor == -1;
    }

    private bool CheckIndexIsValid(int CursorIndexToCheck)
    {
        return (CursorIndexToCheck < _Size) && (CursorIndexToCheck >= 0);
    }

    public bool IsCursorValuable( int CursorIndexToCheck )
    {
        if (!CheckIndexIsValid(CursorIndexToCheck)) return false;
        return _TimelineValuable[CursorIndexToCheck];
    }

    public bool IsCursorValuable()
    {
        return IsCursorValuable(GetCursorIndex());
    }
}

public class PlayerTimelineValue : TimelineValue
{
    public PlayerController.Direction _Value = PlayerController.Direction.NONE;
    public PlayerController.Direction GetValue() { return _Value; }
}


public class PlayerTimeline : Timeline<PlayerTimelineValue> {
    private const int N_MEASURES    = 5;
    private const int MEASURE_SIZE  = 5;

    // Loop Level
    // 0 : INITIAL LOOP
    // 1 : FIRST CHILD
    // ..
    // MEASURE_SIZE-1 : LAST CHILD with 1 move per measure
    // MEASURE_SIZE : THEORIC LAST CHILD  with 0 moves left
    private int _LoopLevel = 0;
    // offset is used to set timeline start to a given timeunit
    // used to avoid unwanted disabled time when getting a nested loop
    private int _Offset = 0;


    private bool _IsPrevious = false;
    public bool IsPrevious()
    {
        return _IsPrevious;
    }

    public static PlayerTimeline Create(int LoopLevel)
    {
        var Result = new PlayerTimeline(LoopLevel);
        Result.Init();
        return Result;
    }
    protected PlayerTimeline(int LoopLevel) : base(N_MEASURES * MEASURE_SIZE)
    {
        _LoopLevel = LoopLevel;
    }

    public static PlayerTimeline Create(int LoopLevel, int CursorIdx)
    {
        var Result = new PlayerTimeline(LoopLevel, CursorIdx);
        Result.Init();
        return Result;
    }
    protected PlayerTimeline(int LoopLevel, int CursorIdx) : base(N_MEASURES * MEASURE_SIZE, CursorIdx)
    {
        _LoopLevel = LoopLevel;
    }

    protected override void Init()
    {
        base.Init();
        if (_LoopLevel == 0) return;
        for ( int i=_Offset; i < _TimelineValuable.Count; ++i)
        {
            //remap to 1 - MEASURE_SIZE to avoid 0
            int idx = (i - (((int)(i / MEASURE_SIZE)) * MEASURE_SIZE)) +1;
            for(int j = MEASURE_SIZE - (_LoopLevel-1); j <= MEASURE_SIZE; ++j)
            {
                if( (idx)%j==0) { _TimelineValuable[i] = false; }
            }
        }//! for i
    }

    // !! autoflag this TL as a previous TL ( used by UI )
    public PlayerTimeline GetNestedTimeline()
    {
        var Result = Create( _LoopLevel+1, _Cursor);

        if (IsCursorValuable())
        {
            System.Array.Copy(this._CursorValues, Result._CursorValues, _Cursor + 1);
            //System.Array.Copy(this._TimelineValuable, Result._TimelineValuable, _Cursor + 1);
            for (int i = 0; i <= _Cursor; ++i) Result._TimelineValuable[i] = this._TimelineValuable[i];
        }
        // Notify this TL as a 'previous TL' as it is current gameplay.
        // If we want to mark previous flag with more flexibility,
        // set the flag explicitely with logic in WM.
        _IsPrevious = true;

        return Result; 
    }

    // Toffa : Really needed still?
    public int getTickForTimeUnits( bool Saturate = false)
    {
        if (Constants.ShowNextInputsOnTimelineOnReplay && Saturate)
            return 25;
        else
            return _Cursor+1;
    }

    public int GetLevel()
    {
        return _LoopLevel;
    }
}
