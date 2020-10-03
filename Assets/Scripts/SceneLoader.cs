using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string scene_to_load;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool key_pressed = Input.anyKey;
        if (key_pressed)
            SceneManager.LoadScene( scene_to_load, LoadSceneMode.Single);
    }
}
