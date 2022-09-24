using System;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "FighterStats", menuName = "rwby/fighter/stats")]
    public class FighterStats : ScriptableObject
    {
        [Serializable]
        public class UDictionaryFighterStatInt : UDictionary<FighterIntBaseStats, int> { }
        
        [Serializable]
        public class UDictionaryFighterStatFloat : UDictionary<FighterFloatBaseStats, float> { }
        
        [Serializable]
        public class UDictionaryFighterStatAnimationCurve : UDictionary<FighterAnimationCurveBaseStats, AnimationCurve> { }

        [UDictionary.Split(45,55)] public UDictionaryFighterStatInt intStats;
        [UDictionary.Split(45,55)] public UDictionaryFighterStatFloat floatStats;
        [UDictionary.Split(45,55)] public UDictionaryFighterStatAnimationCurve animationCurveStats;

        [Button]
        public void UpdateStats()
        {
            intStats.cache = null;
            floatStats.cache = null;
            animationCurveStats.cache = null;
        }
    }
}