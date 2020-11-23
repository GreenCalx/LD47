using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Linq;

public class Level : MonoBehaviour
{
    public GameObject go_stage_selector;

    public enum WORLD_POI
    {
        NONE            = 0,
        LOCKED_STAGE    = 1,
        UNLOCKED_STAGE  = 2,
        DONE_STAGE      = 3,
        START_STAGE     = 4,
        CONNECTOR       = 5, // empty connection to broadcast input to its connex POI
        LEVEL_CONNECTOR = 6 // Connects levels together ( path from LEVEL0 to LEVEL1 for ex )
    }    

    // List of loaded stages for this level
    [HideInInspector]
    public List<Stage> lstages;
    [HideInInspector]
    public List<LConnector> lLConnectors;


    // level_id is level's level ( level 0, level 1, etc.. ) 
    public int level_id = 0;
    private const string world_grid_file_path         = "Assets/LevelPOIGrids/";
    private const string world_grid_file_name_prefix  = "LEVEL";
    private const string world_grid_file_ext          = ".txt";

    private const string webgl_alt_path = "LevelPOIGrids/LEVEL";


    private Dictionary<POI, Transform>   __poi_locations;
    private WORLD_POI[,]                 __world_pois;
    private int[,]                       __world_stage_layout;
    private int[,]                       __world_lconn_layout;


    private Tuple<int , int>             __start_coord;
    private StageSelector                __stage_selector;
    private int                          __n_rows = 0;
    private int                          __n_cols = 0;

    // Start is called before the first frame update
    void Start()
    {   

    }

    // TODO [ if u can read this and its not october or november then gotta be fixd ]
    // this is a cheap fix to an issue where not all stages
    // have started their script when this init is called.
    // so we wait for the first update and use a bool to make it only once
    // would need a worldmanager as well to register loaded objects and init sequence.
    private bool init_done = false;
    void Update()
    {
        if (!init_done)
        {
            init();
            initPlayer();
            refreshStages();
            init_done = true;
        }
    }

    private void refreshStages()
    {
        foreach( Stage s in lstages )
            s.refresh();
    }

    private static int compareStage( Stage s1, Stage s2)
    {
        if ( s1 == null )
            if ( s2 == null )
                return 0; // S1 == S2
            else
                return -1; // S2 <<
        else
            if ( s2 == null )
                return 1; // S1 <<
            else
            {
                // delta on stage ids to cp
                return s1.id - s2.id;
            }
    }

    public void initPlayer()
    {
        if (!!go_stage_selector)
        {
            __stage_selector = go_stage_selector.GetComponent<StageSelector>();
            if (!!__stage_selector)
            {
                if ( (InterSceneCache.world_from == InterSceneCache.UNDEFINED) &&
                     (InterSceneCache.stage_from == InterSceneCache.UNDEFINED) )
                    __stage_selector.init( level_id, lstages[0]); // default at stage 0
                else if ( InterSceneCache.stage_from != InterSceneCache.UNDEFINED )
                { // from stage
                    __stage_selector.init( level_id, lstages[InterSceneCache.stage_from]);
                }
                else
                { // from other world
                    foreach( LConnector lcon in lLConnectors )
                    {
                        if ( lcon.level_target == InterSceneCache.world_from )
                        {
                            __stage_selector.init( level_id, lcon );
                            break;
                        }
                    }
                }

                Transform t_destination = __poi_locations[__stage_selector.selected_poi];
                __stage_selector.moveTo(t_destination);
            }
            
        }

    }

    private void initFromStaticDatas()
    {
        string[] level_poi      = LEVEL_LAYOUTS.load_level_poi(level_id);
        string[] level_stages   = LEVEL_LAYOUTS.load_level_stages(level_id);
        string[] level_lconn    = LEVEL_LAYOUTS.load_level_lconnectors(level_id);
        __n_rows = level_poi.Length;
        if (__n_rows <= 0 )
            return;
        __n_cols = level_poi[0].Length;

        // TODO : Factorize those 3 loops in 1
        // Build world POIs
        __world_pois = new WORLD_POI[__n_rows,__n_cols];
        for (int i=0;i < __n_rows; i++)
        {
            string line = level_poi[i];

            // Build POI states
            for (int j=0; j < __n_cols ; j++ )
            {
                char poi = line[j];
                __world_pois[i,j] = (WORLD_POI)Enum.Parse( typeof(WORLD_POI), poi.ToString()); // in the given enum range or crash ?
                if (__world_pois[i,j] == WORLD_POI.START_STAGE)
                    __start_coord = new Tuple<int,int>(i,j);
            }//! for j cols
        }//! for i rows

        // Build stage layout
        __world_stage_layout = new int[__n_rows, __n_cols];
        for (int i=0;i < __n_rows; i++)
        {
            string line = level_stages[i];

            for (int j=0; j < __n_cols ; j++ )
            {
                char cstage_id = line[j];
                if ( cstage_id == '-' )
                {
                    __world_stage_layout[i,j] = -1;
                    continue;
                }

                string hex_val = cstage_id.ToString();
                int stage_id = Int32.Parse( hex_val, System.Globalization.NumberStyles.HexNumber );
                __world_stage_layout[i,j] = stage_id;

            }
        }

        // Build level connectors layout
        __world_lconn_layout = new int[__n_rows, __n_cols];
        for (int i=0;i < __n_rows; i++)
        {
            string line = level_lconn[i];

            for (int j=0; j < __n_cols ; j++ )
            {
                char cstage_id = line[j];
                if ( cstage_id == '-' )
                {
                    __world_lconn_layout[i,j] = -1;
                    continue;
                }

                string hex_val = cstage_id.ToString();
                int lconn_id = Int32.Parse( hex_val, System.Globalization.NumberStyles.HexNumber );
                __world_lconn_layout[i,j] = lconn_id;

            }
        }

    }

    public void init()
    {
        // Read lmevel file and build world_pois
        string path = world_grid_file_path + world_grid_file_name_prefix
                        + level_id + world_grid_file_ext;

        initFromStaticDatas();

        if ( __start_coord == null )
        {
            __start_coord = new Tuple<int,int>(0,0); // default start..
            Debug.LogError(" no starting stage found for current world. default is 0,0.");
        }

        // INIT internals structs with __world_pois
        __poi_locations = new Dictionary<POI, Transform>(0);

        // init stages
        Stage[] stages = GetComponentsInChildren<Stage>();
        if (stages.Length == 0)
            return;
        lstages = stages.ToList();
        lstages.Sort(compareStage);
        // Get locations
        foreach( Stage s in lstages )
            __poi_locations.Add( s, s.gameObject.transform );

        // init level connectors
        LConnector[] lconnectors = GetComponentsInChildren<LConnector>();
        if (lconnectors.Length == 0)
            return; // must have at least a 'from' lConnector if not level0 (which has a level1 connector)
        lLConnectors = lconnectors.ToList();
        foreach( LConnector lc in lLConnectors)
            __poi_locations.Add( lc, lc.gameObject.transform);

        // Build connections
        buildStageAndConnections( __n_rows, __n_cols);
    }

    private void buildStageAndConnections( int row_boundary, int col_boundary)
    {

        List<Tuple<int,int>> paths = new List<Tuple<int,int>>{ Tuple.Create(__start_coord.Item1, __start_coord.Item2 ) };
        int curr_stage_id   = 0;
        while ( paths.Any() )
        {
            Tuple<int, int> curr_path = paths[0];
            Stage curr_stage = lstages[__world_stage_layout[curr_path.Item1, curr_path.Item2]];
            paths.RemoveAt(0);

            // update stage completion from save file
            WORLD_POI curr_poi =  __world_pois[curr_path.Item1, curr_path.Item2];
            curr_stage.updateCompletion(curr_poi);

            // check neighbors
            for (int i = -1; i <= 1; i++)
            {
                for ( int j = -1; j <= 1; j++)
                {
                    if ((i == 0) && (j == 0))
                        continue; // self
                    int row = curr_path.Item1 + i;
                    int col = curr_path.Item2 + j;
                    if ( (row < 0) || (col < 0) ) // OOB
                        continue;
                    if ( (row >= row_boundary) || (col >= col_boundary) ) // OOB
                        continue;
                    WORLD_POI poi =  __world_pois[row, col];
                    if ( poi == WORLD_POI.NONE ) // NOTHING HERE
                        continue;
                    
                    // Stages only reachable for UP/DOWN/LEFT/RIGHT.
                    // we cut corners out
                    bool stage_is_reachable = (Math.Abs(i) + Math.Abs(j)) != 2;
                    if ( poiIsStage(poi) && stage_is_reachable )
                    {
                        int stage_id = __world_stage_layout[row, col];
                        POI.DIRECTIONS direction = POI.getDirection(j, i);
                        Stage stage_to_connect = lstages[stage_id];
                        if ( curr_stage.connectTo( stage_to_connect, direction) )
                        {
                            paths.Add(Tuple.Create(row, col));
                        }
                    }

                    // POI is a Lconnector, same connectablility than stages
                    if ( poiIsLevelConnector(poi) && stage_is_reachable )
                    {
                        int level_id = __world_lconn_layout[row, col];
                        Debug.Log(" Connect level " + level_id);

                        POI.DIRECTIONS direction = POI.getDirection(j, i);
                        POI target = null;
                        foreach ( LConnector lc in lLConnectors)
                        {
                            if ( lc.level_target == level_id)
                            {
                                target = lc;
                                break;
                            }
                        }
                        bool op_succ = curr_stage.connectTo( target, direction);
                        if (!op_succ)
                            Debug.Log(" FAILED TO CONNECT LEVEL " + level_id);
                        else
                            Debug.Log(" SUCCESS TO CONNECT LEVEL " + level_id + " to stage " + curr_stage.id);
                    }

                    // Connectors can connect diagonally
                    // Only one connector path with a target workds
                    if (poiIsConnector(poi))
                    {
                        POI.DIRECTIONS direction = POI.getDirection(j, i);
                        POI conn_target = findConnectorTarget( row, col, curr_path.Item1, curr_path.Item2, row_boundary, col_boundary ); 
                        if (conn_target == null)
                            continue;
                        bool op_succ = curr_stage.connectTo( conn_target, direction);
                        if (!op_succ)
                            Debug.Log(" FAILED TO CONNECT LEVEL ");
                    }
                    

                }//! for j cols
            }//! for i rows
        }

    }

    private POI findConnectorTarget( int conn_x, int conn_y, int from_x, int from_y, int row_boundary, int col_boundary)
    {
        POI target = null;
        for (int i = -1; i <= 1; i++)
        {
            for ( int j = -1; j <= 1; j++)
            {
                int row = conn_x + i;
                int col = conn_y + j;

                if ( (i == 0) && ( j == 0) )
                    continue; // identity
                if ( (from_x == row) && ( from_y == col) )
                    continue; // loop detected
                if ( (row < 0) || (col < 0) ) // OOB
                        continue;
                if ( (row >= row_boundary) || (col >= col_boundary) ) // OOB
                        continue;

                WORLD_POI poi =  __world_pois[row, col];
                if ( poiIsConnectorTarget(poi) )
                {
                    if ( poiIsStage(poi) )
                    {
                        int stage_id = __world_stage_layout[row,col];
                        target = lstages[stage_id];
                    } else // LConnector
                    {
                        int level_to_connect = __world_lconn_layout[row,col];
                        foreach ( LConnector lc in lLConnectors)
                        {
                            if ( lc.level_target == level_to_connect)
                            {
                                target = lc;
                                break;
                            }
                        }
                    }
                    break;
                }

                if ( poiIsConnector(poi) )
                    return findConnectorTarget( row, col, conn_x, conn_y, row_boundary, col_boundary);
            }
            if (target != null)
                break;
        }

        return target;    
    }

    private bool poiIsStage( WORLD_POI iPOI )
    {
        return ((iPOI==WORLD_POI.LOCKED_STAGE) ||
                (iPOI==WORLD_POI.UNLOCKED_STAGE) ||
                (iPOI==WORLD_POI.DONE_STAGE) ||
                (iPOI==WORLD_POI.START_STAGE) );
    }

    private bool poiIsConnector( WORLD_POI iPOI )
    {
        return (iPOI==WORLD_POI.CONNECTOR);
    }
    
    private bool poiIsConnectorTarget( WORLD_POI iPOI )
    {
        return ( (iPOI==WORLD_POI.LEVEL_CONNECTOR) || poiIsStage(iPOI) );
    }

    private bool poiIsLevelConnector( WORLD_POI iPOI )
    {
        return (iPOI==WORLD_POI.LEVEL_CONNECTOR);
    }

}
