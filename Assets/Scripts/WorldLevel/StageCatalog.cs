using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageCatalog
{

    private static readonly Dictionary<int, string> __stages_world0 = new Dictionary<int, string>()
    {
        { 0, "intro"},
        { 1, "apple"},
        { 2, "pie"},
        { 3, "feels"},
        { 4, "good"},
        { 5, "in"},
        { 6, "tummy"},
        { 7, ":)"}
    };

    public static string getStageName( int iWorldID, int iStageID)
    {
        switch( iWorldID ) 
        {
            case 0:
                return __stages_world0[iStageID];
                break;
            default:
                return "undefined";
                break;
        }
    }

}
