using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct FighterSoundsRoot : INetworkStruct
    {
        [Networked, Capacity(10)] public NetworkArray<FighterSoundNode> sounds => default;
    }
}