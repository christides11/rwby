using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace rwby
{
    [CreateAssetMenu(fileName = "Moveset", menuName = "rwby/Combat/Moveset")]
    public class Moveset: HnSF.Combat.MovesetDefinition
    {
        //public HurtboxCollection hurtboxCollection;
        //public AnimationReferenceHolder animationCollection;
        public FighterStats fighterStats;

        [Header("Abilities")]
        public List<MovesetAttackNode> groundAbilityNodes;
        public List<MovesetAttackNode> airAbilityNodes;
    }
}