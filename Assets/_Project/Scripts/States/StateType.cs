using System;

namespace rwby
{
    [System.Serializable]
    [Flags]
    public enum StateType
    {
        NONE = 0,
        MOVEMENT = 1 << 0,
        ATTACK = 1 << 1,
        RECOVERY = 1 << 2,
        HITSTUN = 1 << 3
    }
}