using System;

namespace rwby
{
    [System.Serializable]
    [Flags]
    public enum StateGroundedGroupType
    {
        NONE = 0,
        GROUND = 1 << 0,
        AERIAL = 1 << 1
    }
}