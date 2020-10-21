using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POI : MonoBehaviour
{

    public enum DIRECTIONS
    {
      NONE  ,
      UP    ,
      LEFT  ,
      DOWN  ,
      RIGHT ,
      UPLEFT,
      UPRIGHT,
      DOWNLEFT,
      DOWNRIGHT
    }

    public List<Tuple<POI,POI.DIRECTIONS>> neighbors;
    // Start is called before the first frame update
    void Start()
    {
        neighbors = new List<Tuple<POI,POI.DIRECTIONS>>(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void connectTo( POI otherPOI, POI.DIRECTIONS iDirection )
    {
      if ( neighbors.)
      neighbors.Add( otherPOI, iDirection);
      otherPOI.oneWayConnectTo( this, revertDirection(iDirection));
    }

    public void oneWayConnectTo( POI otherPOI, POI.DIRECTIONS iDirection)
    {
      neighbors.Add( otherPOI, iDirection);
    }

    private POI.DIRECTIONS revertDirection( POI.DIRECTIONS iDirection)
    {
      if ( iDirection == POI.DIRECTIONS.UP ) return POI.DIRECTIONS.DOWN;
      else if ( iDirection == POI.DIRECTIONS.DOWN ) return POI.DIRECTIONS.UP;
      else if ( iDirection == POI.DIRECTIONS.LEFT ) return POI.DIRECTIONS.RIGHT;
      else if ( iDirection == POI.DIRECTIONS.RIGHT ) return POI.DIRECTIONS.LEFT;
      else if ( iDirection == POI.DIRECTIONS.UPLEFT ) return POI.DIRECTIONS.DOWNRIGHT;
      else if ( iDirection == POI.DIRECTIONS.DOWNRIGHT ) return POI.DIRECTIONS.UPLEFT;
      else if ( iDirection == POI.DIRECTIONS.UPRIGHT ) return POI.DIRECTIONS.DOWNLEFT;
      else if ( iDirection == POI.DIRECTIONS.DOWNLEFT ) return POI.DIRECTIONS.UPRIGHT;
      else
        return POI.DIRECTIONS.NONE;
    }
}
