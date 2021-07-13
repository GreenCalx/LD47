using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct Wire
{
    public List<WireChunk> chunks;
    public Wire(WireChunk iRoot)
    {
        chunks = new List<WireChunk>(1);
        chunks.Add(iRoot);
    }

    // TODO : I don't like the fact that we need this
    public WireChunk getWChunkFromCoord( Vector3Int iGridCoord )
    {
        foreach( WireChunk wc in chunks)
        {
            if ( wc.coord == iGridCoord )
                return wc;
        }
        return null;
    }

}

public class WireChunk
{
    public List<Vector3Int> predecessors;
    public List<Vector3Int> successors;
    public Vector3Int coord;
    public bool hasImpulse;
    public List<ActivableObject> targets;

    public WireChunk( Vector3Int iCoord )
    {
        coord = iCoord;
        hasImpulse = false;
        predecessors = new List<Vector3Int>();
        successors = new List<Vector3Int>();
        targets = new List<ActivableObject>();
    }

    public void AddTargets( List<ActivableObject> iTargets )
    {
        foreach ( ActivableObject ao in iTargets )
        {
            if ( !targets.Contains(ao) )
                targets.Add(ao);
        }
        //targets.AddRange(iTargets);
    }
}

public class ConnectorGraph : MonoBehaviour
{
    public List<Wire> wires;
    public List<ActivatorObject> emitters;
    private Dictionary<ActivatorObject,LinkedList<TileBase>> paths;
    public Dictionary<ActivatorObject,LinkedList<TileBase>> getPaths() 
    { return paths; }

    private GridLayout GL;
    private Tilemap TL;

    // Start is called before the first frame update
    void Start()
    {
        wires = new List<Wire>();
        paths = new Dictionary<ActivatorObject, LinkedList<TileBase>>(0);
        GL = GetComponentInParent<GridLayout>();
        TL = GetComponent<Tilemap>();

        BuildGraph();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BuildGraph()
    {
        foreach( ActivatorObject ao in emitters )
        {
            // 0. Find tile on which emitter is
            Vector3Int start_pos = GL.WorldToCell( ao.transform.position );
            TileBase start_tile  = TL.GetTile(start_pos);

            // 1. Find neighbor connection for root on this layer of the mapping
            List<Vector3Int> neighbors = findNeighbors(start_pos);

            // 1.1 Update wires with root
            WireChunk root_chunk = new WireChunk(start_pos);


            Wire wire = new Wire(root_chunk);
            // Add Wire
            wires.Add(wire);

            // Build path for curr wire
            for( int i = 0; i < wires.Count ; i++)
            {
                findPath( ref wire, root_chunk );
            }
                        

        }//! fe

        // 4. TODO : Junctions with Activator and Activable with Wire
        
    }

    void findPath( ref Wire ioCurrWire, WireChunk iChunkToExpand )
    {
        if ( iChunkToExpand == null )
            return;

        // Find neighbors and set successors/predecessors
        List<Vector3Int> neighbors = findNeighbors(iChunkToExpand.coord);

            foreach( Vector3Int neighbor in neighbors )
            {
                if ( iChunkToExpand.predecessors.Contains(neighbor) )
                    continue;
                if ( iChunkToExpand.coord == neighbor )
                {    
                    iChunkToExpand.predecessors.Add(neighbor);
                    continue;
                }
                iChunkToExpand.successors.Add(neighbor);
                WireChunk new_chunk = new WireChunk( neighbor );
                new_chunk.predecessors.Add(iChunkToExpand.coord);
                ioCurrWire.chunks.Add(new_chunk);
            }

        // recurse to explore path in deepness
        if ( iChunkToExpand.successors.Count > 0)
        {
            foreach( Vector3Int coord_chunk_to_explore in iChunkToExpand.successors )
                findPath(ref ioCurrWire, ioCurrWire.getWChunkFromCoord(coord_chunk_to_explore) );
        } else {
            // try to find an activable target if no successors
            if ( findActivableTarget(iChunkToExpand) )
            {
                Debug.Log(" Found an activable target for current wire path.");
                //ioCurrWire.targets.Add( iChunkToExpand.target.gameObject );
            }
        }
    } //! findPath

    public bool findActivableTarget( WireChunk iChunk )
    {
        Vector3Int up       = iChunk.coord + new Vector3Int(0,1,0);
        Vector3Int down     = iChunk.coord + new Vector3Int(0,-1,0);
        Vector3Int left     = iChunk.coord + new Vector3Int(-1,0,0);
        Vector3Int right    = iChunk.coord + new Vector3Int(1,0,0);

        // switch to woorld coordinate
        Vector3 worldcoord_up = GL.CellToWorld(up);
        Vector3 worldcoord_down = GL.CellToWorld(down);
        Vector3 worldcoord_left = GL.CellToWorld(left);
        Vector3 worldcoord_right = GL.CellToWorld(right);


        // ALT solution : Make a cache with activable objects pos and compare distance with woorldcoord_x
        // check if there is an activable object in current radius
        Vector2 up_center       = new Vector2( worldcoord_up.x, worldcoord_up.y);
        Vector2 down_center     = new Vector2( worldcoord_down.x, worldcoord_down.y);
        Vector2 left_center     = new Vector2( worldcoord_left.x, worldcoord_left.y);
        Vector2 right_center    = new Vector2( worldcoord_right.x, worldcoord_right.y);
        float radius = 1.0f;

        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        List<Collider2D> up_col = new List<Collider2D>(5),
                         down_col = new List<Collider2D>(5),
                         left_col = new List<Collider2D>(5),
                         right_col = new List<Collider2D>(5);
        int res_up = Physics2D.OverlapCircle( up_center, radius, filter, up_col );
        int res_down = Physics2D.OverlapCircle( down_center, radius, filter, down_col);
        int res_left = Physics2D.OverlapCircle( left_center, radius, filter, left_col);
        int res_right = Physics2D.OverlapCircle( right_center, radius, filter, right_col);

        if ( res_up > 0)
        {
            iChunk.AddTargets( LookForActivables(up_col) );
        }
        if ( res_down > 0)
        {
            iChunk.AddTargets( LookForActivables(down_col) );
        }
        if ( res_left > 0)
        {
            iChunk.AddTargets( LookForActivables(left_col) );
        }
        if ( res_right > 0)
        {
           iChunk.AddTargets( LookForActivables(right_col) );
        }

        return iChunk.targets.Count > 0;
    }

    public List<ActivableObject> LookForActivables( List<Collider2D> iCols )
    {
        List<ActivableObject> activables = new List<ActivableObject>();
        foreach( Collider2D col in iCols )
        {
            ActivableObject ao = col.gameObject.GetComponent<ActivableObject>();
            if (!!ao)
                activables.Add(ao);
        }
        return activables;
    }

    public List<Vector3Int> findNeighbors( Vector3Int pos )
    {
        List<Vector3Int> retval = new List<Vector3Int>(0);

        Vector3Int up       = pos + new Vector3Int(0,1,0);
        Vector3Int down     = pos + new Vector3Int(0,-1,0);
        Vector3Int left     = pos + new Vector3Int(-1,0,0);
        Vector3Int right    = pos + new Vector3Int(1,0,0);

        TileBase up_tile    = TL.GetTile(up);
        TileBase down_tile  = TL.GetTile(down);
        TileBase left_tile  = TL.GetTile(left);
        TileBase right_tile = TL.GetTile(right);

        if (!!up_tile)
            retval.Add(up);
        if (!!down_tile)
            retval.Add(down);
        if (!!left_tile)
            retval.Add(left);
        if (!!right_tile)
            retval.Add(right);
        
        return retval;
    }

}
