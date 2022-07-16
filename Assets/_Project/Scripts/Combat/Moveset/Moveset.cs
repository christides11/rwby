using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace rwby
{
    [CreateAssetMenu(fileName = "Moveset", menuName = "rwby/Combat/Moveset")]
    public class Moveset : HnSF.Combat.MovesetDefinition
    {
        public FighterStats fighterStats;

        public SpecialDefinition[] specials;
        
        public virtual void Initialize()
        {
            if (stateMap == null)
            {
                stateMap = new Dictionary<int, HnSF.StateTimeline>();
            }

            if (stateMap.Count == states.Length) return;
            stateMap.Clear();
            foreach (var intStateMap in states)
            {
                intStateMap.stateTimeline.Initialize();
                stateMap.Add(intStateMap.state.GetState(), intStateMap.stateTimeline);
            }
        }
    }
}