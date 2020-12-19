using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

sealed class Vector3SerializationSurrogate : ISerializationSurrogate
{
    // Method called to serialize a Vector3 object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        obj = v3;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}


sealed class QuaternionSerializationSurrogate : ISerializationSurrogate
{
    // Method called to serialize a Quaternion object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        Quaternion v3 = (Quaternion)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
        info.AddValue("w", v3.w);
    }

    // Method called to deserialize a Quaternion object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        Quaternion v3 = (Quaternion)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        v3.w = (float)info.GetValue("w", typeof(float));
        obj = v3;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}


sealed class ColorSerializationSurrogate : ISerializationSurrogate
{
    // Method called to serialize a Color object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        Color v3 = (Color)obj;
        info.AddValue("r", v3.r);
        info.AddValue("g", v3.g);
        info.AddValue("b", v3.b);
        info.AddValue("a", v3.a);
    }

    // Method called to deserialize a Color object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        Color v3 = (Color)obj;
        v3.r = (float)info.GetValue("r", typeof(float));
        v3.g = (float)info.GetValue("g", typeof(float));
        v3.b = (float)info.GetValue("b", typeof(float));
        v3.a = (float)info.GetValue("a", typeof(float));
        obj = v3;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}


public class Save : MonoBehaviour
{
    [Serializable]
    public class InputSaver
    {
        [Serializable]
        public class MiniObject
        {
            public Vector3 Position;
            public Vector3 LocalPosition;
            public Vector3 LocalScale;
            public Quaternion Rotation;
            public Quaternion LocalRotation;
            public String Name;

            public MiniObject(Transform T, String Name)
            {
                Position = T.position;
                Rotation = T.rotation;
                LocalPosition = T.localPosition;
                LocalRotation = T.localRotation;
                LocalScale = T.localScale;
                this.Name = Name;
            }

            public override bool Equals(object obj)
            {
                MiniObject B = (MiniObject)obj;
                return Position == B.Position &&
                Rotation == B.Rotation &&
                LocalPosition == B.LocalPosition &&
                LocalRotation == B.LocalRotation &&
                LocalScale == B.LocalScale && Name == B.Name;
            }
        }
        [Serializable]
        public class WorldManagerData {
            public List<PlayerData> Players = new List<PlayerData>();
            public int CurrentTick = -1;
            public float AutomaticReplayCurrentTime = 0;
            public float CurrentTime = 0;
            public bool NeedTick; // When player has chose a direction
            public bool WaitForInput; // Playre is controlling so we wait for hi inputs
            public bool NeedReset;
            public bool IsRewinding = false;
            public bool IsGoingBackward = true;
            public bool FixedUpdatePassed = false;
            public int PlayerCount = 0;

            public WorldManagerData(WorldManager WM)
            {
                foreach (GameObject P in WM.Players)
                {
                    Players.Add(new PlayerData(P.GetComponent<PlayerController>()));
                }
                PlayerCount = WM.Players.Count;
                CurrentTick = WM.CurrentTick;
                AutomaticReplayCurrentTime = WM.AutomaticReplayCurrentTime;
                CurrentTime = WM.CurrentTime;
                NeedTick = WM.NeedTick; // When player has chose a direction
                WaitForInput = WM.WaitForInput; // Playre is controlling so we wait for hi inputs
                NeedReset = WM.NeedReset;
                IsRewinding = WM.IsRewinding;
                IsGoingBackward = WM.IsGoingBackward;
                FixedUpdatePassed = WM.FixedUpdatePassed;

            }
            public void Apply(WorldManager WM)
            {
                // TODO: save recorder state
                WM.Rewind = new WorldManager.Recorder();
                for (int i = 0; i < WM.Players.Count; ++i)
                {
                    if(i >= Players.Count)
                    {
                        GameObject.Destroy(WM.Players[i]);
                        WM.Players.RemoveAt(i);
                    }
                    else
                    {
                        Players[i].Apply(WM.Players[i].GetComponent<PlayerController>());
                    }
                }
                WM.Players[WM.Players.Count-1].GetComponent<PlayerController>().levelUI.updatePlayerRef(WM.Players[WM.Players.Count-1]);
                WM.CurrentTick = CurrentTick;
                WM.AutomaticReplayCurrentTime = AutomaticReplayCurrentTime;
                WM.CurrentTime = CurrentTime;
                WM.NeedTick = NeedTick; // When player has chose a direction
                WM.WaitForInput = WaitForInput; // Playre is controlling so we wait for hi inputs
                WM.NeedReset = NeedReset;
                WM.IsRewinding = IsRewinding;
                WM.IsGoingBackward = IsGoingBackward;
                WM.FixedUpdatePassed = FixedUpdatePassed;
            }

            public override bool Equals(object obj)
            {
                WorldManagerData B = (WorldManagerData)obj;
                for (int i = 0; i < Players.Count; ++i)
                {
                    if (!Players[i].Equals(B.Players[i])) return false;
                }
                return B.CurrentTick == CurrentTick &&
                B.AutomaticReplayCurrentTime == AutomaticReplayCurrentTime &&
                //B.CurrentTime == CurrentTime && // very sensible to breaking ODTs
                B.NeedTick == NeedTick && // When player has chose a direction
                B.WaitForInput == WaitForInput && // Playre is controlling so we wait for hi inputs
                B.NeedReset == NeedReset &&
                B.IsRewinding == IsRewinding &&
                B.IsGoingBackward == IsGoingBackward &&
                B.FixedUpdatePassed == FixedUpdatePassed;
            }
        }
        [Serializable]
        public class LooperData
        {
            public List<PlayerController.Direction> Events = new List<PlayerController.Direction>();
            public bool IsPaused = false;
            public bool IsRunning = false;
            public bool IsRecording = false;
            public int CurrentIdx;

            public LooperData(Looper L)
            {
                Events = L.Events;
                IsPaused = L.IsPaused;
                IsRunning = L.IsRunning;
                IsRecording = L.IsRecording;
                CurrentIdx = L.CurrentIdx;
            }
            public void Apply(Looper L)
            {
                L.Events = Events;
                L.IsPaused = IsPaused;
                L.IsRunning = IsRunning;
                L.IsRecording = IsRecording;
                L.CurrentIdx = CurrentIdx;
            }
            public override bool Equals(object obj)
            {
                LooperData B = (LooperData)obj;

                for (int i =0; i < Events.Count; ++i)
                {
                    if (Events[i] != B.Events[i]) return false;
                }
                return IsPaused == B.IsPaused &&
                                IsRunning == B.IsRunning &&
                                IsRecording == B.IsRecording &&
                                CurrentIdx == B.CurrentIdx;

            }
        }
        [Serializable]
        public class PlayerData
        {
            public PlayerController.Direction CurrentDirection = PlayerController.Direction.NONE;
            public bool TickRequired = false;
            public bool IsLoopedControled = false;
            public bool HasAlreadyBeenBreakedFrom = false;
            public bool has_active_ui = false;
            public bool WAIT_ORDER = false;
            public LooperData L;
            public int BreakingTick = -1;
            public Color SpriteColor;

            public PlayerData(PlayerController PC)
            {
                CurrentDirection = PC.CurrentDirection;
                TickRequired = PC.TickRequired;
                IsLoopedControled = PC.IsLoopedControled;
                has_active_ui = PC.has_active_ui;
                WAIT_ORDER = PC.WAIT_ORDER;
                L = new LooperData(PC.L);
                BreakingTick = PC.BreakingTick;
                SpriteColor = PC.gameObject.GetComponentInChildren<SpriteRenderer>().color;

            }
            public void Apply(PlayerController PC)
            {
                PC.CurrentDirection = CurrentDirection;
                PC.TickRequired = TickRequired;
                PC.IsLoopedControled = IsLoopedControled;
                PC.has_active_ui = has_active_ui;
                PC.WAIT_ORDER = WAIT_ORDER;
                L.Apply(PC.L);
                PC.BreakingTick = BreakingTick;
                PC.gameObject.GetComponentInChildren<SpriteRenderer>().color = SpriteColor;
            }
            public override bool Equals(object obj)
            {
                PlayerData B = (PlayerData)obj;
                return B.CurrentDirection == CurrentDirection &&
                B.TickRequired == TickRequired &&
                B.IsLoopedControled == IsLoopedControled &&
                B.has_active_ui == has_active_ui &&
                B.WAIT_ORDER == WAIT_ORDER &&
                L.Equals(B.L) &&
                B.BreakingTick == BreakingTick &&
                SpriteColor.Equals(B.SpriteColor) ;

            }
        }

        [Serializable]
        public class Frame
        {
            public List<MiniObject> GameObjects = new List<MiniObject>();
            public WorldManagerData WM;
            public void Init()
            {

                GameObject[] SceneGameObjects = GameObject.FindObjectsOfType<GameObject>();

                foreach (GameObject go in SceneGameObjects)
                {

                    if(go.name.Contains("Tail") )
                    {
                        continue;
                    }
                    GameObjects.Add(new MiniObject(go.transform, go.name));
                    if (go.name == "GameLoop")
                    {
                        WM = new WorldManagerData(go.GetComponent<WorldManager>());
                    }
                }
            }
            public void Apply()
            {
                foreach (MiniObject go in GameObjects)
                {
                    GameObject CurrentGO = GameObject.Find(go.Name);
                    if (CurrentGO)
                    {
                        CurrentGO.transform.position = go.Position;
                        CurrentGO.transform.localPosition = go.LocalPosition;
                        CurrentGO.transform.localScale = go.LocalScale;
                        CurrentGO.transform.rotation = go.Rotation;
                        CurrentGO.transform.localRotation = go.LocalRotation;

                        if (go.Name == "GameLoop")
                        {
                            WorldManager CurrentWM = CurrentGO.GetComponent<WorldManager>();
                            if (CurrentWM)
                            {
                                WM.Apply(CurrentWM);
                            }
                        }
                    }
                }
            }
            public override bool Equals(object obj)
            {
                Frame B = (Frame)obj;
                for(int i=0;i < GameObjects.Count; ++i)
                {
                    if (!GameObjects[i].Equals(B.GameObjects[i]))
                        return false;
                }

                return WM.Equals(B.WM);
            }
        }
        [Serializable]
        public class InputSaverInput
        {
            public bool IsUp = false;
            public bool IsDown = false;
            public bool Down = false;
            public bool IsAxis = false;
            public float AxisValue = 0f;
            public override bool Equals(object obj)
            {
                //Check for null and compare run-time types.
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    InputSaverInput B = (InputSaverInput)obj;
                    return (IsUp == B.IsUp && IsDown == B.IsDown && Down == B.Down && IsAxis == B.IsAxis && AxisValue == B.AxisValue);
                }
            }
        }
        [Serializable]
        public class InputSaverEntry
        {
            public Dictionary<String, InputSaverInput> Inputs = new Dictionary<string, InputSaverInput>();
            public int NumberOfFramesIsSame = 1;
            public bool isDpadDownPressed = false;
            public bool isDpadUpPressed = false;
            public bool isDpadLeftPressed = false;
            public bool isDpadRightPressed = false;
            public void Add(String s, bool IsAxis = false)
            {
                InputSaverInput I = new InputSaverInput();
                if (!IsAxis)
                {
                    I.IsUp = Input.GetButtonUp(s);
                    I.IsDown = Input.GetButtonDown(s);
                    I.Down = Input.GetButton(s);
                }
                else
                {
                    I.IsAxis = true;
                    I.AxisValue = Input.GetAxisRaw(s);
                }
                Inputs.Add(s, I);
            }
            public override bool Equals(System.Object obj)
            {
                //Check for null and compare run-time types.
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    InputSaverEntry B = (InputSaverEntry)obj;

                    foreach (KeyValuePair<String, InputSaverInput> Value in Inputs)
                    {
                        if (!B.Inputs[Value.Key].Equals(Value.Value)) {
                            return false;
                        }
                    }
                    if (isDpadDownPressed != B.isDpadDownPressed || isDpadLeftPressed != B.isDpadLeftPressed ||
                            isDpadRightPressed != B.isDpadRightPressed || isDpadUpPressed != B.isDpadUpPressed)
                        return false;
                    return true;
                }
            }

        }
        public List<InputSaverEntry> InputsFrame = new List<InputSaverEntry>();
        public Frame FirstFrame = new Frame();
        public Frame EndFrame = new Frame();
        public void Record()
        {
            InputSaverEntry Entry = new InputSaverEntry();
            Entry.Add("Up");
            Entry.Add("Down");
            Entry.Add("Right");
            Entry.Add("Left");
            Entry.Add("Break");
            Entry.Add("BackTick");
            Entry.Add("Tick");
            Entry.Add("Restart");
            Entry.Add("DPad_Vertical", true);
            Entry.Add("DPad_Horizontal", true);

            if (InputsFrame.Count > 0 && Entry.Equals(InputsFrame[InputsFrame.Count - 1]))
            {
                InputsFrame[InputsFrame.Count - 1].NumberOfFramesIsSame += 1;
            }
            else
            {
                InputsFrame.Add(Entry);
            }
        }
    }

    bool IsRecording = false;
    bool IsReplaying = false;
    bool IsSerializing = false;
    public bool IsLooping = false;
    int CurrentReplayedCount = 0;
    int CurrentIdx = 0;
    bool FrameLock = false;
    public string FileName = "test";
    InputSaver IS = new InputSaver();
    public InputSaver.InputSaverEntry Tick(InputSaver.InputSaverEntry Entry)
    {
        if (IsReplaying)
        {

            if(FrameLock) return  IS.InputsFrame[CurrentIdx];
            if (CurrentIdx < IS.InputsFrame.Count)
            {
                var CurrentInputFrame = IS.InputsFrame[CurrentIdx];
                if (CurrentReplayedCount >= CurrentInputFrame.NumberOfFramesIsSame)
                {
                    ++CurrentIdx;
                    CurrentReplayedCount = 0;
                    return Tick(CurrentInputFrame);
                }
                ++CurrentReplayedCount;
                if (!FrameLock) FrameLock = true;
                return CurrentInputFrame;
            }
            else
            {
                if (IsLooping)
                {
                    CurrentIdx = 0;
                    CurrentReplayedCount = 0;
                    IS.FirstFrame.Apply();
                    if (!FrameLock) FrameLock = true;
                    return Tick(IS.InputsFrame[CurrentIdx]);
                } else
                {
                    CompareEndFrames();
                    return Entry;
                }
            }
        }
        else return Entry;
    }

    // Update is called once per frame
    void Update()
    {
        String Path = Application.persistentDataPath + "/" + FileName + ".save";
        bool StopSave = Input.GetKeyDown(KeyCode.M);
        bool StartSave = Input.GetKeyDown(KeyCode.L);
        bool Load = Input.GetKeyDown(KeyCode.K);
        if (Load)
        {
            if (this.Load(Path))
            {
                IS.FirstFrame.Apply();
                IsReplaying = true;
            }
        }
        else
        {
            if (StartSave)
            {
                IsRecording = true;
                IS.FirstFrame.Init();
            }
            if (StopSave)
            {
                IsRecording = false;
                IS.EndFrame.Init();
            }

            if (IsRecording)
            {
                IS.Record();
            }
            else
            {
                if (!IsSerializing && !IsReplaying && IS.InputsFrame.Count != 0)
                {
                    BinaryFormatter BF = new BinaryFormatter();
                    SurrogateSelector ss = new SurrogateSelector();
                   ss.AddSurrogate(typeof(Vector3),
                                    new StreamingContext(StreamingContextStates.All),
                                     new Vector3SerializationSurrogate());
                    ss.AddSurrogate(typeof(Quaternion),
                                    new StreamingContext(StreamingContextStates.All),
                                    new QuaternionSerializationSurrogate());
                    ss.AddSurrogate(typeof(Color),
                        new StreamingContext(StreamingContextStates.All),
                        new ColorSerializationSurrogate());
                    BF.SurrogateSelector = ss;

                    FileStream file = File.Create(Path);
                    IsSerializing = true;
                    BF.Serialize(file, IS);
                    IsSerializing = false;
                    file.Close();

                    IS = new InputSaver();
                }
            }
        }
    }

    private void LateUpdate()
    {
        // to be sure that we return only one frame\
        FrameLock = false;
    }

    public void EndLevel()
    {
        if (IsRecording)
        {
                IS.EndFrame.Init();

                String Path = Application.persistentDataPath + "/" + FileName + ".save";
                if (!IsSerializing && !IsReplaying && IS.InputsFrame.Count != 0)
                {
                    BinaryFormatter BF = new BinaryFormatter();
                    SurrogateSelector ss = new SurrogateSelector();
                   ss.AddSurrogate(typeof(Vector3),
                                    new StreamingContext(StreamingContextStates.All),
                                     new Vector3SerializationSurrogate());
                    ss.AddSurrogate(typeof(Quaternion),
                                    new StreamingContext(StreamingContextStates.All),
                                    new QuaternionSerializationSurrogate());
                    ss.AddSurrogate(typeof(Color),
                        new StreamingContext(StreamingContextStates.All),
                        new ColorSerializationSurrogate());
                    BF.SurrogateSelector = ss;

                    FileStream file = File.Create(Path);
                    IsSerializing = true;
                    BF.Serialize(file, IS);
                    IsSerializing = false;
                    file.Close();

                    IS = new InputSaver();
                }
        }

        if (IsReplaying)
        {
            CompareEndFrames();
        }
    }

    void CompareEndFrames()
    {
        InputSaver.Frame CurrentEndFrame = new InputSaver.Frame();
        CurrentEndFrame.Init();

        if (CurrentEndFrame.Equals(IS.EndFrame))
        {
            Debug.Log("[SUCCESS] TEST OK");
        }
        else
        {
            Debug.Log("[ERROR]   TEST KO");
        }
        IsReplaying = false;
    }

    bool Load(String s)
    {
        if (!File.Exists(s))
        {
            return false;
        }
        BinaryFormatter BF = new BinaryFormatter();
        SurrogateSelector ss = new SurrogateSelector();
                   ss.AddSurrogate(typeof(Vector3),
                                    new StreamingContext(StreamingContextStates.All),
                                     new Vector3SerializationSurrogate());
                    ss.AddSurrogate(typeof(Quaternion),
                                    new StreamingContext(StreamingContextStates.All),
                                    new QuaternionSerializationSurrogate());
                    ss.AddSurrogate(typeof(Color),
                        new StreamingContext(StreamingContextStates.All),
                        new ColorSerializationSurrogate());
        BF.SurrogateSelector = ss;
        using (FileStream stream = new FileStream(Application.persistentDataPath + "/" + FileName + ".save", FileMode.Open, FileAccess.Read))
        {
            IS = BF.Deserialize(stream) as InputSaver;
            stream.Close();
        }
        return true;
    }
}
