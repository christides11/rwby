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
        [Networked, Capacity(8)] public NetworkArray<int> Ints => default;
        [Networked, Capacity(8)] public NetworkArray<float> Floats => default;

        public void UpdateInt(int index, WhiteboardModifyTypes modifyType, int value)
        {
            switch (modifyType)
            {
                case WhiteboardModifyTypes.SET:
                    Ints.Set(index, value);
                    break;
                case WhiteboardModifyTypes.ADD:
                    Ints.Set(index, Ints[index] + value);
                    break;
                case WhiteboardModifyTypes.SUBTRACT:
                    Ints.Set(index, Ints[index] - value);
                    break;
                case WhiteboardModifyTypes.MULTIPLY:
                    Ints.Set(index, Ints[index] * value);
                    break;
                case WhiteboardModifyTypes.DIVIDE:
                    Ints.Set(index, Ints[index] / value);
                    break;
            }
        }

        public void UpdateFloat(int index, float value)
        {
            Floats.Set(index, value);
        }
    }
}