using System;

namespace rwby
{
    [System.Serializable]
    [Flags]
    public enum StateGroupType
    {
        NONE = 0,
        GROUND = 1 << 0,
        AERIAL = 1 << 1
    }
}