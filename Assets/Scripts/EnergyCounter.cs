using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCounter
{
    // Has MAX_ENERY to spend MAX_REPLENISH times
    public int MAX_REPLENISH;
    public int MAX_ENERGY;

    public class EnergyCell
    {
        int max_energy;
        int energy;
        public EnergyCell( int iMaxEnergy )
        {
            max_energy = iMaxEnergy;
            energy = 0;
            refill();
        }
        public void refill()
        { energy = max_energy; }
        public bool tryConsume()
        { return (--energy > 0); }
        public int getEnergy()
        { return energy; }
        public void setAvailableEnergy( int iEnergy)
        { energy = iEnergy; }
    }//! EnergyCell
    public List<EnergyCell> eCells;

    // Start is called before the first frame update
    public EnergyCounter()
    {
        refillAllCells();
    }

    public EnergyCounter( int iMaxEnergy, int iMaxReplenish)
    {
        MAX_ENERGY      = iMaxEnergy;
        MAX_REPLENISH   = iMaxReplenish;
        refillAllCells();
    }

    public void refillAllCells()
    {

        eCells = new List<EnergyCell>(MAX_REPLENISH);
        for ( int i = 0; i < MAX_REPLENISH; i++)
        {
            eCells.Add( new EnergyCell(MAX_ENERGY) );
        }

    }

    public EnergyCounter getNestedCounter()
    {
        // update replenishes according to number used in cur loop
        int delta_replenishes = MAX_REPLENISH - getReplenish();

        // get counter with -1 energy overall
        EnergyCounter ec =  new EnergyCounter( MAX_ENERGY-1, delta_replenishes);
        Debug.Log(" NESTED CPT CREATED WITH : " + (MAX_ENERGY-1) + " energy for " + delta_replenishes + " cells");

        // update last cell in new ec to match current cell consuming
        if ( getReplenish() > 0 )
        {
            int remaining_energy = getEnergy();
            EnergyCell last_cell_of_new_cpt = ec.getLastEnergyCell();
            last_cell_of_new_cpt.setAvailableEnergy(remaining_energy);
        }

        return ec;
    }

    public int getEnergy()
    {
        return  ( eCells.Count > 0 ) ? eCells[0].getEnergy() : 0;
    }

    public int getReplenish()
    {
        return ( eCells.Count > 0 ) ? eCells.Count : 0;
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
                // cell depleted, remove from cell list
                // should procede to lmove elements ( API doc ) 
                eCells.RemoveAt(0);
                if ( eCells.Count == 0 )
                {
                    return false; // no more energy cells
                }
            }
            return true;
        }
        return false; // no more energy cells
    }
}
