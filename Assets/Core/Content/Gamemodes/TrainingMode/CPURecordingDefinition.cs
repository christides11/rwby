using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby.core.training
{
    [System.Serializable]
    public struct CPURecordingDefinition : INetworkStruct
    {
        [Networked, Capacity(600)] public NetworkLinkedList<NetworkPlayerInputData> buffer => default;
    }
}