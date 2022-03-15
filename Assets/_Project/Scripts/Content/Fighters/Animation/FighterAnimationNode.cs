using System;
using Fusion;
using UnityEngine;

namespace rwby
{
    public struct FighterAnimationNode : INetworkStruct, IEquatable<rwby.FighterAnimationNode>
    {
        public ModObjectReference bank;
        public int animation;
        public float weight;
        public float currentTime;

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
            return a.bank == b.bank && a.animation == b.animation
                                    && Mathf.Approximately(a.weight, b.weight) &&
                                    Mathf.Approximately(a.currentTime, b.currentTime);
        }

        public static bool operator !=(rwby.FighterAnimationNode a, rwby.FighterAnimationNode b)
        {
            return !(a == b);
        }
    }
}