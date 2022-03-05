using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace rwby
{
    [CreateAssetMenu(fileName = "Moveset", menuName = "rwby/Combat/Moveset")]
    public class Moveset: HnSF.Combat.MovesetDefinition
    {
        public FighterStats fighterStats;

        [Header("Abilities")]
        public List<MovesetAttackNode> groundAbilityNodes;
        public List<MovesetAttackNode> airAbilityNodes;

        [NonSerialized] public Dictionary<int, StateTimeline> stateMap;
        [Header("States")] [SerializeField] protected IntStateMap[] states = Array.Empty<IntStateMap>();

        public virtual void Initialize()
        {
            if (stateMap == null)
            {
                stateMap = new Dictionary<int, StateTimeline>();
            }

            if (stateMap.Count == states.Length) return;
            stateMap.Clear();
            foreach (var intStateMap in states)
            {
                stateMap.Add(intStateMap.state.GetState(), intStateMap.stateTimeline);
            }
        }
    }
}