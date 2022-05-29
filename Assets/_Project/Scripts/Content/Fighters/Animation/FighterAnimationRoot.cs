using Fusion;
using UnityEngine;

namespace rwby
{
    public struct FighterAnimationRoot : INetworkStruct
    {
        [Networked, Capacity(10)] public NetworkArray<FighterAnimationNode> layer0 => default;
        [Networked, Capacity(10)] public NetworkArray<FighterAnimationNode> fadeLayer => default;

        public override bool Equals(object obj)
        {
            return obj is rwby.FighterAnimationRoot root && this == root;
        }
        
        public static bool operator ==(FighterAnimationRoot a, rwby.FighterAnimationRoot b)
        {
            for (int i = 0; i < a.layer0.Length; i++)
            {
                if (a.layer0[i] != b.layer0[i]) return false;
            }
            for (int i = 0; i < a.fadeLayer.Length; i++)
            {
                if (a.fadeLayer[i] != b.fadeLayer[i]) return false;
            }

            return true;
            //return Mathf.Approximately(a.weight, b.weight);
        }

        public static bool operator !=(FighterAnimationRoot a, FighterAnimationRoot b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (layer0, fadeLayer).GetHashCode();
        }
    }
}