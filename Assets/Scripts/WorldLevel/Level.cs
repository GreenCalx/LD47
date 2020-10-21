using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class Level : MonoBehaviour
{

    public readonly int[,] world_grid; // useless ? not used

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
    private Tuple<int , int>             __start_coord;

    // Start is called before the first frame update
    void Start()
    {   
        init();
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
                return S1.id - S2.id;
            }
    }

    public void init()
    {
        // Read lmevel file and build world_pois
        string path = world_grid_file_path + world_grid_file_name_prefix
                        + level_id + world_grid_file_ext;
        StreamReader reader = new StreamReader(path);

        int n_rows = 0, n_cols = 0;
        while (reader.Peek() >= 0)
        {
            string line = reader.ReadLine();
            n_cols = line.Length;
            n_rows++;
            Debug.Log(line);
        }
        __world_pois = new WORLD_POI[n_rows,n_cols];
        reader.BaseStream.Position = 0;
        for (int i=0;i < n_rows; i++)
        {
            string line = reader.ReadLine();
            for (int j=0; j < n_cols ; j++ )
            {
                char poi = line[j];
                __world_pois[i,j] = (WORLD_POI)Enum.Parse( typeof(WORLD_POI), poi.ToString()); // in the given enum range or crash ?
                if (__world_pois[i,j] == WORLD_POI.START_STAGE)
                    __start_coord = new Tuple<int,int>(i,j);
            }//! for j cols
        }//! for i rows
        reader.Close();
        if ( __start_coord == null )
        {
            __start_coord = new Tuple<int,int>(0,0); // default start..
            Debug.LogError(" no starting stage found for current world. default is 0,0.")
        }

        // INIT internals structs with __world_pois
        // init stages
        Stage[] stages = GetComponentsInChildren<Stage>();
        lstages = stages.ToList();
        lstages.Sort(compareStage);

        buildStageAndConnections(lstages);
        // init level connectors

    }

    private void buildStageAndConnections()
    {

        List<Tuple<int,int>> paths = new List<Tuple<int,int>>{ Tuple.Create(__start_coord.Item1, __start_coord.Item2 ); };
        int curr_stage_id   = 0;
        int n_stages_to_set = lstages.Count;
        while ( paths.Any() )
        {
            Tuple<int, int> curr_path = paths[0];
            Stage curr_stage = lstages[curr_stage_id];
            paths.RemoveAt(0);

            // check neighbors
            for (int i = -1; i <= 1; i++)
            {
                for ( int j = -1; j <= 1; j++)
                {
                    int row = curr_path.Item1 + i;
                    int col = curr_path.Item2 + j;
                    if ( (row < 0) || (col < 0) ) // OOB
                        continue;
                    WORLD_POI poi =  __world_pois[row, col];
                    if ( poi == WORLD_POI.NONE ) // NOTHING HERE
                        continue;
                    
                    // Stages only reachable for UP/DOWN/LEFT/RIGHT.
                    // we cut corners out
                    bool stage_is_reachable = (Math.Abs(row) + Math.Abs(col)) != 2;
                    if ( poiIsStage(poi) && stage_is_reachable )
                    {
                        
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
    void Update()
    {
        
    }
}
