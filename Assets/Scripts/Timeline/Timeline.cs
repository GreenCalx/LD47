using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline 
{
    private const int N_MEASURES    = 5;
    private const int MEASURE_SIZE  = 5;

    // 0/F : silence
    // 1/T : play
    private BitArray    __timeLine;
    private int          last_tick;
    // offset is used to set timeline start to a given timeunit
    // used to avoid unwanted disabled time when getting a nested loop
    private int offset;
    public  bool        timeline_finished;

    // Loop Level
    // 0 : INITIAL LOOP
    // 1 : FIRST CHILD
    // ..
    // MEASURE_SIZE-1 : LAST CHILD with 1 move per measure
    // MEASURE_SIZE : THEORIC LAST CHILD  with 0 moves left
    public int loop_level;

    public Timeline( int iLoopLevel )
    {
        offset = 0;
        reset(iLoopLevel);
    }

    public Timeline( int iLoopLevel, int iLastTick )
    {
        offset      = iLastTick;
        last_tick   = iLastTick;
        reset(iLoopLevel, iLastTick);
    }

    public void reset( int iLoopLevel )
    {
        reset(iLoopLevel, -1);
    }

    public void reset( int iLoopLevel, int iLastTick )
    {
        last_tick = iLastTick;
        timeline_finished = false;
        loop_level = iLoopLevel;
        init();
    }

    public int getTickForCursor()
    {
        return last_tick+1;
    }

    public int getTickForTimeUnits( bool Saturate = false)
    {
        if (Constraints.ShowNextInputsOnTimelineOnReplay && Saturate)
            return 25;
        else
            return last_tick;
    }

    public void setCurrentTick(int iCurrentTick)
    {
        last_tick = iCurrentTick;
    }

    public void reset()
    {
        reset(loop_level);
    }

    public void init()
    {
        __timeLine = new BitArray( N_MEASURES * MEASURE_SIZE, true);
        for ( int i=0; i < __timeLine.Count; i+=MEASURE_SIZE)
        {
            if ( i < offset )
                continue;
            for ( int j=loop_level; j > 0; j--)
            {
                int k = i;                  // place cursor
                k += (MEASURE_SIZE - j );   // apply offset
                __timeLine[k] = false;
            }//! for j
        }//! for i
    }//! init

    public BitArray getTimeline()
    {
        return __timeLine;
    }

    public bool isTimelineOver()
    {
        // NOTE (Toffa) : We need to look at last_tick +1 because last_tick would be 24 as the first is 0 if we want to check for 25 steps
        timeline_finished = ( last_tick+1 >= N_MEASURES*MEASURE_SIZE );
        return timeline_finished;
    }

    public bool getAt( int iTimeIndex )
    {
        if ( (iTimeIndex<__timeLine.Count) && (iTimeIndex>=0) )
            return __timeLine[iTimeIndex];
        return false; // else OoB we do nothing
    }

    public Timeline getNestedTimeline()
    {
        return new Timeline( loop_level+1, last_tick);
    }

}
