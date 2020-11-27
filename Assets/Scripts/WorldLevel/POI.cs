using System.Collections;
using System.Collections.Generic;
using System;
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

    // todo refacto to bit op
    public static POI.DIRECTIONS getDirection( int x, int y)
    {
      if ((x==0)&&(y==0))
        return DIRECTIONS.NONE;
      if ( x == -1 )
      {
        if ( y == 1 )
          return DIRECTIONS.DOWNLEFT;
        else if ( y == 0 )
          return DIRECTIONS.LEFT;
        else
          return DIRECTIONS.UPLEFT;
      } else if ( y == -1 )
      {
        if ( x == 1 )
          return DIRECTIONS.UPRIGHT;
        if (x == 0)
          return DIRECTIONS.UP;
        else
          return DIRECTIONS.UPLEFT;
      } else if ( x == 1 ) {
        if ( y == 1 )
          return DIRECTIONS.DOWNRIGHT;
        else if ( y == 0 )
          return DIRECTIONS.RIGHT;
        else
          return DIRECTIONS.UPRIGHT;
      } else if ( y == 1 ) {
        if ( x == 1 )
          return DIRECTIONS.DOWNRIGHT;
        if (x == 0)
          return DIRECTIONS.DOWN;
        else
          return DIRECTIONS.DOWNLEFT;
      } else {
        return DIRECTIONS.NONE;
      }
    }

    public List<Tuple<POI,POI.DIRECTIONS>> neighbors;
    // Start is called before the first frame update
    void Start()
    {
      Debug.Log("unexpected init");
      init();
    }

    protected void init()
    {
        neighbors = new List<Tuple<POI, POI.DIRECTIONS>> { };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public POI tryNeighbor(POI.DIRECTIONS iDirection)
    {
      foreach( Tuple<POI,POI.DIRECTIONS> poi_to_dir in neighbors)
      {
        if (poi_to_dir.Item2 == iDirection)
          return poi_to_dir.Item1;
      }
      return null;
    }

    public bool isAlreadyConnected(POI otherPOI)
    {
      foreach( Tuple<POI,POI.DIRECTIONS> tuple_poi in neighbors )
      {
        if (tuple_poi.Item1 == otherPOI)
          return true;
      }
      return false;
    }

    public bool connectTo( POI otherPOI, POI.DIRECTIONS iDirection )
    {
      if ( isAlreadyConnected(otherPOI) )
        return false;
      neighbors.Add( Tuple.Create(otherPOI, iDirection) );
      otherPOI.oneWayConnectTo( this, revertDirection(iDirection));
      return true;
    }

    public void oneWayConnectTo( POI otherPOI, POI.DIRECTIONS iDirection)
    {
      neighbors.Add( Tuple.Create(otherPOI, iDirection) );
    }

    public void oneWayRevertConnectTo( POI otherPOI, POI.DIRECTIONS iDirection)
    {
      neighbors.Add( Tuple.Create(otherPOI, revertDirection(iDirection) ) );
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
