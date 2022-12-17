using System;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct FighterSoundNode : INetworkStruct, IEquatable<rwby.FighterSoundNode>
    {
        public int bank;
        public int sound;
        public int createFrame;
        public NetworkBool parented;
        public Vector3 pos;
        public float volume;
        public float minDist;
        public float maxDist;
        public float pitch;

        public bool Equals(FighterSoundNode other)
        {
            return bank == other.bank
                   && sound == other.sound
                   && createFrame == other.createFrame
                   && parented.Equals(other.parented)
                   && pos.Equals(other.pos)
                   && volume.Equals(other.volume)
                   && minDist.Equals(other.minDist)
                   && maxDist.Equals(other.maxDist)
                   && pitch.Equals(other.pitch);
        }

        public override bool Equals(object obj)
        {
            return obj is FighterEffectNode other && Equals(other);
        }
        
        public static bool operator ==(rwby.FighterSoundNode a, rwby.FighterSoundNode b)
        {
            return a.bank == b.bank
                   && a.sound == b.sound
                   && a.createFrame == b.createFrame
                   && a.parented == b.parented
                   && a.pos == b.pos
                   && a.volume == b.volume
                   && a.minDist == b.minDist
                   && a.maxDist == b.maxDist
                   && a.pitch == b.pitch;
        }

        public static bool operator !=(rwby.FighterSoundNode a, rwby.FighterSoundNode b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(bank, sound, createFrame, parented, pos, volume, minDist, maxDist);
        }
    }
}