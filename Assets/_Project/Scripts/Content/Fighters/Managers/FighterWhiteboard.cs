using Cysharp.Threading.Tasks;
using Fusion;
using HnSF.Fighters;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterWhiteboard : NetworkBehaviour
    {
        [Networked, Capacity(5)] public NetworkArray<int> Ints => default;
        [Networked, Capacity(5)] public NetworkArray<float> Floats => default;

        public void UpdateInt(int index, int value)
        {
            Ints.Set(index, value);
        }

        public void UpdateFloat(int index, float value)
        {
            Floats.Set(index, value);
        }
    }
}