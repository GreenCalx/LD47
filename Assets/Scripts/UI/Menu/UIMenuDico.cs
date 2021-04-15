using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

class UIMenuDico
{

    public static readonly Dictionary<string, Action> dico = new Dictionary<string, Action>()
    {
        {"UNLOCK_LEVELS" , unlock_levels},
        {"LOCK_LEVELS"   , lock_levels},
        {"QWERTY" , qwerty_keyboard},
        {"AZERTY", azerty_keyboard}
    };

    private static void unlock_levels()
    {
        LevelProgress.unlockAll();
    }

    private static void lock_levels()
    {
        LevelProgress.loadSave();
    }

    public static void azerty_keyboard()
    {
        Debug.Log(" azerty_keyboard :: TO IMPL");
    }
    public static void qwerty_keyboard()
    {
        Debug.Log(" qwerty_keyboard :: TO IMPL");
    }

    // --------------------------------------------------
    public static Action getActionFromKey(string iKey)
    {
        return dico[iKey];
    }

    public static void callFunction(string iKey)
    {
        Action func;
        if( !dico.TryGetValue( iKey, out func) )
        {
            Debug.LogError("Failed to load UIMenu dico func for " + iKey + " in dico : " + dico.ToString() );
            return;
        }
        func();
    }

}
