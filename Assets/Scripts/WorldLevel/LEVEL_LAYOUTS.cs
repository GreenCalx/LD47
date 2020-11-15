using System.Collections;
using System.Collections.Generic;
using System;

public static class LEVEL_LAYOUTS
{

    /// LEVEL 0
    public static readonly string[] level0_POI= 
    {
    "056500",
    "010100",
    "011110",
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
    public static readonly string[] level0_LCONNECTORS=
    {
    "--1---",
    "------",
    "------",
    "------",
    "------"
    }; 

    /// LEVEL 1
    public static readonly string[] level1_POI= 
    {
    "000000",
    "000000",
    "000000",
    "004100",
    "006000",
    }; 
    public static readonly string[] level1_STAGES=
    {
    "------",
    "------",
    "------",
    "--01--",
    "------"
    }; 
    public static readonly string[] level1_LCONNECTORS=
    {
    "------",
    "------",
    "------",
    "------",
    "--0---"
    }; 

    public static string[] load_level_poi(int id)
    {
        switch (id)
        {
            case 0:
                return level0_POI;
                break;
            case 1:
                return level1_POI;
                break;
            default:
                return new string[0];
                break;
        }
    }
    public static string[] load_level_stages(int id)
    {
        switch (id)
        {
            case 0:
                return level0_STAGES;
                break;
            case 1:
                return level1_STAGES;
                break;
            default:
                return new string[0];
                break;
        }
    }
    public static string[] load_level_lconnectors(int id)
    {
        switch (id)
        {
            case 0:
                return level0_LCONNECTORS;
                break;
            case 1:
                return level1_LCONNECTORS;
                break;
            default:
                return new string[0];
                break;
        }
    }


}
