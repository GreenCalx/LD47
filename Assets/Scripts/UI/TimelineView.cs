using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineView : MonoBehaviour
{
    public class TimelineComparer : IEqualityComparer<Timeline>
    {
        public bool Equals(Timeline x, Timeline y)
        {
            return x.loop_level == y.loop_level;
        }
    
        public int GetHashCode(Timeline obj)
        {
            return obj.loop_level.GetHashCode();
        }
    }

    private Timeline __displayedTimeline;
    private HashSet<Timeline> __lTL;

    // Start is called before the first frame update
    void Start()
    {
        __lTL = new HashSet<Timeline>( new TimelineComparer() );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Timeline getDisplayedTimeline()
    {
        return __displayedTimeline;
    }


    public void updateTimeline(Timeline iTL)
    {
        if ( !__lTL.Contains(iTL) )
            __lTL.Add(iTL);
        else 
        {
            __lTL.Remove(iTL);
            __lTL.Add(iTL);
        }
    }
}
