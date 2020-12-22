using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline 
{
    // for rewind just record everything
    // This is a shitty design as it is not easy to print the current timeline value while rewinding for instance
    // the problem is that is can be hard to have something work well in reverse wioth physics and shit
    public class Recorder
    {
        public void Record(int Tick, GameObject Go, PlayerController.Direction D)
        {
            // We are using a while loop because it is possible to 'leap' a tick
            // and have to add 2 ticks instead of one
            while (GameObjects.Count - 1 < Tick)
            {
                GameObjects.Add(new List<GameObject>());
                Directions.Add(new List<PlayerController.Direction>());
            }

            GameObjects[Tick].Add(Go);
            Directions[Tick].Add(D);
        }

        public int Tick()
        {
            var TickGameObjects = GameObjects[GameObjects.Count - 1];
            var TickDirections = Directions[Directions.Count - 1];

            TickGameObjects.Reverse();
            TickDirections.Reverse();

            for (int i = 0; i < TickGameObjects.Count; ++i)
            {
                var GO = TickGameObjects[i];
                var D = TickDirections[i];
                var Mover = GO.GetComponent<Movable>();
                if (Mover) Mover.Move(PlayerController.InverseDirection(D), false);
            }

            GameObjects.RemoveAt(GameObjects.Count - 1);
            Directions.RemoveAt(Directions.Count - 1);

            return GameObjects.Count;
        }

        public void Tick(int CurrentTick)
        {
            var TickGameObjects = GameObjects[CurrentTick];
            var TickDirections = Directions[CurrentTick];

            for (int i = 0; i < TickGameObjects.Count; ++i)
            {
                var GO = TickGameObjects[i];
                var D = TickDirections[i];
                var Mover = GO.GetComponent<Movable>();
                if (Mover) Mover.Move(PlayerController.InverseDirection(D), false);
            }
        }

        public void DeleteRecord(int Tick)
        {
            // for backward implementation we need to remove the last recoirded tick
            if (GameObjects.Count -1 >= Tick)
            {
                GameObjects[Tick] = new List<GameObject>();
                Directions[Tick] = new List<PlayerController.Direction>();
            }

        }

        public bool IsEmpty()
        {
            return GameObjects.Count == 0;
        }

        public List<List<GameObject>> GameObjects = new List<List<GameObject>>();
        public List<List<PlayerController.Direction>> Directions = new List<List<PlayerController.Direction>>();
    }


    public Recorder Rewind = new Recorder();

    private const int N_MEASURES    = 5;
    private const int MEASURE_SIZE  = 5;

    // 0/F : silence
    // 1/T : play
    public PlayerController.Direction[] Events;
    private BitArray    __timeLine;
    public int          last_tick;
    // offset is used to set timeline start to a given timeunit
    // used to avoid unwanted disabled time when getting a nested loop
    public int offset;
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
        Events = new PlayerController.Direction[N_MEASURES * MEASURE_SIZE];
        reset(iLoopLevel);
    }

    public Timeline( int iLoopLevel, int iLastTick )
    {
        offset      = iLastTick;
        last_tick   = iLastTick;
        Events = new PlayerController.Direction[N_MEASURES * MEASURE_SIZE];
        reset(iLoopLevel, iLastTick);
    }

    public void reset( int iLoopLevel )
    {
        reset(iLoopLevel, -1);
    }

    public PlayerController.Direction GetCurrent()
    {
        return Events[last_tick];
    }

    public void SetCurrent(PlayerController.Direction Dir)
    {
        Events[last_tick] = Dir;
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
        return last_tick;
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
        if (loop_level == 0) return;
        for ( int i=offset; i < __timeLine.Count; ++i)
        {
            //remap to 1 - MEASURE_SIZE to avoid 0
            int idx = (i - (((int)(i / MEASURE_SIZE)) * MEASURE_SIZE)) +1;
            for(int j = MEASURE_SIZE - (loop_level-1); j <= MEASURE_SIZE; ++j)
            {
                if( (idx)%j==0)
                {
                    __timeLine[i] = false;
                }
            }
        }//! for i
    }//! init

    public BitArray getTimeline()
    {
        return __timeLine;
    }

    public bool isTimelineOver()
    {
        timeline_finished = ( last_tick >= N_MEASURES*MEASURE_SIZE );
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
        var Result = new Timeline( loop_level+1, last_tick);
        System.Array.Copy(this.Events, Result.Events, last_tick);
        Result.Rewind = (Rewind);
        return Result; 
    }

}
