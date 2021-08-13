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

            /*
            foreach( ActivableObject ao in activableObjects )
                ao.listen(signalKey, this, true);
            */
            sendPulse();

            GetComponentInChildren<AudioSource>().Play();
            isSwitched = true;
            SwitchTick = GameObject.Find("GameLoop").GetComponent<WorldManager>().Mdl.CurrentTick;
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
        int Tick = GameObject.Find("GameLoop").GetComponent<WorldManager>().Mdl.CurrentTick;
        if (Tick < SwitchTick) Reset();
    }

    public void Reset()
    {
        if (isSwitched)
        {
            foreach (ActivableObject ao in activableObjects)
                ao.listen(signalKey, this, false);

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
