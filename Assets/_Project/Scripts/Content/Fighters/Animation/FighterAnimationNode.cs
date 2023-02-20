using System;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct FighterAnimationNode : INetworkStruct, IEquatable<rwby.FighterAnimationNode>
    {
        public int bank;
        public int animation;
        public float weight;
        public int frame;

        public override bool Equals(object obj)
        {
            return obj is rwby.FighterAnimationNode && this == (rwby.FighterAnimationNode)obj;
        }

        public bool Equals(rwby.FighterAnimationNode other)
        {
            return this == other;
        }

        public static bool operator ==(rwby.FighterAnimationNode a, rwby.FighterAnimationNode b)
        {
            return a.bank == b.bank 
                   && a.animation == b.animation
                   && Mathf.Approximately(a.weight, b.weight) &
                   a.frame == b.frame;
        }

        public static bool operator !=(rwby.FighterAnimationNode a, rwby.FighterAnimationNode b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (bank, animation, weight, frame).GetHashCode();
        }
    }
}