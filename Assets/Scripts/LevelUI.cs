using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [Range(0, 1f)] [SerializeField] public float energy_disabled_alpha = 0.5f;
    [Range(0, 1f)] [SerializeField] public float energy_enabled_alpha = 1f;
    public GameObject playerRef;
    public GameObject[] energy_panels;
    public GameObject   replenish_label;

    private PlayerController playerController = null;
    private bool hasPlayerRef = false;

    private Text ui_replenish_lbl;

    private const string ENERGY_PANEL_PREFIX = "ENERGY";

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
        //refresh();
    }

    public void refresh()
    {
        if (!!hasPlayerRef)
        {
            EnergyCounter ec = playerController.energyCounter;
            if (ec != null)
            {
                ui_replenish_lbl.text   = "" + ec.getReplenish();
                updateEnergyPanels(ec.getEnergy());
            }
        }
        else { Debug.Log("UIEnergy : Player ref is missing"); }
    }

    public void updateEnergyPanels( int iPlayerEnergy)
    {
        UIEnergyPanel[] energy_panels = GetComponentsInChildren<UIEnergyPanel>();
        for (int i=0; i < energy_panels.Length; i++)
        {
            Image im = energy_panels[i].gameObject.GetComponent<Image>();
            if (!!im)
            {
                var new_color = im.color;
                new_color = ( i >= iPlayerEnergy ) ? UnityEngine.Color.red : UnityEngine.Color.white;
                im.color = new_color;
            }
        }
    }
}
