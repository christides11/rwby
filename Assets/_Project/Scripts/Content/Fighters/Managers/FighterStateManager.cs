using System;
using Fusion;
using HnSF;
using HnSF.Combat;
using HnSF.Fighters;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Profiling;

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

        [Networked] public int CurrentMoveset { get; set; }
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentStateMoveset { get; set; }
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentState { get; set; }
        [Networked] public int CurrentStateFrame { get; set; } = 0;
        [Networked] public NetworkBool markedForStateChange { get; set; } = false;
        [Networked] public int nextStateMoveset { get; set; } = -1;
        [Networked] public int nextState { get; set; } = 0;
        [Networked] public StateGroundedGroupType CurrentGroundedState { get; set; } = StateGroundedGroupType.AERIAL;
        [Networked] public StateType CurrentStateType { get; set; } = StateType.NONE;


        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;
        public rwby.Moveset[] movesets;
        
        [NonSerialized] public StateFunctionMapper functionMapper = new StateFunctionMapper(); 
        [NonSerialized] public StateConditionMapper conditionMapper = new StateConditionMapper();

        [HideInInspector] public int interruptedOnFrame = 0;
        
        public static void OnChangedState(Changed<FighterStateManager> changed){
            changed.Behaviour.OnStateChanged?.Invoke(changed.Behaviour);
        }

        public void Tick()
        {
            if (markedForStateChange && GameModeBase.singleton.CanTakeInput())
            {
                ChangeState(nextState, nextStateMoveset, 0, true);
            }

            if (CurrentState == 0) return;
            var stateTimeline = GetState();
            ProcessState(stateTimeline, onInterrupt: false, autoIncrement: stateTimeline.autoIncrement, autoLoop: stateTimeline.autoLoop);
            HandleStateGroup(stateTimeline);
        }

        private void ProcessState(StateTimeline state, bool onInterrupt = false, bool autoIncrement = false, bool autoLoop = false)
        {
            //Profiler.BeginSample($"State Processing: {state.stateName}");
            var topState = state;
            while (true)
            {
                int realFrame = onInterrupt ? topState.totalFrames+1 : Mathf.Clamp(CurrentStateFrame, 0, state.totalFrames);
                foreach (var d in state.data)
                {
                    if (d.Parent != -1) continue; 
                    ProcessStateVariables(state, d, realFrame, topState.totalFrames, onInterrupt);
                }

                if (!state.useBaseState) break;
                state = (StateTimeline)state.baseState;
            }
            state = topState;
            if (onInterrupt != false || !autoIncrement)
            {
                //Profiler.EndSample();
                return;
            }
            IncrementFrame(1);
            if (autoLoop && CurrentStateFrame > state.totalFrames)
            {
                SetFrame(1);
            }
            //Profiler.EndSample();
        }
        
        public void ProcessStateVariables(StateTimeline state, IStateVariables d, int realFrame, int totalFrames, bool onInterrupt)
        {
            var valid = true;
            for (int j = 0; j < d.FrameRanges.Length; j++)
            {
                int frx = ConvertFrameRangeNumber((int)d.FrameRanges[j].x, totalFrames);
                int fry = ConvertFrameRangeNumber((int)d.FrameRanges[j].y, totalFrames);
                if (realFrame >= frx && realFrame <= fry) continue;
                valid = false;
                break;
            }

            if (!valid) return;
            if (d.Condition != null && !conditionMapper.TryCondition(d.Condition.GetType(), manager, d.Condition, state, realFrame)) return;
            functionMapper.functions[d.GetType()](manager, d, state, realFrame);

            if (d.Children is null) return;
            foreach (var t in d.Children)
            {
                int childStateIndex = state.stateVariablesIDMap[t];
                ProcessStateVariables(state, state.data[childStateIndex], realFrame, totalFrames, onInterrupt);
            }
        }

        private int ConvertFrameRangeNumber(int number, int totalFrames)
        {
            if (number == -1) return totalFrames;
            if (number == -2) return totalFrames+1;
            return number;
        }

        public bool TryCondition(StateTimeline state, IConditionVariables condition, int frame)
        {
            if (condition == null) return true;
            return conditionMapper.TryCondition(condition.GetType(), manager, condition, state, frame);
        }

        private void HandleStateGroup(StateTimeline stateTimeline)
        {
            switch (stateTimeline.initialGroundedState)
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
            nextStateMoveset = moveset;
        }

        public bool ChangeState(int state, int moveset = -1, int stateFrame = 0, bool callOnInterrupt = true)
        {
            markedForStateChange = false;
            int previousStateMoveset = CurrentStateMoveset;
            int previousState = CurrentState;
            
            if (callOnInterrupt && previousState != (int)FighterCmnStates.NULL)
            {
                StateTimeline previousStateTimeline = GetState();
                interruptedOnFrame = CurrentStateFrame;
                SetFrame(previousStateTimeline.totalFrames+1);
                ProcessState(previousStateTimeline, true);
            }

            CurrentStateMoveset = moveset == -1 ? CurrentMoveset : moveset;
            CurrentState = state;
            CurrentStateFrame = stateFrame;
            if (CurrentStateFrame == 0)
            {
                SetFrame(0);
                ProcessState(GetState());
                SetFrame(1);
            }

            var currentStateTimeline = GetState();
            if (previousState != (int)FighterCmnStates.NULL)
            {
                StateChanged(GetState(previousStateMoveset, previousState) as rwby.StateTimeline, currentStateTimeline);
            }
            CurrentGroundedState = currentStateTimeline.initialGroundedState;
            CurrentStateType = currentStateTimeline.stateType;
            return true;
        }
        
        public void StateChanged(rwby.StateTimeline previousState, rwby.StateTimeline currentState)
        {
            combatManager.HitboxManager.Reset();
            
            if (CurrentGroundedState != currentState.initialGroundedState)
            {
                manager.FPhysicsManager.SnapECB();
                if (CurrentGroundedState == StateGroundedGroupType.AERIAL)
                {
                    manager.ResetVariablesOnGround();
                }
                else
                {

                }
            }

            if (currentState.stateType is StateType.MOVEMENT or StateType.NONE)
            {
                if (previousState.stateType != currentState.stateType)
                {
                    combatManager.ResetString();
                    combatManager.ComboTime = 0;
                    combatManager.ComboCounter = 0;
                }
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
            CurrentMoveset = movesetIndex;
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