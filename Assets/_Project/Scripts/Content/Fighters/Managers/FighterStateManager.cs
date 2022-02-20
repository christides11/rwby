using Fusion;
using HnSF.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterCombatManager))]
    [OrderAfter(typeof(Fusion.HitboxManager), typeof(FighterManager))]
    public class FighterStateManager : NetworkBehaviour, IFighterStateManager
    {
        public delegate void StateAction(IFighterBase self, int from, int fromStateFrame);
        public delegate void StateFrameAction(IFighterBase self, int preChangeFrame);
        public event StateAction OnStatePreChange;
        public event StateAction OnStatePostChange;
        public event StateFrameAction OnStateFrameSet;

        [Networked] public int CurrentState { get; set; }
        [Networked] public int CurrentStateFrame { get; set; }

        protected Dictionary<int, HnSF.StateTimeline> states = new Dictionary<int, HnSF.StateTimeline>();
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;

        public void Tick()
        {
            if (CurrentState == 0) return;
            //states[CurrentState].OnUpdate();
        }

        public override void FixedUpdateNetwork()
        {
            if (CurrentState == 0) return;
            //states[CurrentState].OnLateUpdate();
        }

        public void AddState(HnSF.StateTimeline state, int stateNumber)
        {
            //(state as FighterState).manager = manager;
            //states.Add(stateNumber, state);
        }

        public void RemoveState(int stateNumber)
        {
            if(CurrentState == stateNumber)
            {
                return;
            }
            states.Remove(stateNumber);
        }

        public bool ChangeState(int state, int stateFrame = 0, bool callOnInterrupt = true)
        {
            /*
            if (states.ContainsKey(state))
            {
                int oldState = CurrentState;
                int oldStateFrame = CurrentStateFrame;

                if (callOnInterrupt)
                {
                    if (oldState != 0)
                    {
                        states[oldState].OnInterrupted();
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
                OnStatePostChange?.Invoke(manager, oldState, oldStateFrame);
                return true;
            }*/
            return false;
        }

        public HnSF.StateTimeline GetState(int state)
        {
            /*
            if (states.ContainsKey(state))
            {
                return states[state];
            }*/
            return null;
        }

        public string GetCurrentStateName()
        {
            return "";
            /*
            if(CurrentState == 0)
            {
                return "N/A";
            }
            return states[CurrentState].GetName();*/
        }

        public void IncrementFrame()
        {
            CurrentStateFrame++;
        }

        public void SetFrame(int frame)
        {
            //uint preFrame = CurrentStateFrame;
            //CurrentStateFrame = frame;
            //OnStateFrameSet?.Invoke(manager, preFrame);
        }
    }
}