using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTile : ActivatorObject
{

    public bool isSwitched = false;
    public Sprite spriteSwitched;
    private List<GameObject> objectsOnSwitch = new List<GameObject>();
    public Sprite spriteNotSwitched;
    public int SwitchTick = -1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isSwitched)
        {
            objectsOnSwitch.Add(other.gameObject);

            // register object to get called on tick
            // TODO toffa: make this work in case it is another object than player that trigger
            GameObject.Find("GameLoop").GetComponent<WorldManager>().AddListener(other.gameObject.GetComponent<PlayerController>() as ITickObserver, this);

            /*
            foreach( ActivableObject ao in activableObjects )
                ao.listen(signalKey, this, true);
            */
            pulsate(true);

            GetComponentInChildren<AudioSource>().Play();
            isSwitched = true;
            SwitchTick = GameObject.Find("GameLoop").GetComponent<WorldManager>().TL.last_tick;
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (!!sr)
                sr.sprite = spriteSwitched;
        }

        TryReset();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
    }

    public void TryReset()
    {
        int Tick = GameObject.Find("GameLoop").GetComponent<WorldManager>().TL.last_tick;
        if (Tick < SwitchTick) Reset();
    }

    public void Reset()
    {
        if (isSwitched)
        {
            /*
            foreach (ActivableObject ao in activableObjects)
                ao.listen(signalKey, this, false);
*/
            pulsate(false);
            isSwitched = false;

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (!!sr)
                sr.sprite = spriteNotSwitched;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TryReset();
       /* objectsOnSwitch.Remove(other.gameObject);

        // switch is reseted when going out of it
        // and nothing is on it anymore
        if (isSwitched && objectsOnSwitch.Count == 0)
        {
            foreach( ActivableObject ao in activableObjects )
                ao.listen(signalKey, this, false);

            isSwitched = false;

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (!!sr)
                sr.sprite = spriteNotSwitched;
        } */
    }

    void Update()
    {
        TryReset();
    }
}
