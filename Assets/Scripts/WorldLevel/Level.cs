using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Linq;

public class Level : MonoBehaviour
{
    public GameObject go_stage_selector;
    //public readonly int[,] world_grid; // useless ? not used

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

    // level_id is level's level ( level 0, level 1, etc.. ) 
    public int level_id = 0;
    private const string world_grid_file_path         = "Assets/LevelPOIGrids/";
    private const string world_grid_file_name_prefix  = "LEVEL";
    private const string world_grid_file_ext          = ".txt";


    private Dictionary<Stage, Transform> __poi_locations;
    private WORLD_POI[,]                 __world_pois;
    private int[,]                       __world_stage_layout;
    private Tuple<int , int>             __start_coord;
    private StageSelector                __stage_selector;

    // Start is called before the first frame update
    void Start()
    {   

    }

    // TODO [ if u can read this and its not october then gotta be fixd ]
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
                __stage_selector.init(lstages[0]); // default at stage 0
                Transform t_destination = __poi_locations[__stage_selector.selected_stage];
                __stage_selector.moveTo(t_destination);    
            }
            
        }

    }

    public void init()
    {
        // Read lmevel file and build world_pois
        string path = world_grid_file_path + world_grid_file_name_prefix
                        + level_id + world_grid_file_ext;
        StreamReader reader = new StreamReader(path);

        int n_rows = 0, n_cols = 0;
        long stage_layout_reader_pos = -1;
        while (reader.Peek() >= 0)
        {
            string line = reader.ReadLine();
            if ( line.Contains("!") )
                continue;
            if ( line.Contains("#") )
            {
                if (  reader.Peek() < 0 )
                    break; 
                reader.ReadLine(); // line break we discard val
                stage_layout_reader_pos = reader.BaseStream.Position; 
                break;
            } else {
                n_cols = line.Length;
                n_rows++;
            }

            Debug.Log(line);
        }
        __world_pois = new WORLD_POI[n_rows,n_cols];
        reader.BaseStream.Position = 0;
        reader.DiscardBufferedData();
        for (int i=0;i < n_rows; i++)
        {
            string line = reader.ReadLine();
            // if comments ( should not happen )
            while ( line.Contains("!") ) 
                reader.ReadLine();
            if ( line.Contains("#") )
                break; // reached stage layout ( should not happen )

            // Build POI states
            for (int j=0; j < n_cols ; j++ )
            {
                char poi = line[j];
                __world_pois[i,j] = (WORLD_POI)Enum.Parse( typeof(WORLD_POI), poi.ToString()); // in the given enum range or crash ?
                if (__world_pois[i,j] == WORLD_POI.START_STAGE)
                    __start_coord = new Tuple<int,int>(i,j);
            }//! for j cols
        }//! for i rows

        // Build stage layout
        __world_stage_layout = new int[n_rows, n_cols];
        //reader.BaseStream.Position = stage_layout_reader_pos; // blocks reader at position somehow ?
        for (int i=0;i < n_rows; i++)
        {
            string line = reader.ReadLine();
            // if comments or break ( should not happen )
            while ( line.Contains("!") || line.Contains("#") )
                line =  reader.ReadLine();

            for (int j=0; j < n_cols ; j++ )
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

        reader.Close();


        if ( __start_coord == null )
        {
            __start_coord = new Tuple<int,int>(0,0); // default start..
            Debug.LogError(" no starting stage found for current world. default is 0,0.");
        }

        // INIT internals structs with __world_pois
        // init stages
        Stage[] stages = GetComponentsInChildren<Stage>();
        if (stages.Length == 0)
            return;
        lstages = stages.ToList();
        lstages.Sort(compareStage);
        // Get locations
        __poi_locations = new Dictionary<Stage, Transform>(0);
        foreach( Stage s in lstages )
            __poi_locations.Add( s, s.gameObject.transform );

        buildStageAndConnections( n_rows, n_cols);
        // init level connectors

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
                        
                        // update stage completion from save file
                        curr_stage.updateCompletion(poi);

                    }
                    // Connectors can connect diagonally
                    

                }//! for j cols
            }//! for i rows
        }

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
        return ((iPOI==WORLD_POI.CONNECTOR) ||
                (iPOI==WORLD_POI.LEVEL_CONNECTOR) );
    }

    // Update is called once per frame

}
