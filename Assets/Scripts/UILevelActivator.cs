using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UILevelActivator : MonoBehaviour
{

    Button[] buttons;
    // Start is called before the first frame update
    void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        Debug.Log(" N BUTTONS " + buttons.Length);

        foreach (var b in buttons)
        {
            Text t = b.GetComponentInChildren<Text>();
            int level = Int32.Parse(t.text);
            Debug.Log(" level " + level);
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    b.interactable  = LevelProgress.level0;
                    break;
                case 2:
                    b.interactable  = LevelProgress.level1;
                    break;
                case 3:
                    b.interactable  = LevelProgress.level2;
                    break;
                case 4:
                    b.interactable  = LevelProgress.level3;
                    break;
                case 5:
                    b.interactable  = LevelProgress.level4;
                    break;
                case 6:
                    b.interactable  = LevelProgress.level5;
                    break;
                case 7:
                    b.interactable  = LevelProgress.level6;
                    break;
                case 8:
                    b.interactable  = LevelProgress.level7;
                    break;
                default:
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
