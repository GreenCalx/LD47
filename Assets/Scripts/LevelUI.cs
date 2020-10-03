using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{

    public GameObject playerRef;
    public GameObject[] energy_panels;
    public GameObject   replenish_label;

    private PlayerController playerController = null;
    private bool hasPlayerRef = false;

    private Text ui_replenish_lbl;


    // Start is called before the first frame update
    void Start()
    {
        if ( playerRef != null)
        {
            playerController = playerRef.GetComponent<PlayerController>();
            hasPlayerRef = (playerController != null);
            
        }
        ui_replenish_lbl = replenish_label.GetComponent<Text>();
        
    }

    // Update is called once per frame
    void Update()
    {
        refresh();
    }

    public void refresh()
    {
        if (!!hasPlayerRef)
        {
            // TODO : get counter from player
            EnergyCounter ec = playerController.energyCounter;
            if (ec != null)
            {
                ui_replenish_lbl.text   = "" + ec.replenish;
                updateEnergyPanels(ec.energy);
                
            }
        }
    }

    public void updateEnergyPanels( int iPlayerEnergy)
    {
        for (int i=0; i < energy_panels.Length; i++)
        {
            
        }
    }
}
