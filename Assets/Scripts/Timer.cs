using System;
/// <summary>
/// This class is used as a Timer in any unity script that might needs it.
/// It can be seen as a state machine updating its states when the current time changes.
/// NOTE(mtn5): It is not using anything related to unity directly and is using floating point as seconds.
///
/// IMPORTANT(mtn5): Caller has to take care of the timer update by providing how much time has passed.
/// Then everything about internal states is taken care of by this class.
/// Example usage :
///    Timer t = new Timer( myEndTime )
///    ... In update function ...
///    t.Update( Time.deltaTime );
///    if ( t.Ended() ) ... do something ...
/// 
/// </summary>
[Serializable]
public class Timer {
    /// <summary>
    /// Variables
    /// </summary>
    private float _CurrentTime = 0;
    private float _EndTime = -1;
    /// <summary>
    /// States
    /// </summary>
    private bool _AutoRestart = false;
    private bool _Ended = false;
    private enum States { eRunning, ePaused, eStopped };
    private States _CurrentState = States.eStopped;
    /// <summary>
    /// Constructor taking in the desired end time of the timer
    /// </summary>
    /// <param name="Time">desired end time in seconds</param>
    public Timer( float Time )
    {
        _EndTime = Time;
    }
    /// <summary>
    /// Start the timer
    /// IMPORTANT(mtn5): it will start the timer from the current time
    /// if you want to start the timer from 0, see Restart or Reset
    /// </summary>
    public void Start()
    {
        _CurrentState = States.eRunning;
    }
    /// <summary>
    /// Update the internal time by adding the input to the current time
    /// It will also reset the ended state.
    /// This function is expected to be called ONCE in a frame.
    /// </summary>
    /// <param name="Time">deltaTime in seconds</param>
    public void Update(float Time)
    {
        if (!IsRunning()) return;

        _CurrentTime += Time;
        _UpdateStates();
    }
    /// <summary>
    /// Pause the timer, will not update its time
    /// </summary>
    public void Pause()
    {
        _CurrentState = States.ePaused;
    }
    /// <summary>
    /// Reset the timer to its beginning state
    /// </summary>
    public void Reset()
    {
        _CurrentState = States.eStopped;
        _CurrentTime = 0;
    }
    /// <summary>
    /// Will reset the timer and start it
    /// </summary>
    public void Restart()
    {
        Reset();
        Start();
    }
    /// <summary>
    /// This function returns if the timer has completed during this frame.
    /// IMPORTANT(mtn5): Be careful that this state will be true only one frame!
    /// </summary>
    /// <returns>Has the timer completed during the frame</returns>
    public bool Ended()
    {
        return _Ended;
    }
    /// <summary>
    /// Is the timer currently running?
    /// </summary>
    /// <returns> True if running, else false. Paused is NOT running</returns>
    public bool IsRunning()
    {
        return (_CurrentState == States.eRunning);
    }
    /// <summary>
    /// Current time of the timer. Be careful zith this as you don't know when the update might be called and change this value.
    /// </summary>
    /// <returns>The current time between 0 and Length</returns>
    public float GetTime()
    {
        return _CurrentTime;
    }
    /// <summary>
    /// Length of the timer, meaning how much time before it is considered Ended and done
    /// </summary>
    /// <returns>The desired length of the timer</returns>
    public float Length()
    {
        return _EndTime;
    }
    /// <summary>
    /// If you ever need to change the desired length of the timer.
    /// Be careful that it will reset the timer if you try to set a length lower than the current time
    /// </summary>
    /// <param name="Time">Desired new length of the timer</param>
    public void SetEndTime(float Time)
    {
        _EndTime = Time;
        _UpdateStates();
    }
    /// <summary>
    /// Update the state machine
    /// </summary>
    private void _UpdateStates()
    {
        switch (_CurrentState)
        {
            case States.eRunning:
                if (_CurrentTime >= _EndTime)
                {
                    _CurrentState = States.eStopped;
                    _Ended = true;
                    if (_AutoRestart) Restart();
                }
                else
                {
                    _Ended = false;
                }
            break;
            case States.ePaused:
            case States.eStopped:
                if (_EndTime < _CurrentTime) Reset();
            break;
        }
    }
}
