using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UITitleScreen : UIMenuSubscriber
{
    
    public Color selected_color;
    public Color unselected_color;
    public string scene_to_load;

    private Text[] __selectables;

    private int __select_index;
    private int __max_index;

    public GameObject UIOptions;
    private GameObject handlerUIOptions;


    // Start is called before the first frame update
    void Start()
    {
        __selectables =  GetComponentsInChildren<Text>();
        __select_index = 0;
        __max_index = __selectables.Length - 1;
        is_active = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (is_active)
        {
            var right   = Input.GetButtonDown("Right")   ;
            var left    = Input.GetButtonDown("Left")    ;
            var enter   = Input.GetButtonDown("Submit");


            if (left)
                __select_index = ( __select_index <= 0 ) ? 
                                    __max_index : 
                                    __select_index - 1;
            if ( right ) 
                __select_index = ( __select_index >= __max_index ) ? 
                                    0 : 
                                    __select_index + 1;

            if ( enter )
                doAction();
        }

        refreshUI();
    }

    private void refreshUI()
    {
        for ( int i = 0; i < __selectables.Length ; i++ )
        {
            if ( i == __select_index )
                __selectables[i].color = selected_color;
            else
                __selectables[i].color = unselected_color;
        }
    }

    private void doAction()
    {
        switch (__select_index)
        {
            case 0:
                loadGame();
                break;
            case 1:

                loadOptions();
                break;
            default:
                break;
        }
        return;
    }

    private void loadGame()
    {
        SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
    }

    private void loadOptions()
    {
        if (!!UIOptions)
        {
            handlerUIOptions = Instantiate(UIOptions);
            UIMenu as_UIMenu = handlerUIOptions.GetComponent<UIMenu>();
            if (!!as_UIMenu)
            {
                as_UIMenu.subscribe(this);
            }
            else    
                Debug.LogError("UITitleScreen : Handler expected to carry UIMenu. Check input GameObject.");
        }
    }

}
