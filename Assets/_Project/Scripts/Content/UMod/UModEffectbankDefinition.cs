using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "EffectbankDefinition", menuName = "rwby/Content/UMod/EffectbankDefinition")]
    public class UModEffectbankDefinition : IEffectbankDefinition
    {
        public override string Name { get { return effectbankName; } }
        public override List<EffectbankEffectEntry> Animations { get { return effects; } }
        public override Dictionary<string, int> EffectMap { get { return effectMap; } }

        [SerializeField] private string effectbankName;
        [SerializeField] private List<EffectbankEffectEntry> effects = new List<EffectbankEffectEntry>();

        [NonSerialized] public Dictionary<string, int> effectMap = new Dictionary<string, int>();

        private void OnValidate()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].index = i;
            }
        }

        private void OnEnable()
        {
            effectMap.Clear();
            for (int i = 0; i < effects.Count; i++)
            {
                effectMap.Add(effects[i].id, i);
            }
        }
    }
}