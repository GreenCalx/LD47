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
        reset(iLoopLevel);
    }

    public void reset( int iLoopLevel )
    {
        loop_level = iLoopLevel;
        init();
    }

    public void init()
    {
        __timeLine = new BitArray( N_MEASURES * MEASURE_SIZE, true);
        for ( int i=0; i < __timeLine.Count; i+=MEASURE_SIZE)
        {
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

    public bool getAt( int iTimeIndex )
    {
        if ( (i<__timeLine.Count) && (i>=0) )
            return __timeLine[i];
        return false; // else OoB we do nothing
    }


}
