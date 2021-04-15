using System;

public static class LevelProgress
{

    public static bool DEBUG_ACTIVATE_ALL = false;

    // Number of stages per Level
    public static readonly int[] n_stages_per_level = { 9, 10 };
    public static bool[] arrLevel0;
    public static bool[] arrLevel1; // CONT..

    static LevelProgress()
    {
        //default all to false
        arrLevel0 = new bool[n_stages_per_level[0]];
        arrLevel1 = new bool[n_stages_per_level[1]];

        //etc..

        if ( DEBUG_ACTIVATE_ALL )
        {
            unlockAll();
        }
    }

    // only level 0 right now
    public static void unlockAll()
    {
        for (int i = 0; i < arrLevel0.Length; i++ )
            arrLevel0[i] = true;
    }

    public static void loadSave()
    {
        // todo ; in the meantime it resets all...
        resetStageProgress();
    }

    public static void resetStageProgress()
    {
        arrLevel0 = new bool[n_stages_per_level[0]];
        arrLevel1 = new bool[n_stages_per_level[1]];
    }

    public static void completeStage( int levelID, int stageID)
    {
        if ( (levelID < 0) || (stageID < 0) )
            return;
            
        switch (levelID)
        {
            case 0:
                if ( stageID < arrLevel0.Length )
                    arrLevel0[stageID] = true;
                break;
            case 1:
                if ( stageID < arrLevel1.Length )
                    arrLevel1[stageID] = true;
                break;
            default:
                break;
        }
    }

    public static bool getCompletion( int levelID, int stageID)
    {
        bool completion = false;
        if ( (levelID < 0) || (stageID < 0) )
            return completion;

        switch (levelID)
        {
            case 0:
                if ( stageID < arrLevel0.Length )
                    completion = arrLevel0[stageID];
                break;
            case 1:
                if ( stageID < arrLevel1.Length )
                    completion = arrLevel1[stageID];
                break;
            default:
                break;
        }
        return completion;
    }

    public static bool gameIsFinished()
    {
        return false; // TODO
    }
}