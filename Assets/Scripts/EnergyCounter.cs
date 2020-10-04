using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCounter
{
    // Has MAX_ENERY to spend MAX_REPLENISH times
    public int MAX_REPLENISH;
    public int N_DISABLED_ENERGY;
    public int MAX_ENERGY;

    public class EnergyCell
    {
        int max_energy;
        int energy;
        int n_disabled_energy;
        public EnergyCell( int iMaxEnergy, int iDisabledEnergy )
        {
            max_energy = iMaxEnergy;
            n_disabled_energy = iDisabledEnergy;
            energy = 0;
            refill();
        }
        public void refill()
        { energy = max_energy; }
        public bool tryConsume()
        { 
            //if ( energy > 0 )
            //{
            //  energy--;
            //  return true;
           //}
            return ( --energy > 0 );
        }
        public int getEnergy()
        { return energy; }
        public void setAvailableEnergy( int iEnergy)
        { energy = ( iEnergy > max_energy ) ? max_energy : iEnergy; }

        public int getRemainingDepletedEnergy()
        {
            if ( (energy == 0) && (n_disabled_energy > 0))
                return (n_disabled_energy--);
            else
                return 0;
        } 
    }//! EnergyCell
    public List<EnergyCell> eCells;
    public bool has_locked_energy = false;

    // Start is called before the first frame update
    public EnergyCounter()
    {
        refillAllCells();
    }

    public EnergyCounter( int iMaxEnergy, int iMaxReplenish)
    {
        MAX_ENERGY      = iMaxEnergy;
        MAX_REPLENISH   = iMaxReplenish;
        N_DISABLED_ENERGY = 0;
        refillAllCells();
    }

    public EnergyCounter( int iMaxEnergy, int iDisabledEnergy, int iMaxReplenish)
    {
        MAX_ENERGY      = iMaxEnergy;
        MAX_REPLENISH   = iMaxReplenish;
        N_DISABLED_ENERGY = iDisabledEnergy;
        refillAllCells();
    }

    public void refillAllCells()
    {

        eCells = new List<EnergyCell>(MAX_REPLENISH);
        for ( int i = 0; i < MAX_REPLENISH; i++)
        {
            eCells.Add( new EnergyCell(MAX_ENERGY, N_DISABLED_ENERGY) );
        }

    }

    public EnergyCounter getNestedCounter()
    {
        // update replenishes according to number used in cur loop
        int delta_replenishes = MAX_REPLENISH - getReplenish();
        int n_replenishes = MAX_REPLENISH - delta_replenishes;
        // get counter with -1 energy overall
        EnergyCounter ec =  new EnergyCounter( MAX_ENERGY-1, N_DISABLED_ENERGY+1, n_replenishes);
        Debug.Log(" NESTED CPT CREATED WITH : " + ec.MAX_ENERGY + " energy for " + ec.MAX_REPLENISH + " cells and " + ec.N_DISABLED_ENERGY + " disabled energy.");

        // update curr cell in new ec to match current cell consuming
        if ( (getReplenish() > 0) && (ec.eCells.Count > 0) )
        {
            int remaining_energy = getEnergy();
            Debug.Log("remaining energy" + remaining_energy);
            EnergyCell cur_cell = ec.eCells[0];
            cur_cell.setAvailableEnergy(remaining_energy);
        }

        return ec;
    }

    public bool isCurrentCellEnergyLocked()
    {
        if ( eCells.Count > 0 )
        {
            EnergyCell curr_cell = eCells[0];
            int remaining_depleted_energy = curr_cell.getRemainingDepletedEnergy();
            Debug.Log("remaining_depleted_energy : " + remaining_depleted_energy);
            has_locked_energy = (remaining_depleted_energy > 0);
        }
        return has_locked_energy;
    }

    public int getEnergy()
    {
        return  ( eCells.Count > 0 ) ? eCells[0].getEnergy() : 0;
    }

    public int getReplenish()   
    {
        return ( eCells.Count > 0 ) ? eCells.Count : 0;
    }

    public int getDisabledEnergy()
    {
        return N_DISABLED_ENERGY;
    }

    public EnergyCell getLastEnergyCell()
    {
        int lastcell_idx = ( (eCells.Count-1) >= 0 ) ? (eCells.Count-1) : 0;
        return eCells[lastcell_idx];
    }



    public bool tryConsume()
    {
        
        if ( eCells.Count > 0 )
        {
            EnergyCell curr_cell = eCells[0];
            if ( !curr_cell.tryConsume() )
            {       
                has_locked_energy = isCurrentCellEnergyLocked();

                if (!has_locked_energy)
                {
                    // cell depleted, remove from cell list
                    // should procede to lmove elements ( API doc ) 
                    eCells.RemoveAt(0);
                }
            }
            return true;
        }
        return false; // no more energy cells
    }
}
