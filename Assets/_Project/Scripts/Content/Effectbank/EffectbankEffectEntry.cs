using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class EffectbankEffectEntry
    {
        [HideInInspector] public int index;
        public string id;
        public BaseEffect effect;
    }
}