using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public struct StatInt : StatBase<int>, INetworkStruct
    {
        public int BaseValue { get { return baseValue; } }

        NetworkBool dirty;
        int baseValue;

        public StatInt(int value)
        {
            baseValue = value;
            dirty = true;
        }

        public float GetValue()
        {
            return baseValue;
        }

        public void UpdateBaseValue(int value)
        {
            baseValue = value;
            dirty = true;
        }

        public int GetCurrentValue()
        {
            return baseValue;
        }

        public void Dirty()
        {
            dirty = true;
        }

        public static implicit operator int(StatInt f) => f.GetCurrentValue();
    }
}