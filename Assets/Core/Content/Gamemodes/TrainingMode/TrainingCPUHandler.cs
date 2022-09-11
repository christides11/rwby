using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using rwby.core.training;
using UnityEngine;

namespace rwby.core.training
{
    [OrderAfter(typeof(ClientManager))]
    [OrderBefore(typeof(FighterInputManager))]
    public class TrainingCPUHandler : NetworkBehaviour, IInputProvider, IFighterCallbacks
    {
        public delegate void EmptyAction(TrainingCPUHandler cpuHandler);
        public event EmptyAction OnCPUListUpdated;
    
        [Networked(OnChanged = nameof(CpuListUpdated)), Capacity(4)] public NetworkArray<TrainingCPUReference> cpus { get; }
        [Networked(OnChanged = nameof(CpuSettingsUpdated)), Capacity(4)] public NetworkArray<TrainingCPUSettingsDefinition> cpuSettings { get; }

        public NetworkPlayerInputData[] testData = new NetworkPlayerInputData[4];
        
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
                            b.GetBehaviour<FighterInputManager>().inputProvider = Object;
                            b.GetBehaviour<FighterInputManager>().inputEnabled = true;
                            b.GetBehaviour<FighterManager>().callbacks = this;
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
            
            for (int i = 0; i < cpus.Length; i++)
            {
                NetworkPlayerInputData id = new NetworkPlayerInputData();
                if (!cpus[i].objectId.IsValid)
                {
                    testData[i] = id;
                    continue;
                }

                if (cpuSettings[i].behaviour == 0)
                {
                    switch (cpuSettings[i].block)
                    {
                        case 0:
                            break;
                        case 4:
                            id.buttons.Set(PlayerInputType.BLOCK, true);
                            break;
                    }
                }

                testData[i] = id;
            }
        }
        
        public NetworkPlayerInputData GetInput(int inputIndex)
        {
            return testData[inputIndex];
        }

        public void FighterHealthChanged(FighterManager fm)
        {
            if(fm.HealthManager.Health <= 0) fm.HealthManager.SetHealth(fm.fighterDefinition.Health);
        }
    }
}