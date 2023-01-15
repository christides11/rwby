using System;
using ExitGames.Client.Photon.StructWrapping;
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

        [Networked, Capacity(10)] public NetworkDictionary<MovesetStateIdentifier, int> airtimeMoveCounter => default;

        [Networked] public int CurrentMoveset { get; set; }
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentStateMoveset { get; set; }
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentState { get; set; }
        [Networked] public int CurrentStateFrame { get; set; } = 0;
        [Networked] public NetworkBool markedForStateChange { get; set; } = false;
        [Networked] public int nextStateMoveset { get; set; } = -1;
        [Networked] public int nextState { get; set; } = 0;
        [Networked] public int nextStateFrame { get; set; } = 0;
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

        public bool CheckStateAirUseCounter(int moveset, int state, int limit)
        {
            if (!airtimeMoveCounter.ContainsKey(new MovesetStateIdentifier(moveset, state))) return true;
            if (airtimeMoveCounter[new MovesetStateIdentifier(moveset, state)] >= limit) return false;
            return true;
        }

        public void IncrementStateAirUseCounter(int moveset, int state)
        {
            var key = new MovesetStateIdentifier(moveset, state);
            var dict = airtimeMoveCounter;
            dict.Add(key, 0);
            dict[new MovesetStateIdentifier(moveset, state)] += 1;
        }
        
        public void Tick()
        {
            if (CurrentState == 0) return;
            var stateTimeline = GetState();
            ProcessState(stateTimeline, onInterrupt: false, autoIncrement: stateTimeline.autoIncrement, autoLoop: stateTimeline.autoLoop);
            // TODO: Move to combat manager.
            // Handles cancel list on clashing
            if (manager.FCombatManager.ClashState)
            {
                var m = (rwby.Moveset)GetMoveset(CurrentMoveset);
                switch (CurrentGroundedState)
                {
                    case StateGroundedGroupType.GROUND:
                        foreach (var d in m.clashCancelListGrd.data)
                        {
                            ProcessStateVariables((rwby.StateTimeline)stateTimeline, d, CurrentStateFrame, stateTimeline.totalFrames, false);
                        }
                        break;
                    case StateGroundedGroupType.AERIAL:
                        foreach (var d in m.clashCancelListAir.data)
                        {
                            ProcessStateVariables((rwby.StateTimeline)stateTimeline, d, CurrentStateFrame, stateTimeline.totalFrames, false);
                        }
                        break;
                }
            }
            HandleStateGroup(stateTimeline);
            
            if (markedForStateChange)
            {
                ChangeState(nextState, nextStateMoveset, nextStateFrame, true);
            }
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
            if (CurrentStateFrame > state.totalFrames)
            {
                if (autoLoop)
                {
                    SetFrame(state.autoLoopFrame);
                }
                else
                {
                    SetFrame(state.totalFrames);
                }
            }
            //Profiler.EndSample();
        }
        
        public void ProcessStateVariables(StateTimeline state, IStateVariables d, int realFrame, int totalFrames, bool onInterrupt)
        {
            var valid = d.FrameRanges.Length == 0 ? true : false;
            for (int j = 0; j < d.FrameRanges.Length; j++)
            {
                int frx = ConvertFrameRangeNumber((int)d.FrameRanges[j].x, totalFrames);
                int fry = ConvertFrameRangeNumber((int)d.FrameRanges[j].y, totalFrames);
                if (realFrame >= frx && realFrame <= fry)
                {
                    valid = true;
                    break;
                }
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

        public rwby.Moveset GetCurrentMoveset()
        {
            return (rwby.Moveset)GetMoveset(CurrentMoveset);
        }
        
        public HnSF.Combat.IMovesetDefinition GetMoveset(int index)
        {
            return movesets[index];
        }

        public bool CheckStateConditions(int movesetID, int stateID, int frame, bool checkInputSequence, bool checkCondition,
            bool ignoreAirtimeCheck = false, bool ignoreStringUseCheck = false, bool ignoreAuraCheck = false)
        {
            StateTimeline state = (StateTimeline)(GetState(movesetID, stateID));

            if (!ignoreAuraCheck && combatManager.Aura < state.auraRequirement) return false;
            if (!ignoreAirtimeCheck && state.maxUsesPerAirtime != -1 &&
                !CheckStateAirUseCounter(movesetID, stateID, state.maxUsesPerAirtime)) return false;
            if (!ignoreStringUseCheck && state.maxUsesInString != -1 && !manager.FCombatManager.MovePossible(
                    new MovesetStateIdentifier(movesetID, stateID), state.maxUsesInString, state.selfChainable))
                return false;
            if (checkInputSequence &&
                !manager.FCombatManager.CheckForInputSequence(state.inputSequence,
                    holdInput: state.inputSequenceAsHoldInputs)) return false;
            if (checkCondition && !manager.FStateManager.TryCondition(state, state.condition, frame)) return false;
            return true;
        }

        public void MarkForStateChange(int state, int moveset = -1, int frame = 0, bool force = false)
        {
            if (markedForStateChange && !force) return;
            markedForStateChange = true;
            nextState = state;
            nextStateMoveset = moveset;
            nextStateFrame = frame;
        }

        public void ForceStateChange(int state, int moveset = -1, int frame = 0, bool clearMarkedState = false, bool callOnInterrupt = true)
        {
            if (clearMarkedState) markedForStateChange = false;
            ChangeState(state, moveset, frame, callOnInterrupt);
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
            
            manager.BoxManager.ResetAllBoxes();
            
            var previousGroundedState = CurrentGroundedState;
            var previousStateType = CurrentStateType;
            
            CurrentStateMoveset = moveset == -1 ? CurrentMoveset : moveset;
            CurrentState = state;
            CurrentStateFrame = stateFrame;
            var currentStateTimeline = GetState();
            if(currentStateTimeline.initialGroundedState != StateGroundedGroupType.NONE) CurrentGroundedState = currentStateTimeline.initialGroundedState;
            CurrentStateType = currentStateTimeline.stateType;
            if (CurrentStateFrame == 0)
            {
                SetFrame(0);
                ProcessState(currentStateTimeline);
                SetFrame(1);
            }
            
            if (previousState != (int)FighterCmnStates.NULL)
            {
                StateChanged(previousGroundedState, previousStateType, 
                    GetState(previousStateMoveset, previousState) as rwby.StateTimeline, currentStateTimeline);
            }
            return true;
        }
        
        public void StateChanged(StateGroundedGroupType previousGroundedGroup, StateType previousStateType,
            rwby.StateTimeline previousState, rwby.StateTimeline currentState)
        {
            combatManager.ClashState = false;
            combatManager.ResetCharge();
            combatManager.HitboxManager.Reset();
            manager.fighterEffector.ClearCurrentEffects();
            manager.fighterSounder.ClearCurrentSounds();
            
            if (CurrentGroundedState != previousGroundedGroup)
            {
                manager.FPhysicsManager.SnapECB();
                if (CurrentGroundedState == StateGroundedGroupType.GROUND)
                {
                    manager.ResetVariablesOnGround();
                    airtimeMoveCounter.Clear();
                }
                else
                {
                    
                }
            }

            if (CurrentGroundedState == StateGroundedGroupType.AERIAL)
            {
                if(currentState.maxUsesPerAirtime != -1) IncrementStateAirUseCounter(CurrentStateMoveset, CurrentState);
            }

            if (CurrentStateType is StateType.MOVEMENT or StateType.NONE)
            {
                if (previousStateType != CurrentStateType)
                {
                    combatManager.ResetString();
                    combatManager.ResetProration();
                    combatManager.ComboStartTick = 0;
                    combatManager.ComboCounter = 0;
                    combatManager.CurrentGroundBounces = 0;
                    combatManager.CurrentWallBounces = 0;
                    combatManager.hardKnockdownCounter = 0;
                    combatManager.shouldHardKnockdown = false;
                }
            }else if (CurrentStateType == StateType.ATTACK)
            {
                if (currentState.maxUsesInString != -1)
                {
                    combatManager.AddMoveToString(CurrentStateMoveset, CurrentState);
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
            return (GetMoveset(CurrentStateMoveset) as rwby.Moveset).GetState(state);
        }

        public HnSF.StateTimeline GetState(int moveset, int state)
        {
            return movesets[moveset].GetState(state);
        }

        public void SetMoveset(int movesetIndex)
        {
            CurrentMoveset = movesetIndex;
        }

        public string GetCurrentStateName()
        {
            return ((GetMoveset(CurrentStateMoveset) as rwby.Moveset).GetState(CurrentState) as rwby.StateTimeline).stateName;
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