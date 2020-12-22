using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;


[System.Serializable]
public class Looper : MonoBehaviour, ISavable {
    public PlayerController PC;
    // StartPosition is the position from which the record started
    public Vector2 StartPosition;
    public class Model : IModel
    {
        // Events are the recorded events that happened during a recording
        // Be careful that the Event 0 is the first event to happen, therefor a Tick
        // will be the NONE direction applied to the first tick
        public bool IsPaused = false;
        public bool IsRunning = false;
        public bool IsRecording = false;
        // CurrentIdx is maintained by WorldManager... Yes, I know...
        public int CurrentIdx;
    }

    IModel ISavable.GetModel()
    {
        return Data;
    }
    public Model Data = new Model();


    /// <summary>
    /// Reset will put back invariant but most notably will reset the position of the object to the
    /// StartPosition
    /// </summary>
    public void Reset()
    {
        PC.gameObject.transform.position = new Vector3(StartPosition.x, StartPosition.y, 0);
        PC.GetComponentInChildren<SpriteRenderer>().transform.localPosition = Vector3.zero;
        Data.IsRunning = false;
        Data.IsPaused = false;
        Data.IsRecording = false;
        Data.CurrentIdx = 0;
    }
    /// <summary>
    /// Only call Reset for now
    /// </summary>
    public void StartRunning()
    {
        Reset();
        Data.IsRunning = true;

    }
    /// <summary>
    /// Only call reset for now
    /// </summary>
    public void StopRunning()
    {
        Reset();

    }
    /// <summary>
    /// Will pause the loop if running
    /// </summary>
    public void TogglePause()
    {
        if (Data.IsRunning) 
            Data.IsPaused = !Data.IsPaused;
    }
    /// <summary>
    /// Start a recording
    /// It will update the invariant and treset to the StartPosition
    /// </summary>
    public void StartRecording()
    {
        StartPosition = PC.gameObject.transform.position;
        Data.IsRecording = true;
        Data.IsPaused = false;
        Data.IsRunning = false;

    }
    /// <summary>
    /// Update the invariant
    /// </summary>
    public void StopRecording()
    {
        Data.IsRecording = false;
        Data.IsRunning = false;
        Data.IsPaused = false;

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