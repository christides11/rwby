using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    [OrderAfter(typeof(FighterCombatManager))]
    [OrderBefore(typeof(GameModeBase))]
    public class CombatPairFinder : SimulationBehaviour
    {

    }
}