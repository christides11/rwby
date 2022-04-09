using Fusion;
using UnityEngine;

namespace rwby
{
    public struct FighterAnimationRoot : INetworkStruct
    {
        public float weight;
        [Networked, Capacity(10)] public NetworkLinkedList<FighterAnimationNode> layer0 => default;
        
        public override bool Equals(object obj)
        {
            return obj is rwby.FighterAnimationRoot && this == (rwby.FighterAnimationRoot)obj;
        }
        
        public static bool operator ==(FighterAnimationRoot a, rwby.FighterAnimationRoot b)
        {
            if (a.layer0.Count != b.layer0.Count) return false;
            for (int i = 0; i < a.layer0.Count; i++)
            {
                if (a.layer0[i] != b.layer0[i]) return false;
            }
            return Mathf.Approximately(a.weight, b.weight);
        }

        public static bool operator !=(FighterAnimationRoot a, FighterAnimationRoot b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (weight, layer0).GetHashCode();
        }
    }
}