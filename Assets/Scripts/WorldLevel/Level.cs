using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class Level : MonoBehaviour
{

    public int[,] world_grid;

    [Flags]
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

    // level_id is level's level ( level 0, level 1, etc.. ) 
    public int level_id = 0;
    private const string world_grid_file_path         = "Assets/LevelPOIGrids/";
    private const string world_grid_file_name_prefix  = "LEVEL";
    private const string world_grid_file_ext          = ".txt";


    private Dictionary<Stage, Transform> __poi_locations;
    private WORLD_POI[,]                 __world_pois;

    // Start is called before the first frame update
    void Start()
    {   
        init();
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
            }//! for j cols
        }//! for i rows
        reader.Close();

        // INIT internals structs with __world_pois
        // init stages
        //Stage[] stages = GetComponentsInChildren<Stage>();

        // init level connectors

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
