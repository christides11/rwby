using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

namespace rwby.core.training
{
    [OrderAfter(typeof(CombatPairFinder))]
    [OrderBefore(typeof(CombatPairResolver))]
    public class TrainingCPUHandlerPreCombatResolver : NetworkBehaviour
    {
        public GamemodeTraining gamemode;
        public TrainingCPUHandler cpuHandlerCore;
        public CombatPairFinder combatPairFinder;
        
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            for (int i = 0; i < cpuHandlerCore.cpus.Length; i++)
            {
                if (!cpuHandlerCore.cpus[i].objectId.IsValid) continue;

                if (cpuHandlerCore.cpuSettings[i].behaviour == 0 && cpuHandlerCore.cpuSettings[i].block == 1)
                {
                    switch (cpuHandlerCore.cpuSettings[i].block)
                    {
                        case 0:
                            break;
                        case 4:
                            
                            break;
                    }
                    var temp = combatPairFinder.hitboxCombatPairs.Where(x =>
                        x.Key.Item2.Id == cpuHandlerCore.cpus[i].objectId).ToArray();

                    if (temp.Length != 0)
                    {
                        
                    }
                }

                /*
                foreach (var valuePair in temp)
                {
                    //combatPairFinder.hitboxCombatPairs.Remove(valuePair.Key);
                }*/
            }
        }
    }
}