using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCell
{
    public int max_energy;
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
        return ( --energy >= 0 );
    }
    public int getEnergy()
    { return (energy>=0) ? energy : 0; }
    public int getDepleted()
    {
        return (n_disabled_energy>=0) ? n_disabled_energy : 0;
    }
    public void setEnergy( int iEnergy)
    { energy = ( iEnergy > max_energy ) ? max_energy : iEnergy; }
    public void setDepleted(int iEnergy)
    { n_disabled_energy = iEnergy; }
    public bool tryConsumeDepletedEnergy()
    {
        return (--n_disabled_energy >= 0);
    } 
}//! EnergyCell

public class EnergyCounter
{
    // Has MAX_ENERY to spend MAX_REPLENISH times
    public int MAX_REPLENISH;
    public int N_DISABLED_ENERGY;
    public int MAX_ENERGY;


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

    public EnergyCounter(int iMaxEnergy, int iDisabledEnergy, int iMaxReplenish, int iCurrentReplenish)
    {
        MAX_ENERGY = iMaxEnergy;
        MAX_REPLENISH = iMaxReplenish;
        N_DISABLED_ENERGY = iDisabledEnergy;
        refillAllCells();

        var ToRemoveCells = iMaxReplenish - iCurrentReplenish;
        for (int i = 0; i < ToRemoveCells; ++i)
        {
            eCells.RemoveAt(0);
        }
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
        EnergyCounter ec =  new EnergyCounter( MAX_ENERGY-1, N_DISABLED_ENERGY+1, MAX_REPLENISH, n_replenishes);
        Debug.Log(" NESTED CPT CREATED WITH : " + ec.MAX_ENERGY + " energy for " + ec.MAX_REPLENISH + " cells and " + ec.N_DISABLED_ENERGY + " disabled energy.");

        // update curr cell in new ec to match current cell consuming
        if ( (getReplenish() > 0) && (ec.eCells.Count > 0) )
        {
            int remaining_energy = getEnergy();
            int remaining_depleted = getDepleted();
            Debug.Log("remaining energy" + remaining_energy);
            EnergyCell cur_cell = ec.eCells[0];
            cur_cell.setEnergy(remaining_energy-1);
            cur_cell.setDepleted( (cur_cell.getEnergy() != 0 ? remaining_depleted+1 : remaining_depleted));
        }

        return ec;
    }

    public bool isCurrentCellEnergyLocked()
    {
        if ( eCells.Count > 0 )
        {
            EnergyCell curr_cell = eCells[0];
            has_locked_energy = curr_cell.tryConsumeDepletedEnergy();
        }
        return has_locked_energy;
    }

    public EnergyCell getCurrentCell()
    {
        if ( eCells.Count > 0 )
        {
            EnergyCell curr_cell = eCells[0];
            return curr_cell;
        } 
        return null;
    }

    public int getEnergy()
    {
        return  ( eCells.Count > 0 ) ? eCells[0].getEnergy() : 0;
    }

    public int getReplenish()   
    {
        return ( eCells.Count > 0 ) ? eCells.Count : 0;
    }

    public int getDepleted()
    {
        return (eCells.Count > 0) ? eCells[0].getDepleted() : 0;
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
            if (curr_cell.tryConsume()) return true;
            if (isCurrentCellEnergyLocked()) return true;
            // cell depleted, remove from cell list
            // should procede to lmove elements ( API doc ) 
            eCells.RemoveAt(0);
            return tryConsume();
        }
        return false; // no more energy cells
    }
}
