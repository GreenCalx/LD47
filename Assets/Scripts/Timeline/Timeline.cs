using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline 
{
    private const int N_MEASURES    = 5;
    private const int MEASURE_SIZE  = 5;
    private List<Measure> __measures;
    public int current_measure_index;
    public bool timeline_finished;
    public Measure active_measure;

    // Loop Level
    // 0 : INITIAL LOOP
    // 1 : FIRST CHILD
    // ..
    // MEASURE_SIZE : LAST CHILD ( no move left in the timeline anyway )
    public int loop_level;

    public Timeline( int iLoopLevel )
    {
        loop_level = iLoopLevel;
    }

    void init()
    {
        timeline_finished            = false;
        current_measure_index        = 0;
        __measures = new List<Measure>(N_MEASURES);
        for (int i=0; i<N_MEASURES; i++ )
        {
            Measure new_measure = new Measure();
            new_measure.init( MEASURE_SIZE, loop_level);
            __measures.Add( new_measure );
        }

    }

    public TimeUnit tryNext()
    {
        TimeUnit nextTimeUnit = active_measure.tryNext();
        if ( nextTimeUnit == null )
        {
            if (!tryNextMeasure())
                return null;
            return active_measure.tryNext();
        }
        return nextTimeUnit;
    }

    private bool tryNextMeasure()
    {
        current_measure_index++;
        if ( current_measure_index >= N_MEASURES )
        {
            timeline_finished = true;
            return false;
        }
        active_measure = __measures[current_measure_index];
        return true;
    }
}
