using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class AnimationbankContainer : NetworkBehaviour
    {
        public Dictionary<string, int> animationbankMap = new Dictionary<string, int>();
        public List<IAnimationbankDefinition> animationbanks = new List<IAnimationbankDefinition>();

        public void AddAnimationbank(string animationbankName, IAnimationbankDefinition animationbank)
        {
            animationbanks.Add(animationbank);
            animationbankMap.Add(animationbankName, animationbanks.Count - 1);
        }

        public IAnimationbankDefinition GetAnimationbank(string animationbankName)
        {
            return animationbanks[animationbankMap[animationbankName]];
        }
    }
}