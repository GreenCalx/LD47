using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using System.Linq; // Intersect, Union on Lists

/**
* ATTACH THIS COMPONENT TO CONNECTION LAYER OF THE STAGE MAP
* 
* TODO : CALL UPDATE_WIRE() each tick in world
*   retrieve curr loaded connector graph from stage_selector => stage => get_connector_graph() and call update_wires()
*/

[System.Serializable]
public enum SIGNAL_KEYS
{
    NONE, BLUE, RED, YELLOW
}

public class TempWireValue : FixedTickValue
{
    public Wire Obj;

    public override void OnTick()
    {
        base.OnTick();
        Obj.update_pulses();
        Obj.emitter.can_pulse = true;

    }

    public override void OnBackTick()
    {
        base.OnBackTick();
        Obj.update_pulses();
        Obj.emitter.can_pulse = true;
    }
    public override void OnPostTick()
    {
        Obj.resetPWStorages();
    }
}

public class PulseToken
{
    // propagation speed
    public int speed;

    // Required Weight : defined by activator targets by number of emitters in wire.
    // IF <= required_weight : DEACTIVATE
    // IF > required_weight  : ACTIVATE
    public int PW; // pulsewidth 

    // TODO REMOVEME when we stop clonin like fagittiz
    public bool deletion_flag;
    public PulseToken( int iSpeed, int iPW )
    {
        speed = iSpeed;
        PW = iPW;
        deletion_flag = false;
    }
    public PulseToken( int iSpeed ) : this( iSpeed, 1 )
    {}

    public PulseToken getPropagated()
    {
        Debug.LogWarning("!!! Creates unnecessary object. TODO : DO BETTA");
        return new PulseToken(speed-1, PW);
    }
}

public class Wire
{
    public enum Events {
        NONE,
        INF_ON,
        INF_OFF,
        TRIGG_ON,
        TRIG_OFF,
        PULSE
    }

    public ConnectorGraph CG;
    public bool has_TL_obs;
    public SIGNAL_KEYS sig_key;
    public ActivatorObject emitter;
    public int pulse_speed;
    public bool is_infinite;
    public List<WireChunk> chunks;
    public WireChunk root_chunk;

    public bool pulses_got_updated = false; // TEMP OnPostTick

    public void print()
    {
        Debug.Log("WIRE " + emitter.gameObject.name + " of size " + chunks.Count);
        foreach(WireChunk wc in chunks)
        { 
            Debug.Log(wc.coord);
            if (wc.targets.Count != 0)
            {
                foreach( ActivableObject ao in wc.targets)
                    Debug.Log("Chunk Target : " + ao.name );
            }
        }
    }

    public Wire(ActivatorObject iEmitter, SIGNAL_KEYS iSigType, ConnectorGraph iCG)
    {
        CG = iCG;
        chunks = new List<WireChunk>(1);

        if ( iEmitter == null )
        {
            Debug.LogError("Missing Emitter in Wire.");
            emitter = null;
        }
        emitter = iEmitter;
        sig_key = iEmitter.signalKey;
        pulse_speed = emitter.pulse_speed;
        root_chunk = null;
        is_infinite = ( pulse_speed <= 0 ) ? true : false ;
        has_TL_obs = false;
    }

    public Wire (ActivatorObject iEmitter, ConnectorGraph iCG) : this( iEmitter, SIGNAL_KEYS.NONE, iCG)
    {}

    public void addRootChunk(Vector3Int iRootPosition)
    {
        root_chunk = new WireChunk(iRootPosition, this, CG);
        chunks.Add(root_chunk);
    }

    public WireChunk getRootChunk()
    {
        return chunks[0];
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

    public void delete_chunk( WireChunk iChunk )
    {
        foreach( Vector3Int succ in iChunk.successors )
        { 
            WireChunk wc_succ = getWChunkFromCoord(succ);
            wc_succ.predecessors.Remove(iChunk.coord); 
        }
        foreach( Vector3Int pred in iChunk.predecessors )
        { 
            WireChunk wc_pred = getWChunkFromCoord(pred);
            wc_pred.successors.Remove(iChunk.coord); 
        }

        chunks.Remove(iChunk);
    }

    public List<ActivableObject> getWireTargets()
    {
        List<ActivableObject> retval = new List<ActivableObject>();
        foreach (WireChunk wc in chunks)
        {
            if (wc.targets.Count > 0)
                retval.AddRange(wc.targets);
        }
        return retval;
    }

    public void resetPWStorages()
    {
        List<ActivableObject> targets = getWireTargets();
        foreach( ActivableObject t in targets)
            t.resetStorage();
    }

    public void pulse(bool iState)
    {
        if ( is_infinite || (pulse_speed <= 0) || (pulse_speed >= chunks.Count) )
        {
            // Instantaneous trigger
            is_infinite = true;
            List<ActivableObject> aos = getWireTargets();
            foreach (ActivableObject target in aos )
            {
                // if (iState)
                //     target.activate();
                // else
                //     target.deactivate();
                PulseToken ptk = new PulseToken(0, iState?1:-1);
                target.listen(ptk);
                ptk = null; // wait GC...
            }
            //(TL.GetCursorValue() as WireTimelineValue)._WireEvent = iState ? Events.INF_ON : Events.INF_OFF ;
                
            return;
        }

        getRootChunk().pulse_bag.Add( new PulseToken(pulse_speed) );
        getRootChunk().propagateAll();  
    }

    public void update_pulses()
    {
        // inf wire
        if (is_infinite)
            return;

        // propagation of pulses
        int n_chunks = chunks.Count;
        for( int i=n_chunks-1; i>=0 ; i--)
        {
            WireChunk wc = chunks[i];
            if (wc==null)
            {
                Debug.LogError("Missing WireChunk.");
                continue;
            }
            // reset activated state
            wc.activated_this_cycle = false;

            // propagate impulses if needed
            if (wc.hasImpulse())
            {
                wc.propagateAll();
            }
        }

        // If no activation of chunks carrying targets, we deactivate targets.
        foreach( WireChunk wc in chunks )
        {
            if (wc.targets.Count!=0)
            {
                if (!wc.activated_this_cycle)
                {
                    foreach ( ActivableObject ao in wc.targets )
                    { ao.deactivate(); }
                }
            }
        }

        pulses_got_updated = true; // TEMP OnPostTick
    }
}

public class WireChunk
{
    public List<Vector3Int> predecessors;
    public List<Vector3Int> successors;
    public Vector3Int coord;
    public List<PulseToken> pulse_bag;
    public List<ActivableObject> targets;

    public bool activated_this_cycle;

    public ConnectorGraph connectorGraph;
    public Wire wire;

    public WireChunk( Vector3Int iCoord, Wire iWire, ConnectorGraph iCG )
    {
        wire = iWire;
        coord = iCoord;
        pulse_bag = new List<PulseToken>();
        predecessors = new List<Vector3Int>();
        successors = new List<Vector3Int>();
        targets = new List<ActivableObject>();
        activated_this_cycle = false;

        connectorGraph = iCG;
        connectorGraph.TL.SetTileFlags( coord, TileFlags.None );

    }

    public void pulse_color()
    {
        connectorGraph.TL.SetColor( coord, Color.yellow );
    }

    public void reset_color()
    {
        connectorGraph.TL.SetColor( coord, Color.white );
    }

    public bool hasImpulse()
    {
        return (pulse_bag.Count > 0);
    }

    public void AddTargets( List<ActivableObject> iTargets )
    {
        foreach ( ActivableObject ao in iTargets )
        {
            if ((ao.sig_key != SIGNAL_KEYS.NONE ) && ( ao.sig_key != wire.sig_key ))
            {
                Debug.LogWarning("Mismatch between activator and activable signal type.");
                continue;
            }
            if ( !targets.Contains(ao) )
                targets.Add(ao);
        }
        //targets.AddRange(iTargets);
    }

    public void propagateAll()
    {
        foreach( PulseToken pt in pulse_bag)
        {
            propagatePulse(pt);
        }

        pulse_bag.RemoveAll( e => e.deletion_flag );
    }

    public void propagatePulse( PulseToken iPT )
    {
        if ( iPT.speed == 0 )
        {
            // Reset PT speed
            iPT.speed = wire.pulse_speed;
            pulse_bag.Add( iPT );
            pulse_color();
            return; // Stop propagation
        }

        if ( successors.Count == 0 )
        {
            foreach( ActivableObject target in targets)
            {
                this.activated_this_cycle = target.listen(iPT);
            }
            iPT.deletion_flag = true;
            reset_color();
        } else {
            foreach ( Vector3Int succ_coord in successors )
            {
                WireChunk wc = wire.getWChunkFromCoord(succ_coord);
                wc.propagatePulse( iPT.getPropagated() ); // decrement speed as we propagate
                iPT.deletion_flag = true;
                reset_color();
            }
        }
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
    [HideInInspector]
    public Tilemap TL;

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
            if (ao == null)
                continue;
                
            // Subscribe AO to this connector graph
            ao.subscribeToGraph(this);

            // 0. Find tile on which emitter is
            Vector3Int start_pos = GL.WorldToCell( ao.transform.position );
            TileBase start_tile  = TL.GetTile(start_pos);

            // 1. Find neighbor connection for root on this layer of the mapping
            List<Vector3Int> neighbors = findNeighbors(start_pos);

            // 1.1 Update wires with root
            Wire wire = new Wire(ao, ao.signalKey,this);
            wire.addRootChunk(start_pos);

            // Add Wire
            wires.Add(wire);

            // Build path for curr wire
            findPath( ref wire, wire.root_chunk );

            cut_dead_branches(ref wire);

            wire.print();

        }//! fe

        // 4. Try to merge wires ( common sections e.g. 2 emitters fo 1 activator )
        //  SI W1.chunks ∩ W2.chunks != {}
        //     ET W1.SIGNAL_TYPE == W2.SIGNAL_TYPE
        //  ALORS W3 = W1 U W2 
        //  PUIS delete W1, W2
        /*
        bool merge_occured = false;
        for ( int i=0; i < wires.Count ; i++ )
        {
            Wire w1 = wires[i];
            for (int j=i+1; j < wires.Count; j++)
            {
                Wire w2 = wires[j];
                Wire merged_w = null;
                if ( tryMergeWires( w1, w2, ref merged_w) )
                {
                    merge_occured = true;
                    
                    wires.Remove(w1);
                    wires.Remove(w2);
                    wires.Insert(0, merged_w);

                    break;
                }
            } // !for j wire

            if ( merge_occured )
            { // reset main loop to try to merge with other wires as well.
                i = 0;
                merge_occured = false;
            }
        } // !for i wire
        */

        // 4. TODO : Junctions with Activator and Activable with Wire
        
    }

    public void cut_dead_branches( ref Wire ioWire )
    {
        List<WireChunk> to_cut = new List<WireChunk>();
        foreach( WireChunk wc in ioWire.chunks )
        {
            if( (wc.successors.Count == 0) && (wc.targets.Count == 0) )
            {
                // Dead branch
                WireChunk cut_wc = wc;
                int n_pred = cut_wc.predecessors.Count;
                int n_succ = cut_wc.successors.Count;
                int n_neighbors = n_pred + n_succ;
                to_cut.Add(cut_wc);
                while ( n_pred == 1 ) // dangerous?
                {
                    WireChunk predec = ioWire.getWChunkFromCoord(cut_wc.predecessors[0]);
                    n_pred = predec.predecessors.Count;
                    n_succ = predec.successors.Count;
                    n_neighbors = n_pred + n_succ;
                    if (n_neighbors > 2)
                        break;

                    to_cut.Add(predec);
                    cut_wc = predec;
                } //!while
                // got out of while if >1 precessor (junc) or solo (0 p)
                // thus we add this last WC to cut and we'll remove links to it afterwards
                //to_cut.Add(cut_wc);
            }
        }//! for wc

        // Actual cut
        foreach( WireChunk wc_to_cut in to_cut )
        {
            ioWire.delete_chunk(wc_to_cut);
        }

    }

    public bool tryMergeWires ( Wire iW1, Wire iW2, ref Wire oW3 )
    {
        Debug.Log("tryMergeWires");
        List<WireChunk> res = iW1.chunks.Intersect( iW2.chunks ).ToList();
        if ( res.Count > 0 )
        {
            foreach( WireChunk wc in res)
            { Debug.Log(wc.coord); }
        }

        return false;
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
                WireChunk new_chunk = new WireChunk( neighbor, ioCurrWire, this );
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

    public void pulsateFrom( ActivatorObject iAO, bool iState )
    {
        foreach ( Wire w in wires )
        {
            if (w.emitter == iAO)
            {
                if ( iState == true )
                    w.pulse(iState);
                else if ( w.is_infinite )
                {
                    w.pulse(iState);
                }
                
            }
        }//! pulsateFrom
    }

    public void observeWire(ActivatorObject ao)
    {
        var WireValue = new TempWireValue();
        foreach( Wire w in wires )
        {
            if ( w.emitter == ao )
            {
                // if already has observer, we return to avoid double update_pulses/tick
                // might change later ?
                if (w.has_TL_obs)
                    return;

                WireValue.Obj = w;
                w.has_TL_obs = true;
                break;
            }

        }
        // register object to get called on tick
        // NOTE toffa : for now we will update the wire at the beginning of the tick
        GameObject.Find("GameLoop").GetComponent<WorldManager>().TL.AddObserver(WireValue);
    }


    // ---- TEMP OnPostTick ----
    public void tryResetWireStatuses()
    {
        bool needReset = true;
        foreach( Wire w in wires )
        {
            needReset &= w.pulses_got_updated;
        }

        if (!needReset)
            return;

        foreach( Wire w in wires )
        {
            w.pulses_got_updated = false;
        }
    }

    public void tryResetPulsesCounter()
    {
        bool needReset = true;
        foreach( Wire w in wires )
        {
            needReset &= w.pulses_got_updated;
        }

        if (needReset)
        {
            foreach( Wire w in wires )
                w.resetPWStorages();
        }
    }
    // --------------------------

}
