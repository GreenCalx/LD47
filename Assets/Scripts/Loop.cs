using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class Looper : MonoBehaviour
{
    // Needed to update startposition
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
    public bool IsRecording = true;
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
    }
    /// <summary>
    /// Only call Reset for now
    /// </summary>
    public void Start()
    {
        Reset();
    }
    /// <summary>
    /// Only call reset for now
    /// </summary>
    public void Stop()
    {
        Reset();
    }
    /// <summary>
    /// Start a recording
    /// It will update the invariant 
    /// </summary>
    public void StartRecord()
    {
        StartPosition = PC.gameObject.transform.position;
        IsRecording = true;
        IsPaused = false;
        IsRunning = false;
    }

    public void EndRecord()
    {
        IsRecording = false;
    }

    public void Update()
    {
        var Start = Input.GetKeyDown(KeyCode.A);
        var Stop = Input.GetKeyDown(KeyCode.Z);
        var Pause = Input.GetKeyDown(KeyCode.E);
        var Restart = Input.GetKeyDown(KeyCode.R);
        var Record = Input.GetKeyDown(KeyCode.T);
        var StopRecord = Input.GetKeyDown(KeyCode.Y);

        if (L.IsRunning)
        {

            if (Stop)
            {
                IsLoopedControled = false;
                L.IsRunning = false;
            }
            if (Pause) L.IsPaused = true;
        }
        else
        {
            if (Start)
            {
                L.IsRunning = true;
                IsLoopedControled = true;
            }
        }

        if (L.IsRecording)
        {
            if (StopRecord) L.IsRecording = false;
        }
        else
        {
            if (Record)
            {
                L.IsRecording = true;
                L.StartPosition = this.gameObject.transform.position;
            }
        }
    }
}


#if false
public class Loop : MonoBehaviour
    {
        public PlayerController Player;
        public Vector2 StartPosition;
        public class ListItem
        {
            public ListItem(PlayerController.Direction D, float T) { Direction = D; DirectionTime = T; }
            public PlayerController.Direction Direction;
            public float                      DirectionTime;
        }

        List<ListItem> Events = new List<ListItem>();
        int CurrentEvent = 0;
        public bool IsPaused = false;
        public bool IsRunning = false;
        public bool IsRecording = false;
        float CurrentTime = 0;

        void Start()
        {
            CurrentEvent = 0;
            CurrentTime = 0;
        }

        public void Reset()
        {
            Events = new List<ListItem>();
            CurrentEvent = 0;
            CurrentTime = 0;
            IsPaused = false;
            IsRunning = false;
            IsRecording = false;
        }

        void Update()
        {
            var Start = Input.GetKeyDown(KeyCode.A);
            var Stop = Input.GetKeyDown(KeyCode.Z);
            var Pause = Input.GetKeyDown(KeyCode.E);
            var Restart = Input.GetKeyDown(KeyCode.R);
            var Record = Input.GetKeyDown(KeyCode.T);
            var StopRecord = Input.GetKeyDown(KeyCode.Y);

            if(IsRunning)
            {
                if (Stop) IsRunning = false;
                if (Pause) IsPaused = true;
            } else
            {
                if (Start)
                {
                    IsRunning = true;
                    //Player.SetPosition(StartPosition);
                }
            }
            
            if(IsRecording)
            {
                if (StopRecord) IsRecording = false;
            } else
            {
                if (Record)
                { 
                   IsRecording = true;
                    StartPosition = Player.transform.position;
                }
            }


            if (!IsRecording)
            {
                if (Restart)
                {
                    CurrentEvent = 0;
                    //Player.SetPosition(StartPosition);
                }

                if (IsRunning)
                {
                    if (!IsPaused)
                    {
                        CurrentTime += Time.deltaTime;

                        Player.IsLoopedControled = true;
                        Player.CurrentDirection = Events[CurrentEvent].Direction;

                        if (CurrentTime >= Events[CurrentEvent].DirectionTime)
                        {
                            CurrentEvent += 1;
                            CurrentTime = 0;
                            if (CurrentEvent > Events.Count-1)
                            {
                                CurrentEvent = 0;
                                CurrentTime = 0;
                                //Player.SetPosition(StartPosition);
                            }
                        }
                    }
                } else
                {
                    Player.IsLoopedControled = false;
                }
            }
            else
            {
                if(Events.Count == 0) Events.Add(new ListItem(Player.CurrentDirection, 0));

                Events[Events.Count-1].DirectionTime += Time.deltaTime;
                if(Player.CurrentDirection != Events[Events.Count-1].Direction)
                {
                    Events.Add(new ListItem(Player.CurrentDirection, 0));
                }
            }

        }
    }

#endif
