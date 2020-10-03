using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
