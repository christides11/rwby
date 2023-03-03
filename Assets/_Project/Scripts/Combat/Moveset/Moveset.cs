using System;
using System.Collections;
using System.Collections.Generic;
using HnSF.Combat;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace rwby
{
    [CreateAssetMenu(fileName = "Moveset", menuName = "rwby/Combat/Moveset")]
    public class Moveset : ScriptableObject, IMovesetDefinition
    {
        public FighterStats fighterStats;

        public SpecialDefinition[] specials;

        public ExternalStateVariables clashCancelListGrd;
        public ExternalStateVariables clashCancelListAir;
        
        [NonSerialized] protected Dictionary<int, HnSF.StateTimeline> stateMap;
        [Header("States")] 
        [SerializeField] protected IntStateMap[] groundActions = Array.Empty<IntStateMap>();
        [SerializeField] protected IntStateMap[] groundAttacks = Array.Empty<IntStateMap>();
        [SerializeField] protected IntStateMap[] airActions = Array.Empty<IntStateMap>();
        [SerializeField] protected IntStateMap[] airAttacks = Array.Empty<IntStateMap>();
        [SerializeField] protected IntStateMap[] recovery = Array.Empty<IntStateMap>();
        [SerializeField] protected IntStateMap[] hitstun = Array.Empty<IntStateMap>();
        
        public virtual void Initialize()
        {
            if (stateMap == null) stateMap = new Dictionary<int, HnSF.StateTimeline>();
            if (stateMap.Count ==
                groundActions.Length + airActions.Length + groundAttacks.Length + airAttacks.Length
                + recovery.Length + hitstun.Length) return;
            stateMap.Clear();

            foreach (var action in groundActions)
            {
                action.stateTimeline.Initialize();
                stateMap.Add(action.state.GetState(), action.stateTimeline);
            }
            
            foreach (var action in airActions)
            {
                action.stateTimeline.Initialize();
                stateMap.Add(action.state.GetState(), action.stateTimeline);
            }
            
            foreach (var action in groundAttacks)
            {
                action.stateTimeline.Initialize();
                try
                {
                    stateMap.Add(action.state.GetState(), action.stateTimeline);
                }
                catch(Exception e)
                {
                    Debug.LogError($"{name}:{action.name} error: {e}", action.stateTimeline);
                }
            }
            
            foreach (var action in airAttacks)
            {
                action.stateTimeline.Initialize();
                stateMap.Add(action.state.GetState(), action.stateTimeline);
            }
            
            foreach (var action in recovery)
            {
                action.stateTimeline.Initialize();
                stateMap.Add(action.state.GetState(), action.stateTimeline);
            }
            
            foreach (var action in hitstun)
            {
                action.stateTimeline.Initialize();
                stateMap.Add(action.state.GetState(), action.stateTimeline);
            }
        }
        
        public HnSF.StateTimeline GetState(int stateID)
        {
            return stateMap[stateID];
        }
    }
}