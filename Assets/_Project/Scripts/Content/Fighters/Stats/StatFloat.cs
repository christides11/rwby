using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public struct StatFloat : StatBase<float>, INetworkStruct
    {
        public float BaseValue { get { return baseValue; }  }

        NetworkBool dirty;
        float baseValue;

        public StatFloat(float value)
        {
            baseValue = value;
            dirty = true;
        }

        public float GetValue()
        {
            return baseValue;
        }

        public void UpdateBaseValue(float value)
        {
            baseValue = value;
            dirty = true;
        }

        public float GetCurrentValue()
        {
            return baseValue;
        }

        public void Dirty()
        {
            dirty = true;
        }

        public static implicit operator float(StatFloat f) => f.GetCurrentValue();
    }
}