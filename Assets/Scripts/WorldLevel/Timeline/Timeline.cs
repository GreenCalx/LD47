using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimelineValue
{
    void Apply(bool Reversed = false);
    void ApplyPhysics(bool Reversed = false);
}

public interface ITimeline
{
    void SetCursor(int CursorIdx);
    int  GetCursorIndex();
    ITimelineValue GetCursorValue();
    ITimelineValue GetCursorValue(int CursorIdx);
    void Increment();
    void Decrement();
    bool IsTimelineAtEnd();
    bool IsTimelineAtBeginning();
    bool IsCursorValuable(int CursorIndexToCheck);
    bool IsCursorValuable();
}

public class Timeline<T> : ITimeline, ITickObserver where T : new()
{
    protected int _Size = -1;
    protected int _Cursor = -1;

    protected BitArray _TimelineValuable;
    protected T[] _CursorValues;

    public bool IsReversed = false;

    protected ITickController Ctl = null;

    void ITickObserver.OnTick()
    {
        GetCursorValue().ApplyPhysics();
    }

    void ITickObserver.SetControler(ITickController Ctl)
    {
        this.Ctl = Ctl;
    }

    virtual protected void Init() {
        _TimelineValuable = new BitArray(_Size, true);
        _CursorValues = new T[_Size];
        for (int i = 0; i < _Size; ++i) _CursorValues[i] = new T();
    }

    public Timeline()
    {

    }

    public Timeline(int Size)
    {
        _Size = Size;
        Init();
    }

    public Timeline(int Size, int Cursor)
    {
        _Size = Size;
        _Cursor = Cursor;
        Init();
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

    public ITimelineValue GetCursorValue()
    {
        if (!CheckIndexIsValid(_Cursor)) return null;
        return _CursorValues[_Cursor] as ITimelineValue;
    }

    public ITimelineValue GetCursorValue( int CursorIdx )
    {
        if (!CheckIndexIsValid(CursorIdx)) return null;
        return _CursorValues[CursorIdx] as ITimelineValue;
    }


    public void Reverse(bool Value)
    {
        IsReversed = Value;
    }

    public void Increment()
    {
        // Do not check boundaries value here as we want it to go past it
       _Cursor = IsReversed ? _Cursor-1 : _Cursor+1;
    }

    public void Decrement()
    {
        // Do not check boundaries value here as we want it to go past it
       _Cursor = IsReversed ? _Cursor+1 : _Cursor-1;
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

public class PlayerTimeline : Timeline<PlayerTimelineValue>
{
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


    public PlayerTimeline(int LoopLevel) : base(N_MEASURES * MEASURE_SIZE)
    {
    }

    public PlayerTimeline(int LoopLevel, int CursorIdx) : base(N_MEASURES * MEASURE_SIZE, CursorIdx)
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

    public void SetPlayer(PlayerController PC)
    {
        foreach(PlayerTimelineValue V in _CursorValues)
        {
            V._Player = PC;
        }
    }


    // !! autoflag this TL as a previous TL ( used by UI )
    public PlayerTimeline GetNestedTimeline()
    {
        var Result = new PlayerTimeline( _LoopLevel+1, _Cursor);

        System.Array.Copy(this._CursorValues, Result._CursorValues, _Cursor);
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
