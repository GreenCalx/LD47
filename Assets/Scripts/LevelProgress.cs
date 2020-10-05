public static class LevelProgress
{
    public static bool level0 = false;
    public static bool level1 = false;
    public static bool level2 = false;
    public static bool level3 = false;
    public static bool level4 = false;
    public static bool level5 = false;
    public static bool level6 = false;
    public static bool level7 = false;
    public static bool level8 = false;

    public static void setProgress( int iLevelIndex, bool iCompletion )
    {
        switch (iLevelIndex)
        {
            case 0:
                level0 = iCompletion;
                break;
            case 1:
                level1 = iCompletion;
                break;
            case 2:
                level2 = iCompletion;
                break;
            case 3:
                level3 = iCompletion;
                break;
            case 4:
                level4 = iCompletion;
                break;
            case 5:
                level5 = iCompletion;
                break;
            case 6:
                level6 = iCompletion;
                break;
            case 7:
                level7 = iCompletion;
                break;
            case 8:
                level8 = iCompletion;
                break;
            default:
                break;
        }
    }

    public static bool finishedAllLevels()
    {
        return (level0 && level1 && level2 && level3 && level4 && level5 && level6 && level7 && level8);
    }
}