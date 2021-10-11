using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct IDGroupCollisionInfo : INetworkStruct
{
    public int hitByIDGroup;
    public NetworkId hitIHurtableNetID;
}