using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Looper : MonoBehaviour
{
    // Needed to update some of PC state like:
    // Position
    // IsLoopControlled
    public PlayerController PC;
    // StartPosition is the position from which the record started
    public Vector2 StartPosition;
    // Events are the recorded events that happened during a recording
    // Be careful that the Event 0 is the first event to happen, therefor a Tick
    // will be the NONE direction applied to the first tick
    public List<PlayerController.Direction> Events = new List<PlayerController.Direction>();
    /// <summary>
    /// States
    /// </summary>
    public bool IsPaused = false;
    public bool IsRunning = false;
    public bool IsRecording = false;
    // CurrentIdx is maintained by WorldManager... Yes, I know...
    public int CurrentIdx;

    /// <summary>
    /// This is called from PlayerController to get the current tick direction
    /// </summary>
    /// <returns></returns>
    public PlayerController.Direction Tick()
    {
        // If we asked for a Tick that is greater than current loop size, we simply return the None move
        if (CurrentIdx >= Events.Count) return PlayerController.Direction.NONE;
        else return Events[CurrentIdx];
    }
    /// <summary>
    /// Reset will put back invariant but most notably will reset the position of the object to the
    /// StartPosition
    /// </summary>
    public void Reset()
    {
        PC.gameObject.transform.position = new Vector3(StartPosition.x, StartPosition.y, 0);
        PC.GetComponentInChildren<SpriteRenderer>().transform.localPosition = Vector3.zero;
        IsRunning = false;
        IsPaused = false;
        IsRecording = false;
        CurrentIdx = 0;
    }
    /// <summary>
    /// Only call Reset for now
    /// </summary>
    public void StartRunning()
    {
        Reset();
        IsRunning = true;

        PC.IsLoopedControled = true;
        PC.WM.WaitForInput = false;
    }
    /// <summary>
    /// Only call reset for now
    /// </summary>
    public void StopRunning()
    {
        Reset();

        PC.IsLoopedControled = false;
    }
    /// <summary>
    /// Will pause the loop if running
    /// </summary>
    public void TogglePause()
    {
        if (IsRunning) 
            IsPaused = !IsPaused;
    }
    /// <summary>
    /// Start a recording
    /// It will update the invariant and treset to the StartPosition
    /// </summary>
    public void StartRecording()
    {
        StartPosition = PC.gameObject.transform.position;
        IsRecording = true;
        IsPaused = false;
        IsRunning = false;

        PC.IsLoopedControled = false;
    }
    /// <summary>
    /// Update the invariant
    /// </summary>
    public void StopRecording()
    {
        IsRecording = false;
        IsRunning = false;
        IsPaused = false;

        PC.IsLoopedControled = false;
    }
    /// <summary>
    /// 
    /// </summary>
    public void ReStart()
    {
        Reset(); // double reset?
        StartRunning();
    }
    /// <summary>
    /// Everything related to inputs are here.
    /// For now almost everything is mapped to an input to Start, Stop, Record, etc
    /// </summary>
    public void Update()
    {
    }
} 