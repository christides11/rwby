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
        public delegate void StateAction(IFighterBase self, ushort from, uint fromStateFrame);
        public delegate void StateFrameAction(IFighterBase self, uint preChangeFrame);
        public event StateAction OnStatePreChange;
        public event StateAction OnStatePostChange;
        public event StateFrameAction OnStateFrameSet;

        [Networked] public ushort CurrentState { get; set; }
        [Networked] public uint CurrentStateFrame { get; set; }

        protected Dictionary<ushort, FighterStateBase> states = new Dictionary<ushort, FighterStateBase>();
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;

        public void Tick()
        {
            if (CurrentState == 0) return;
            states[CurrentState].OnUpdate();
        }

        public override void FixedUpdateNetwork()
        {
            if (CurrentState == 0) return;
            states[CurrentState].OnLateUpdate();
        }

        public void AddState(FighterStateBase state, ushort stateNumber)
        {
            (state as FighterState).manager = manager;
            states.Add(stateNumber, state);
        }

        public void RemoveState(ushort stateNumber)
        {
            if(CurrentState == stateNumber)
            {
                return;
            }
            states.Remove(stateNumber);
        }

        public bool ChangeState(ushort state, uint stateFrame = 0, bool callOnInterrupt = true)
        {
            if (states.ContainsKey(state))
            {
                ushort oldState = CurrentState;
                uint oldStateFrame = CurrentStateFrame;

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

        public string GetCurrentStateName()
        {
            if(CurrentState == 0)
            {
                return "N/A";
            }
            return states[CurrentState].GetName();
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