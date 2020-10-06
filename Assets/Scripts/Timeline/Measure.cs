using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measure : MonoBehaviour
{
    public int measure_time;
    List<TimeUnit> times;
    private int measure_size;

    // Start is called before the first frame update
    public Measure()
    {
        measure_time = -1;
    }
    
    public void init( int iMeasure_size, int loop_level)
    {
        measure_size = iMeasure_size;
        times = new List<TimeUnit>( measure_size );
        int silence_times_threshold = iMeasure_size-loop_level;
        for (int i=0; i<measure_size; i++ )
        {
            bool is_silence = ( i <= silence_times_threshold );
            times.Add( new TimeUnit(is_silence) );
        }
    }

    public TimeUnit tryNext()
    {
        measure_time++;
        if ( measure_time >= measure_size )
        {
            return null;
        }
        TimeUnit nextTimeUnit = times[measure_time];
        return nextTimeUnit;
    }

}
