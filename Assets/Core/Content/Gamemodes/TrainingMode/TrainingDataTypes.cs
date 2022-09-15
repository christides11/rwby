using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.core.training
{
    public enum CPUActionStatus
    {
        Standing,
        Jumping,
        CPU,
        Controller
    }

    public enum CPUGuardStatus
    {
        No_Guard,
        Guard_All,
        Guard_After_First,
        Guard_Only_First,
        Random,
        Hold_Guard
    }

    public enum CpuEnabledStateStatus
    {
        Disabled,
        Enabled
    }

    public enum CPUThrowClashStatus
    {
        Disabled,
        Enabled,
        Random
    }

    public enum CPUCounterhitStatus
    {
        Default,
        All_Hits,
        Random
    }
}