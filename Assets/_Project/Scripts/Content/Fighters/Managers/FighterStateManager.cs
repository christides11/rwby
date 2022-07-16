using System;
using Fusion;
using HnSF;
using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [OrderBefore(typeof(FighterCombatManager))]
    [OrderAfter(typeof(Fusion.HitboxManager), typeof(FighterManager))]
    public class FighterStateManager : NetworkBehaviour, IFighterStateManager
    {   
        public delegate void EmptyDelegate(FighterStateManager stateManager);
        public event EmptyDelegate OnStateChanged;
        
        public int MovesetCount
        {
            get { return movesets.Length; }
        }
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentStateMoveset { get; set; }
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentState { get; set; }
        [Networked] public int CurrentStateFrame { get; set; } = 0;
        [Networked] public NetworkBool markedForStateChange { get; set; } = false;
        [Networked] public int nextStateMoveset { get; set; } = -1;
        [Networked] public int nextState { get; set; } = 0;
        

        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;
        public rwby.Moveset[] movesets;
        
        [NonSerialized] public StateFunctionMapper functionMapper = new StateFunctionMapper(); 
        [NonSerialized] public StateConditionMapper conditionMapper = new StateConditionMapper();
        
        public static void OnChangedState(Changed<FighterStateManager> changed){
            changed.Behaviour.OnStateChanged?.Invoke(changed.Behaviour);
        }

        public void Tick()
        {
            if (markedForStateChange)
            {
                ChangeState(nextState, 0, 0, true);
            }

            if (CurrentState == 0) return;
            var stateTimeline = GetState();
            ProcessState(stateTimeline, onInterrupt: false, autoIncrement: stateTimeline.autoIncrement, autoLoop: stateTimeline.autoLoop);
            HandleStateGroup(stateTimeline);
        }

        private void ProcessState(StateTimeline state, bool onInterrupt = false, bool autoIncrement = false, bool autoLoop = false)
        {
            var topState = state;
            while (true)
            {
                int realFrame = onInterrupt ? state.totalFrames+1 : Mathf.Clamp(CurrentStateFrame, 0, state.totalFrames);
                foreach (var d in state.data)
                {
                    ProcessStateVariables(state, d, realFrame, onInterrupt);
                }

                if (!state.useBaseState) break;
                state = (StateTimeline)state.baseState;
            }
            state = topState;
            if (onInterrupt != false || !autoIncrement) return;
            IncrementFrame(1);
            if (autoLoop && CurrentStateFrame > state.totalFrames)
            {
                SetFrame(1);
            }
        }
        
        public void ProcessStateVariables(StateTimeline state, IStateVariables d, int realFrame, bool onInterrupt)
        {
            var valid = true;
            for (int j = 0; j < d.FrameRanges.Length; j++)
            {
                if (!onInterrupt && realFrame != 0 && d.FrameRanges[j].x == -1) break;
                if (!(realFrame < d.FrameRanges[j].x) &&
                    !(realFrame > d.FrameRanges[j].y)) continue;
                valid = false;
                break;
            }

            if (!valid) return;
            if (d.Condition != null && !conditionMapper.TryCondition(d.Condition.GetType(), manager, d.Condition, state, realFrame)) return;
            functionMapper.functions[d.GetType()](manager, d, state, realFrame);

            foreach (var t in d.Children)
            {
                int childStateIndex = state.stateVariablesIDMap[t];
                ProcessStateVariables(state, state.data[childStateIndex], realFrame, onInterrupt);
            }
        }

        public bool TryCondition(StateTimeline state, IConditionVariables condition, int frame)
        {
            if (condition == null) return true;
            return conditionMapper.TryCondition(condition.GetType(), manager, condition, state, frame);
        }

        private void HandleStateGroup(StateTimeline stateTimeline)
        {
            switch (stateTimeline.stateGroundedGroup)
            {
                case StateGroundedGroupType.AERIAL:
                    break;
                case StateGroundedGroupType.GROUND:
                    //manager.ResetVariablesOnGround();
                    break;
            }
        }
        
        public virtual FighterStats GetCurrentStats()
        {
            return (movesets[CurrentStateMoveset] as Moveset).fighterStats;
        }


        public rwby.Moveset GetMoveset()
        {
            return (rwby.Moveset)GetMoveset(CurrentStateMoveset);
        }
        
        public MovesetDefinition GetMoveset(int index)
        {
            return movesets[index];
        }

        public void MarkForStateChange(int state, int moveset = -1)
        {
            if (markedForStateChange) return;
            markedForStateChange = true;
            nextState = state;
        }

        public bool ChangeState(int state, int moveset = -1, int stateFrame = 0, bool callOnInterrupt = true)
        {
            markedForStateChange = false;
            int previousState = CurrentState;
            if (callOnInterrupt && previousState != (int)FighterCmnStates.NULL)
            {
                StateTimeline currentStateTimeline = GetState();
                SetFrame(currentStateTimeline.totalFrames+1);
                ProcessState(currentStateTimeline, true);
            }

            CurrentStateMoveset = moveset == -1 ? CurrentStateMoveset : moveset;
            CurrentState = state;
            CurrentStateFrame = stateFrame;
            if (CurrentStateFrame == 0)
            {
                SetFrame(0);
                ProcessState(GetState());
                SetFrame(1);
            }

            if (previousState != (int)FighterCmnStates.NULL)
            {
                StateChanged(GetState(previousState) as rwby.StateTimeline, GetState());
            }
            return true;
        }
        
        public void StateChanged(rwby.StateTimeline previousState, rwby.StateTimeline currentState)
        {
            combatManager.HitboxManager.Reset();
            if (previousState.stateGroundedGroup != currentState.stateGroundedGroup)
            {
                manager.FPhysicsManager.SnapECB();
                if (previousState.stateGroundedGroup == StateGroundedGroupType.AERIAL)
                {
                    manager.ResetVariablesOnGround();
                }
                else
                {

                }
            }

            if (currentState.stateType == StateType.MOVEMENT)
            {
                if(previousState.stateType != currentState.stateType) combatManager.ResetString();
            }else if (currentState.stateType == StateType.ATTACK)
            {
                if (currentState.maxUsesInString != -1)
                {
                    combatManager.AddMoveToString();
                }
            }
        }

        public void InitState(HnSF.StateTimeline state)
        {
            if (CurrentState == 0) return;
            SetFrame(0);
        }

        public StateTimeline GetState()
        {
            return GetState(CurrentState) as rwby.StateTimeline;
        }
        
        public HnSF.StateTimeline GetState(int state)
        {
            return (GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[state];
        }

        public HnSF.StateTimeline GetState(int moveset, int state)
        {
            return movesets[moveset].stateMap[state];
        }

        public void SetMoveset(int movesetIndex)
        {
            CurrentStateMoveset = movesetIndex;
        }

        public string GetCurrentStateName()
        {
            return ((GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState] as rwby.StateTimeline).stateName;
        }

        public void IncrementFrame()
        {
            CurrentStateFrame++;
        }
        
        public void IncrementFrame(int amt = 1)
        {
            CurrentStateFrame += amt;
        }

        public void SetFrame(int frame)
        {
            CurrentStateFrame = frame;
        }
    }
}