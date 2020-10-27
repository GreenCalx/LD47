using System.Collections;
using System.Collections.Generic;
using System;

public static class LEVEL_LAYOUTS
{

    public static readonly string[] level0_POI= 
    {
    "056500",
    "010100",
    "611116",
    "001000",
    "004000",
    }; 
    public static readonly string[] level0_STAGES=
    {
    "------",
    "-5-6--",
    "-3247-",
    "--1---",
    "--0---"
    }; 

    public static string[] load_level_poi(int id)
    {
        return level0_POI;
    }
    public static string[] load_level_stages(int id)
    {
        return level0_STAGES;
    }


}
