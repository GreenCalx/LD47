using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitTile : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var PC = other.gameObject.GetComponent<PlayerController>();
        if ( PC)
        {
            SceneManager.LoadScene( "LevelSelectorScene", LoadSceneMode.Single);
        }
    }
}
