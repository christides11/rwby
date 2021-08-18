using Fusion;
using HnSF.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterStateManager : NetworkBehaviour, IFighterStateManager //HnSF.Fighters.FighterStateManager
    {
        public delegate void StateAction(IFighterBase self, ushort from, uint fromStateFrame);
        public delegate void StateFrameAction(IFighterBase self, uint preChangeFrame);
        public event StateAction OnStatePreChange;
        public event StateAction OnStatePostChange;
        public event StateFrameAction OnStateFrameSet;

        [Networked] public ushort CurrentState { get; set; }
        [Networked] public uint CurrentStateFrame { get; set; }

        protected Dictionary<ushort, FighterStateBase> states = new Dictionary<ushort, FighterStateBase>();
        [SerializeField] protected string currentStateName;
        [SerializeField] protected FighterManager manager;

        public void Tick()
        {
            if (CurrentState == 0) return;
            states[CurrentState].OnUpdate();
        }

        public void LateTick()
        {
            states[CurrentState].OnLateUpdate();
        }


        public void AddState(FighterStateBase state, ushort stateNumber)
        {
            (state as FighterState).manager = manager;
            states.Add(stateNumber, state);
        }

        public bool ChangeState(ushort state, uint stateFrame = 0, bool callOnInterrupt = true)
        {
            if (states.ContainsKey(state))
            {
                ushort oldState = CurrentState;
                uint oldStateFrame = CurrentStateFrame;

                if (callOnInterrupt)
                {
                    if (CurrentState != 0)
                    {
                        states[CurrentState].OnInterrupted();
                    }
                }
                CurrentStateFrame = stateFrame;
                CurrentState = state;
                OnStatePreChange?.Invoke(manager, oldState, oldStateFrame);
                if (CurrentStateFrame == 0)
                {
                    states[CurrentState].Initialize();
                    CurrentStateFrame = 1;
                }
                currentStateName = states[CurrentState].GetName();
                OnStatePostChange?.Invoke(manager, oldState, oldStateFrame);
                return true;
            }
            return false;
        }

        public FighterStateBase GetState(ushort state)
        {
            if (states.ContainsKey(state))
            {
                return states[state];
            }
            return null;
        }

        public void IncrementFrame()
        {
            CurrentStateFrame++;
        }

        public void SetFrame(uint frame)
        {
            uint preFrame = CurrentStateFrame;
            CurrentStateFrame = frame;
            OnStateFrameSet?.Invoke(manager, preFrame);
        }
    }
}