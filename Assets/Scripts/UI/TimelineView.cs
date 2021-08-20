using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimelineView : MonoBehaviour
{
    public class TimelineComparer : IEqualityComparer<ITimeline>
    {
        public bool Equals(ITimeline x, ITimeline y)
        {
            
            return (x as PlayerTimeline).GetLevel() == (y as PlayerTimeline).GetLevel();
        }
    
        public int GetHashCode(ITimeline obj)
        {
            return (obj as PlayerTimeline).GetLevel().GetHashCode();
        }
    }

    private ITimeline __displayedTimeline;
    private HashSet<ITimeline> __lTL;

    // Start is called before the first frame update
    void Start()
    {
        __lTL = new HashSet<ITimeline>( new TimelineComparer() );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ITimeline getDisplayedTimeline()
    {
        return __displayedTimeline;
    }


    public void updateTimeline(ITimeline iTL)
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
