using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct IDGroupCollisionInfo : INetworkStruct
{
    public IDGroupCollisionType collisionType;
    public NetworkId hitIHurtableNetID;
    public int hitByIDGroup;
}

public enum IDGroupCollisionType
{
    Hurtbox,
    Hitbox
}