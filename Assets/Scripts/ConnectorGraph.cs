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

}

public struct WireChunk
{
    public List<Vector3Int> predecessors;
    public List<Vector3Int> successors;
    public Vector3Int coord;
    public bool hasImpulse;

    public WireChunk( Vector3Int iCoord )
    {
        coord = iCoord;
        hasImpulse = false;
        predecessors = new List<Vector3Int>();
        successors = new List<Vector3Int>();
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
            wires.Add( new Wire(root_chunk) );
            foreach( Vector3Int coord in neighbors )
            { root_chunk.successors.Add(coord); }

            Wire wire = new Wire(root_chunk);

            // 2. Assemble root paths
            //List<List<Vector3Int>> wip_paths = new List<List<Vector3Int>>(0);
            //foreach( Vector3Int coord in neighbors )
            //{
            //    List<Vector3Int> new_path = new List<Vector3Int>();
            //    new_path.Add(start_pos);
            //    new_path.Add(coord);
            //    wip_paths.Add( new_path );
            //}

 
            // 2.1 recurse until we meet an activable object
            // (store each tile/coord in path)
            //foreach( List<Vector3Int> path in wip_paths)
            //{
            //    findPath(path);
            //}

            // Build path for curr wire
            findPath( ref wire, root_chunk );
            // TODO... explore en profondeur

            // Add Wire
            wires.Add(wire);

        }//! fe

        // 4. transform path into tilebase paths

    }

    void findPath( ref Wire ioCurrWire, WireChunk iChunkToExpand )
    {
        foreach( Vector3Int path_chunk in iChunkToExpand.successors)
        {
            List<Vector3Int> neighbors = findNeighbors(path_chunk);
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
                ioCurrWire.chunks.Add(new_chunk);
            }
        }
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
            Debug.Log(" UP: " + up );
        if (!!down_tile)
            Debug.Log(" DOWN: " + down );
        if (!!left_tile)
            Debug.Log(" LEFT: " + left );
        if (!!right_tile)
            Debug.Log(" RIGHT: " + right );

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
