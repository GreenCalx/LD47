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
                //updateEnergyPanels( ec.getCurrentCell() );
            }
        }
        else { Debug.Log("UIEnergy : Player ref is missing"); }
    }

    public void updateEnergyPanels( EnergyCell iCell)
    {

        if (iCell == null)
        { Debug.Log("ui cell null");return;}

        UIEnergyPanel[] energy_panels = GetComponentsInChildren<UIEnergyPanel>();
        int n_panels = energy_panels.Length;

        int disabled_energy     = iCell.getDepleted();
        int remaining_energy    = iCell.getEnergy();
        bool hasEnergyLeft      = remaining_energy > 0;
        int max_capacity        = iCell.max_energy;
        int used_energy         = max_capacity - remaining_energy - disabled_energy;

        List<bool> cell_energy_status = new List<bool>();
        for ( int i=0; i < disabled_energy; i++)
        { 
            cell_energy_status.Add(true);
        }
        for ( int i=0; i <= 5-disabled_energy; i++)
        { 
            cell_energy_status.Add(true);
        }
        for ( int i=1; i <= used_energy; i++)
        { 
            cell_energy_status[cell_energy_status.Count - i] = false;
        }

        List<bool> disabled_energies = new List<bool>(n_panels); // false is default val
        for ( int i=0; i < disabled_energy; i++)
        { disabled_energies.Add(true); }
        for ( int i=disabled_energy; i < n_panels; i++)
        { disabled_energies.Add(false); }

        for (int i=0; i < n_panels; i++)
        {
            Image im = energy_panels[i].gameObject.GetComponent<Image>();
            var new_color = im.color;

            bool can_consume = cell_energy_status[i];
            bool is_disabled = disabled_energies[i];
            if (!can_consume)
            {
                new_color = UnityEngine.Color.red;
            }
            else {
                if (is_disabled)
                    new_color = UnityEngine.Color.black;
                else
                    new_color = UnityEngine.Color.white;
            }
            im.color = new_color;
        }
    }

    public void updateEnergyPanels( int iPlayerEnergy, int iDisabledEnergy)
    {
        UIEnergyPanel[] energy_panels = GetComponentsInChildren<UIEnergyPanel>();
         int n_panels = energy_panels.Length;
       /* int remaining_energy_to_display = iPlayerEnergy;
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
        } */

        List<bool> disabled_energies = new List<bool>(n_panels); // false is default val
        int n_disabled = 0;
        for ( int i=0; i < iDisabledEnergy; i++)
        { disabled_energies.Add(true); n_disabled++; }
        for ( int i=n_disabled; i < n_panels; i++)
        { disabled_energies.Add(false); }
        Debug.Log("n_disabled " + n_disabled);
        
        List<bool> available_energies = new List<bool>( n_panels );

        for ( int i=0; i < n_disabled; i++)
        {
            available_energies.Add(false);
        }
        for ( int i=iPlayerEnergy; i > 0 ; i--)
        {   
            available_energies.Add(true);
        }
        for ( int i=available_energies.Count; i < n_panels; i++)
        {   
            available_energies.Add(false);
        }
        

        // need 3rd state to pop 3rd state with bool ( otherwise F&&F equivalent T&&T issue )
        List<bool> used_energies = new List<bool>( n_panels );
        for ( int i=0; i < available_energies.Count; i++)
        {
            bool is_used = ( ( available_energies[i] & disabled_energies[i] ) || ( !available_energies[i] & !disabled_energies[i] ) );
            used_energies.Add(is_used);
        } 

        bool all_is_consumed = ( iPlayerEnergy < 0  ); // useless rn
        
        for (int i=0; i < n_panels; i++)
        {
            Image im = energy_panels[i].gameObject.GetComponent<Image>();
            var new_color = im.color;
            if (!!im)
            {
                if (all_is_consumed)
                {
                    Debug.Log("all si consumed");
                    new_color = new_color = UnityEngine.Color.white;
                    im.color = new_color;
                    continue;
                }

                bool is_available   = available_energies[i];
                bool is_disabled    = disabled_energies[i];
                bool is_used        = used_energies[i];

/*                 if ( is_disabled && is_used)
                { new_color = UnityEngine.Color.red; }
                else if ( (is_used && !is_available) ||
                     (is_used && !is_available)  )
                { new_color = UnityEngine.Color.red; }
                else if ( !is_used && is_disabled && is_available)
                    new_color = UnityEngine.Color.black;
                else
                    new_color = UnityEngine.Color.white; */
                    if (is_used)
                    {
                        new_color = UnityEngine.Color.red;
                    }else if (is_disabled)
                    {
                        new_color = UnityEngine.Color.black;
                    } else
                    {
                        new_color = UnityEngine.Color.white;
                    }

                im.color = new_color;
            }
        }//!for panels

    }
}
