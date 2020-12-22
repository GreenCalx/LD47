using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

using System.Reflection;

[Serializable]
public abstract class IModel {
    public void CopyData(IModel Dest)
    {
        var Props = GetType().GetFields();
        foreach (FieldInfo property in Props)
        {
            var value = property.GetValue(this);
            if (value != null)
            {
                property.SetValue(Dest, value);
            }
        }
    }

    public override bool Equals(object obj)
    {
        IModel In = (IModel)obj;
        if (In == null) return false;
        var Props = GetType().GetFields();
        foreach (FieldInfo property in Props)
        {
            if(property.IsDefined(typeof(SerializableAttribute)))
                if (!property.GetValue(this).Equals(property.GetValue(In)))
                    return false;
        }
        return true;
    }

}

public interface ISavable
{
    IModel GetModel();
}

// NOTE toffa: unfortunately we cannot instantiate gameobject and transofmr easily
// those are the only two where we specifically have to create surrogate object to be
// able to save their state
[Serializable]
public class Transform_Save {
    Vector3 position;
    Vector3 localPosition;
    Quaternion localRotation;
    Quaternion rotation;
    Vector3 localScale;

    int childCount;
    GameObject_Save[] childs;

    public Transform_Save(Transform T)
    {
        position = T.position;
        localPosition = T.localPosition;
        localRotation = T.localRotation;
        rotation = T.rotation;
        localScale = T.localScale;

        childCount = T.childCount;
        childs = new GameObject_Save[childCount];
        for (int i = 0; i < T.childCount; ++i)
        {
           childs[i] = new GameObject_Save(T.GetChild(i).gameObject);
        };
    }

    public void GetTransform(Transform T, bool CreateChilds)
    {
        T.position = position;
        T.localPosition = localPosition;
        T.localRotation = localRotation;
        T.rotation = rotation;
        T.localScale = localScale;

        if (CreateChilds)
        {
            for (int i = 0; i < childCount; ++i)
            {
                GameObject go = new GameObject();
                childs[i].GetGameObject(go, CreateChilds);
            };
        }
    }

    public override bool Equals(object obj)
    {
        Transform_Save In = (Transform_Save)obj;
        if (In == null) return false;
        var Fields = GetType().GetFields();
        foreach (FieldInfo F in Fields)
        {
            if (!F.GetValue(this).Equals(F.GetValue(In)))
                return false;
        }
        return true;
    }
}

[Serializable]
public class GameObject_Save
{
    string name;
    int id;
    Transform_Save transform;
    List<IModel> Components = new List<IModel>();

    public GameObject_Save(GameObject GO)
    {
        name = GO.name;
        id = GO.GetInstanceID();
        transform = new Transform_Save(GO.transform) ;

        var Components_go = GO.GetComponents<MonoBehaviour>();
        foreach (var Component in Components_go)
        {
            if (Component is ISavable)
            {
                var C = Activator.CreateInstance(Component.GetType());
                (Component as ISavable).GetModel().CopyData((C as ISavable).GetModel());
                Components.Add((C as ISavable).GetModel());
            }
        }
    }

    public void GetGameObject(GameObject GO, bool CreateChilds)
    {
        GO.name = name;
        transform.GetTransform(GO.transform, CreateChilds);

        var Components_go = GO.GetComponents<MonoBehaviour>();
        for (int i = 0, j=0; i < Components_go.Length; ++i)
        {
            if (Components_go[i] is ISavable)
            {
                Components[j].CopyData((Components_go[i] as ISavable).GetModel());
                j++;
            }
        }
    }

    public override bool Equals(object obj)
    {
        GameObject_Save In = obj as GameObject_Save;
        if(In != null)
        {
            bool Result = In.name == this.name &&
                In.transform.Equals(transform) && Components.Count == In.Components.Count;
            if(Result)
            {
                for(int i = 0; i < Components.Count; ++i)
                {
                    if (!Components[i].Equals(In.Components[i])) return false;
                }
                return Result;
            }
        }
        return false;
    }
}

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
        public class Frame
        {
            public GameObject_Save[] SceneGameObjects;
            public void Init()
            {
                // NOTe toffa: we need to copy gameobject or else they will update once stored
                // as they are stored by ref and not value
                var SceneGameObjectsTemp = GameObject.FindObjectsOfType<GameObject>();
                SceneGameObjects = new GameObject_Save[SceneGameObjectsTemp.Length];
                int idx = 0;
                foreach(GameObject go in SceneGameObjectsTemp)
                {
                    SceneGameObjects[idx] = new GameObject_Save(go);
                    idx++;
                }
            }
            public void Apply()
            {
                // NOTe toffa: we need to copy gameobject or else they will update once stored
                // as they are stored by ref and not value
                var SceneGameObjectsTemp = GameObject.FindObjectsOfType<GameObject>();
                int idx = 0;
                foreach(GameObject go in SceneGameObjectsTemp)
                {
                    SceneGameObjects[idx].GetGameObject(go, false);
                    idx++;
                }
            }
            public override bool Equals(object obj)
            {
                Frame In = obj as Frame;
                if (In != null && In.SceneGameObjects.Length != SceneGameObjects.Length) return false;

                for(int i = 0; i < SceneGameObjects.Length;++i)
                {
                    if (!In.SceneGameObjects[i].Equals( SceneGameObjects[i]))
                    {
                        return false;
                    }
                }
                return true;
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
                        if (!B.Inputs[Value.Key].Equals(Value.Value))
                        {
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

            if (FrameLock) return IS.InputsFrame[CurrentIdx];
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
                }
                else
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
