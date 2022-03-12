using Fusion;
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
        
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentStateMoveset { get; set; }
        [Networked(OnChanged = nameof(OnChangedState))] public int CurrentState { get; set; }
        [Networked] public int CurrentStateFrame { get; set; }
        [Networked] public NetworkBool markedForStateChange { get; set; }
        [Networked] public int nextStateMoveset { get; set; }
        [Networked] public int nextState { get; set; }

        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;
        public PlayableDirector director;
        public rwby.Moveset[] movesets;
        public static void OnChangedState(Changed<FighterStateManager> changed){
            changed.Behaviour.OnStateChanged?.Invoke(changed.Behaviour);
        }
        
        public void ResimulationSync()
        {
            var currentState = (manager.StateManager.GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState];
            if (currentState != director.playableAsset) InitState(currentState);
            director.time = (float)CurrentStateFrame * Runner.Simulation.DeltaTime;
        }
        
        public void Tick()
        {
            if (Runner.Simulation.IsResimulation && Runner.Simulation.IsFirstTick)
            {
                ResimulationSync();
            }
            
            if (markedForStateChange)
            {
                ChangeState(nextState, 0, true);
            }
            if (CurrentState == 0) return;
            director.Evaluate();
            var s = GetState();
            if(s.autoIncrement) IncrementFrame();
            if(s.autoLoop && CurrentStateFrame == s.totalFrames) SetFrame(s.loopFrame);
            HandleStateGroup(s);
        }

        private void HandleStateGroup(StateTimeline stateTimeline)
        {
            switch (stateTimeline.stateGroup)
            {
                case StateGroupType.AERIAL:
                    break;
                case StateGroupType.GROUND:
                    manager.ResetVariablesOnGround();
                    break;
            }
        }
        
        public virtual FighterStats GetCurrentStats()
        {
            return (movesets[CurrentStateMoveset] as Moveset).fighterStats;
        }

        public void MarkForStateChange(int state)
        {
            markedForStateChange = true;
            nextState = state;
        }

        public MovesetDefinition GetMoveset(int index)
        {
            return movesets[index];
        }

        public bool ChangeState(int state, int stateFrame = 0, bool callOnInterrupt = true)
        {
            ChangeState(CurrentStateMoveset, state, stateFrame, callOnInterrupt);
            return true;
        }

        public void ChangeState(int stateMoveset, int state, int stateFrame = 0, bool callOnInterrupt = true)
        {
            markedForStateChange = false;
            if (callOnInterrupt && CurrentState != 0)
            {
                SetFrame((GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState].totalFrames);
                director.Evaluate();
            }
            
            CurrentStateMoveset = stateMoveset;
            CurrentStateFrame = stateFrame;
            CurrentState = state;
            if (CurrentStateFrame == 0)
            {
                InitState();
                SetFrame(0);
                director.Evaluate();
                SetFrame(1);
            }
        }

        public void InitState()
        {
            InitState(GetState());
        }

        public void InitState(HnSF.StateTimeline state)
        {
            director.playableAsset = state;
            if (state == null) return;
            foreach (var pAO in director.playableAsset.outputs)
            {
                director.SetGenericBinding(pAO.sourceObject, manager);
            }
            director.RebuildGraph();
        }

        public StateTimeline GetState()
        {
            return (GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState] as rwby.StateTimeline;
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
            return (GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState].name;
        }

        public void IncrementFrame()
        {
            CurrentStateFrame++;
            director.time = (float)CurrentStateFrame * Runner.Simulation.DeltaTime;
        }

        public int MovesetCount { get; }

        public void SetFrame(int frame)
        {
            CurrentStateFrame = frame;
            director.time = (float)frame * Runner.Simulation.DeltaTime;
        }
    }
}