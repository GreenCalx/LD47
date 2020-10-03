using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class LevelSelectorUI : MonoBehaviour, IPointerClickHandler
{
    private const string LEVEL_NAME_PREFIX = "LEVEL";
    [SerializeField] public int level_to_load;
    public void OnPointerClick(PointerEventData pointerEvenData)
    {
        if (pointerEvenData.button == PointerEventData.InputButton.Left)
        {
            string scene_to_load = LEVEL_NAME_PREFIX + level_to_load ;
            Debug.Log("Load level :" + scene_to_load);
            SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
        }
    }
}
