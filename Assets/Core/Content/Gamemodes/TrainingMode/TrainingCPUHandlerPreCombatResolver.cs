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

                NetworkPlayerInputData id = cpuHandlerCore.testData[i];
                
                var temp = combatPairFinder.hitboxCombatPairs.Where(x =>
                    x.Key.Item2.Id == cpuHandlerCore.cpus[i].objectId).ToArray();
                
                
                FighterManager fm =
                    Runner.TryGetNetworkedBehaviourFromNetworkedObjectRef<FighterManager>(cpuHandlerCore.cpus[i]
                        .objectId);
                
                switch (cpuHandlerCore.cpuSettings[i].status)
                {
                    case (int)CPUActionStatus.Jumping:
                    case (int)CPUActionStatus.Standing:
                        switch (cpuHandlerCore.cpuSettings[i].guard)
                        {
                            case (int)CPUGuardStatus.Guard_All:
                                if (temp.Length != 0)
                                {
                                    id.buttons.Set(PlayerInputType.BLOCK, true);
                                }
                                break;
                        }

                        switch (cpuHandlerCore.cpuSettings[i].guardDirection)
                        {
                            case (int)CpuEnabledStateStatus.Disabled:
                                break;
                            case (int)CpuEnabledStateStatus.Enabled:
                                if (temp.Length != 0)
                                {
                                    id.forward = (temp[0].Key.Item1.transform.position - fm.transform.position).normalized;
                                    id.right = Vector3.Cross(id.forward, Vector3.up);
                                    id.camPos = fm.transform.position;
                                    id.movement = new Vector2(0, 1.0f);
                                }

                                break;
                        }
                        break;
                }

                cpuHandlerCore.testData[i] = id;
                
                fm.FighterUpdate();
            }
        }
    }
}