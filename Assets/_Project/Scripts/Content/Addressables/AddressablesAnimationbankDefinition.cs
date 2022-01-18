using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "AnimationbankDefinition", menuName = "rwby/Content/Addressables/AnimationbankDefinition")]
    public class AddressablesAnimationbankDefinition : IAnimationbankDefinition
    {
        public override string Name { get { return animationbankName; } }
        public override List<AnimationbankAnimationEntry> Animations { get { return animations; } }
        public override Dictionary<string, int> AnimationMap { get { return animationMap; } }

        [SerializeField] private string animationbankName;
        [SerializeField] private List<AnimationbankAnimationEntry> animations = new List<AnimationbankAnimationEntry>();

        public Dictionary<string, int> animationMap = new Dictionary<string, int>();

        private void OnValidate()
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].index = i;
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animationMap.Add(animations[i].id, i);
            }
        }
    }
}