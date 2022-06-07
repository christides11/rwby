using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct FighterEffectsRoot : INetworkStruct
    {
        [Networked, Capacity(10)] public NetworkArray<FighterEffectNode> effects => default;
    }
}