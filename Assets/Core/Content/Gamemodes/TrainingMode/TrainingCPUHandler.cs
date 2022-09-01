using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using rwby.core.training;
using UnityEngine;

namespace rwby
{
    [OrderAfter(typeof(GamemodeTraining))]
    public class TrainingCPUHandler : NetworkBehaviour, IInputProvider
    {
        public delegate void EmptyAction(TrainingCPUHandler cpuHandler);
        public event EmptyAction OnCPUListUpdated;
    
        [Networked(OnChanged = nameof(CpuListUpdated)), Capacity(4)] public NetworkArray<TrainingCPUReference> cpus { get; }
        [Networked(OnChanged = nameof(CpuSettingsUpdated)), Capacity(4)] public NetworkArray<TrainingCPUSettingsDefinition> cpuSettings { get; }

        public GamemodeTraining gamemode;
        
        private static void CpuListUpdated(Changed<TrainingCPUHandler> changed)
        {
            changed.Behaviour.OnCPUListUpdated?.Invoke(changed.Behaviour);
            _ = changed.Behaviour.CheckCPUList();
        }
        
        private static void CpuSettingsUpdated(Changed<TrainingCPUHandler> changed)
        {
            
        }

        private async UniTask CheckCPUList()
        {
            if (Object.HasStateAuthority == false) return;
            
            for(int i = 0; i < cpus.Length; i++)
            {
                ModGUIDContentReference contentReference = cpus[i].characterReference;
                if(contentReference.IsValid() && cpus[i].objectId.IsValid == false)
                {
                    List<PlayerRef> failedLoadPlayers = await gamemode.sessionManager.clientContentLoaderService.TellClientsToLoad<IFighterDefinition>(contentReference);
                    if (failedLoadPlayers == null)
                    {
                        Debug.LogError($"Load CPU {contentReference} failure.");
                        continue;
                    }

                    int indexTemp = i;
                    IFighterDefinition fighterDefinition = ContentManager.singleton.GetContentDefinition<IFighterDefinition>(contentReference);
                    NetworkObject no = Runner.Spawn(fighterDefinition.GetFighter().GetComponent<NetworkObject>(), Vector3.up, Quaternion.identity, null,
                        (a, b) =>
                        {
                            b.gameObject.name = $"CPU.{b.Id} : {fighterDefinition.Name}";
                            var fManager = b.GetBehaviour<FighterManager>();
                            b.GetBehaviour<FighterCombatManager>().Team = 0;
                            _ = b.GetBehaviour<FighterManager>().OnFighterLoaded();
                            fManager.HealthManager.Health = fManager.fighterDefinition.Health;
                            var list = cpus;
                            TrainingCPUReference temp = list[indexTemp];
                            temp.objectId = b.Id;
                            list[indexTemp] = temp;
                        });
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
        }

        public NetworkPlayerInputData GetInput(int inputIndex)
        {
            return new NetworkPlayerInputData();
        }
    }
}