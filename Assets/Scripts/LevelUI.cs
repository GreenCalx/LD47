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

    public void updatePlayerRef( GameObject newRef)
    {
        if (newRef == null)
            return;
        playerRef = newRef;
        if ( playerRef != null)
        {
            playerController = playerRef.GetComponent<PlayerController>();
            hasPlayerRef = (playerController != null);
            
        }
    }

    public void refresh()
    {
        if (!!hasPlayerRef)
        {
            EnergyCounter ec = playerController.energyCounter;
            if (ec != null)
            {
                ui_replenish_lbl.text   = "" + (ec.getReplenish()-1); // has max cell
                updateEnergyPanels( ec.getEnergy(), ec.getDisabledEnergy() );
            }
        }
        else { Debug.Log("UIEnergy : Player ref is missing"); }
    }

    public void updateEnergyPanels( int iPlayerEnergy, int iDisabledEnergy)
    {
        UIEnergyPanel[] energy_panels = GetComponentsInChildren<UIEnergyPanel>();
        int n_panels = energy_panels.Length;
        int remaining_energy_to_display = iPlayerEnergy;
        for (int i=0; i < n_panels; i++)
        {
            Image im = energy_panels[i].gameObject.GetComponent<Image>();
            if (!!im)
            {
                var new_color = im.color;
                if ( i <= iDisabledEnergy-1 ) // disable
                {
                    new_color = UnityEngine.Color.black;
                } else if ( remaining_energy_to_display > 0 )
                {
                    new_color = UnityEngine.Color.white;
                    remaining_energy_to_display--;
                } else {
                    new_color = UnityEngine.Color.red;
                }
                 im.color = new_color;
            }
        }
    }
}
