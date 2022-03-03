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
        [Networked] public int CurrentStateMoveset { get; set; } = 0;
        [Networked] public int CurrentState { get; set; } = 0;
        [Networked] public int CurrentStateFrame { get; set; } = 0;
        
        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCombatManager combatManager;

        public PlayableDirector director;
        
        public bool markedForStateChange = false;
        public int nextState = 0;

        public void ResimulationSync()
        {
            var currentState = manager.FCombatManager.GetMoveset().stateMap[CurrentState];
            if (currentState == director.playableAsset) return;
            InitState();
        }
        
        public void Tick()
        {
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
            Debug.Log("Cannot remove states.");

        }

        public void MarkForStateChange(int state)
        {
            markedForStateChange = true;
            nextState = state;
        }
        
        public bool ChangeState(int state, int stateFrame = 0, bool callOnInterrupt = true)
        {
            ChangeState(manager.FCombatManager.CurrentMovesetIdentifier, state, stateFrame, callOnInterrupt);
            return true;
        }

        public void ChangeState(int stateMoveset, int state, int stateFrame = 0, bool callOnInterrupt = true)
        {
            markedForStateChange = false;
            int oldStateMoveset = CurrentStateMoveset;
            int oldState = CurrentState;
            if (callOnInterrupt && oldState != 0)
            {
                // TODO: Use old state moveset.
                SetFrame(manager.FCombatManager.GetMoveset().stateMap[oldState].totalFrames);
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
            if (CurrentState == 0)
            {
                director.playableAsset = null;
                return;
            }

            director.playableAsset = GetState();
            foreach (var pAO in director.playableAsset.outputs)
            {
                director.SetGenericBinding(pAO.sourceObject, manager);
            }
            director.Play();
            //SetFrame(0);
            //director.Evaluate();
        }

        public StateTimeline GetState()
        {
            return manager.FCombatManager.GetMoveset().stateMap[CurrentState];
        }
        
        public HnSF.StateTimeline GetState(int state)
        {
            return manager.FCombatManager.GetMoveset().stateMap[state];
        }

        public string GetCurrentStateName()
        {
            return manager.FCombatManager.GetMoveset().stateMap[CurrentState].name;
        }

        public void IncrementFrame()
        {
            CurrentStateFrame++;
            director.time = (float)CurrentStateFrame * Runner.Simulation.DeltaTime;
        }

        public void SetFrame(int frame)
        {
            CurrentStateFrame = frame;
            director.time = (float)CurrentStateFrame * Runner.Simulation.DeltaTime;
        }
    }
}