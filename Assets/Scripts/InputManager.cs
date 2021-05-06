using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IControllable
{
    void ProcessInputs(Save.InputSaver.InputSaverEntry Entry);
}

public class InputManager : MonoBehaviour
{
    private List<IControllable> _Controllees = new List<IControllable>();
    private float _LastDpadAxisHorizontal = 0;
    private float _LastDpadAxisVertical = 0;

    public enum Mode { PLAYER, TIMELINE, REPLAY, RECORD, DEACTIVATED };
    public Mode CurrentMode = Mode.DEACTIVATED;

    private bool _Lock = false;
    private List<IControllable> _DeferRemove = new List<IControllable>();
    public bool _Activated = true;

    public Save Saver;

    public void Activate() { _Activated = true; Saver.Stop(); }
    public void DeActivate() { _Activated = false; }

    public void Attach(IControllable iControllable)
    {
        if (!_Controllees.Contains(iControllable))
        {
            _Controllees.Add(iControllable);
        }
    }

    public void Detach(IControllable iControllable)
    {
        if (!_Lock)
            _Controllees.Remove(iControllable);
        else
            _DeferRemove.Add(iControllable);

    }

    private void Lock()
    {
        _Lock = true;
    }

    private void UnLock()
    {
        _Lock = false;
        foreach (IControllable C in _DeferRemove)
        {
            Detach(C);
        }
        _DeferRemove.Clear();
    }

    void Update()
    {
        if (!_Activated) return;
        // NOTE(toffa): Saver stuff test
        Save.InputSaver.InputSaverEntry Entry = new Save.InputSaver.InputSaverEntry();
        Entry.Add("Up");
        Entry.Add("Down");
        Entry.Add("Right");
        Entry.Add("Left");
        Entry.Add("Break");
        Entry.Add("Tick");
        Entry.Add("BackTick");
        Entry.Add("Restart");
        Entry.Add("DPad_Vertical", true);
        Entry.Add("DPad_Horizontal", true);
        Entry.Add("SwitchTL");
        Entry.Add("Cancel");
        Entry.Add("Submit");

        if(Saver) Entry = Saver.Tick(Entry);

        Entry.isDpadDownPressed = Entry.Inputs["DPad_Vertical"].AxisValue == 1 && _LastDpadAxisVertical != 1;
        Entry.isDpadUpPressed = Entry.Inputs["DPad_Vertical"].AxisValue == -1 && _LastDpadAxisVertical != -1;
        Entry.isDpadLeftPressed = Entry.Inputs["DPad_Horizontal"].AxisValue == 1 && _LastDpadAxisHorizontal != 1;
        Entry.isDpadRightPressed = Entry.Inputs["DPad_Horizontal"].AxisValue == -1 && _LastDpadAxisHorizontal != -1;

        _LastDpadAxisHorizontal = Entry.Inputs["DPad_Horizontal"].AxisValue;
        _LastDpadAxisVertical = Entry.Inputs["DPad_Vertical"].AxisValue;

        // Set mode according to inputs

        // IMPORTANT toffa : the collection can change as process inputs could attach or detach gameobjects
        // so we must use vanilla for and not foreach
        // we could also copy the collection before calling process inputs, but I guess we want to processInputs
        // of newly attached object in this frame and not the next one
        var EndIdx = _Controllees.Count;
        Lock();
        for (int i = 0; i < _Controllees.Count; ++i)
        {
            _Controllees[i].ProcessInputs(Entry);
        }
        UnLock();
    }

}

