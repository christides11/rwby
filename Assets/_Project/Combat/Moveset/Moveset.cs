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

        [Header("Attack Level System")]
        public List<MovesetAttackNode> groundLV1StartNodes;
        public List<MovesetAttackNode> groundLV2StartNodes;
        public List<MovesetAttackNode> groundLV3StartNodes;
        public List<MovesetAttackNode> airLV1StartNodes;
        public List<MovesetAttackNode> airLV2StartNodes;
        public List<MovesetAttackNode> airLV3StartNodes;
    }
}