using Fusion;
using HnSF.Fighters;
using UnityEngine;
using UnityEngine.Playables;

namespace rwby
{
    [OrderBefore(typeof(FighterCombatManager))]
    [OrderAfter(typeof(Fusion.HitboxManager), typeof(FighterManager))]
    public class FighterStateManager : NetworkBehaviour, IFighterStateManager
    {
        [Networked] public int CurrentStateMoveset { get; set; }
        [Networked] public int CurrentState { get; set; }
        [Networked] public int CurrentStateFrame { get; set; }
        
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;

        public PlayableDirector director;

        [Networked] public NetworkBool markedForStateChange { get; set; }
        [Networked] public int nextState { get; set; }

        public void ResimulationSync()
        {
            var currentState = (manager.FCombatManager.GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState];
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
        }
        
        public void AddState(HnSF.StateTimeline state, int stateNumber)
        {
            Debug.LogError("Cannot add states.");
        }

        public void RemoveState(int stateNumber)
        {
            Debug.LogError("Cannot remove states.");

        }

        public void MarkForStateChange(int state)
        {
            markedForStateChange = true;
            nextState = state;
        }
        
        public bool ChangeState(int state, int stateFrame = 0, bool callOnInterrupt = true)
        {
            ChangeState(combatManager.CurrentMovesetIdentifier, state, stateFrame, callOnInterrupt);
            return true;
        }

        public void ChangeState(int stateMoveset, int state, int stateFrame = 0, bool callOnInterrupt = true)
        {
            markedForStateChange = false;
            if (callOnInterrupt && CurrentState != 0)
            {
                SetFrame((combatManager.GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState].totalFrames);
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
            return (combatManager.GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState];
        }
        
        public HnSF.StateTimeline GetState(int state)
        {
            return (combatManager.GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[state];
        }

        public string GetCurrentStateName()
        {
            return (combatManager.GetMoveset(CurrentStateMoveset) as rwby.Moveset).stateMap[CurrentState].name;
        }

        public void IncrementFrame()
        {
            CurrentStateFrame++;
            director.time = (float)CurrentStateFrame * Runner.Simulation.DeltaTime;
        }

        public void SetFrame(int frame)
        {
            CurrentStateFrame = frame;
            director.time = (float)frame * Runner.Simulation.DeltaTime;
        }
    }
}