using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCounter
{
    // Has MAX_ENERY to spend MAX_REPLENISH times
    public int MAX_REPLENISH;
    public int MAX_ENERGY;
    public int replenish;
    public int energy;

    // Start is called before the first frame update
    public EnergyCounter()
    {
        refillAll();
    }

    public EnergyCounter( int iMaxEnergy, int iMaxReplenish)
    {
        MAX_ENERGY      = iMaxEnergy;
        MAX_REPLENISH   = iMaxReplenish;
        refillAll();
    }

    public void refillAll()
    {
        replenish   = MAX_REPLENISH;
        energy      = MAX_ENERGY;
    }

    public void refillEnergy()
    {
        energy = MAX_ENERGY;
    }

    public void refillReplenish()
    {
        replenish  = MAX_REPLENISH;
    }

    public EnergyCounter getNestedCounter()
    {
        return new EnergyCounter( energy-1, replenish);
    }
    public EnergyCounter getNestedCounter( int iEnergyRemoval, int iReplenishRemoval)
    {
        return new EnergyCounter( energy-iEnergyRemoval, replenish-iReplenishRemoval);
    }

    public bool tryConsume()
    {
        if ( energy <= 0 )
        {
            if ( replenish <= 0 )
            {
                // NO MORE MOVES, consume failed
                return false;
            }
            else {
                replenish--;
                refillEnergy();
            }
        } else {
            energy--;
        }
        return true;
    }
}
