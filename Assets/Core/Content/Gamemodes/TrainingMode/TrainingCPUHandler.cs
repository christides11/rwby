using System;
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
        [System.Serializable]
        public class AttackOptionDefinition
        {
            public string nickname;
            public PlayerInputDataFlagged[] inputs = Array.Empty<PlayerInputDataFlagged>();
        }
        
        public delegate void EmptyAction(TrainingCPUHandler cpuHandler);
        public event EmptyAction OnCPUListUpdated;
    
        [Networked(OnChanged = nameof(CpuListUpdated)), Capacity(4)] public NetworkArray<TrainingCPUReference> cpus { get; }
        [Networked(OnChanged = nameof(CpuSettingsUpdated)), Capacity(4)] public NetworkArray<TrainingCPUSettingsDefinition> cpuSettings { get; }

        public NetworkPlayerInputData[][] testData = new NetworkPlayerInputData[4][];
        
        public GamemodeTraining gamemode;

        [Header("Settings")]
        public AttackOptionDefinition[] defaultAttackOptions = Array.Empty<AttackOptionDefinition>();

        public List<AttackOptionDefinition>[] cpuAtkOptions = new List<AttackOptionDefinition>[4];

        private void Awake()
        {
            for (int i = 0; i < testData.Length; i++)
            {
                testData[i] = new NetworkPlayerInputData[10];
                
                cpuAtkOptions[i] = new List<AttackOptionDefinition>();
                cpuAtkOptions[i] = new List<AttackOptionDefinition>(defaultAttackOptions);
            }
        }

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
                if(contentReference.IsValid() && !cpus[i].objectId.IsValid)
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
                            b.GetBehaviour<FighterManager>().DisableUpdate = true;
                            fManager.HealthManager.Health = fManager.fighterDefinition.Health;
                            var list = cpus;
                            TrainingCPUReference temp = list[indexTemp];
                            temp.objectId = b.Id;
                            list[indexTemp] = temp;
                        });
                }
            }
        }

        public int disabledUntil = -1;
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (Runner.Tick <= disabledUntil) return;
            
            for (int i = 0; i < cpus.Length; i++)
            {
                NetworkPlayerInputData id = new NetworkPlayerInputData();
                if (!cpus[i].objectId.IsValid)
                {
                    testData[i][Runner.Tick % 10] = id;
                    continue;
                }

                FighterManager fm =
                    Runner.TryGetNetworkedBehaviourFromNetworkedObjectRef<FighterManager>(cpus[i].objectId);
                
                switch (cpuSettings[i].status)
                {
                    case (int)CPUActionStatus.Jumping:
                    case (int)CPUActionStatus.Standing:
                        HandleGuard(i, ref id);
                        break;
                    case (int)CPUActionStatus.CPU:
                        break; 
                }

                testData[i][Runner.Tick % 10] = id;
                
                if (cpuSettings[i].afterHit != 0 && fm.FCombatManager.HitStun == 1)
                {
                    var opt = cpuAtkOptions[i][cpuSettings[i].afterHit - 1];
                    
                    for (int j = 0; j < opt.inputs.Length; j++)
                    {
                        var t = new NetworkPlayerInputData();

                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.A))
                        {
                            t.buttons.Set((int)PlayerInputType.A, true);
                        }
                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.B))
                        {
                            t.buttons.Set((int)PlayerInputType.B, true);
                        }
                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.C))
                        {
                            t.buttons.Set((int)PlayerInputType.C, true);
                        }
                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.DASH))
                        {
                            t.buttons.Set((int)PlayerInputType.DASH, true);
                        }

                        t.movement = opt.inputs[j].movement;
                        t.forward = fm.myTransform.forward;
                        t.right = fm.myTransform.right;

                        testData[i][(Runner.Tick + j) % 10] = t;
                    }

                    disabledUntil = Runner.Tick + opt.inputs.Length+1;
                }

                if (cpuSettings[i].afterBlock != 0 && fm.FCombatManager.BlockStun == 1)
                {
                    var opt = cpuAtkOptions[i][cpuSettings[i].afterBlock - 1];
                    
                    for (int j = 0; j < opt.inputs.Length; j++)
                    {
                        var t = new NetworkPlayerInputData();

                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.A))
                        {
                            t.buttons.Set((int)PlayerInputType.A, true);
                        }
                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.B))
                        {
                            t.buttons.Set((int)PlayerInputType.B, true);
                        }
                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.C))
                        {
                            t.buttons.Set((int)PlayerInputType.C, true);
                        }
                        if (opt.inputs[j].buttons.HasFlag(PlayerInputTypeFlags.DASH))
                        {
                            t.buttons.Set((int)PlayerInputType.DASH, true);
                        }

                        t.movement = opt.inputs[j].movement;
                        t.forward = fm.myTransform.forward;
                        t.right = fm.myTransform.right;

                        testData[i][(Runner.Tick + j) % 10] = t;
                    }

                    disabledUntil = Runner.Tick + opt.inputs.Length+1;
                }
            }
        }

        private void HandleGuard(int index, ref NetworkPlayerInputData inputData)
        {
            switch (cpuSettings[index].guard)
            {
                case (int)CPUGuardStatus.Hold_Guard:
                    inputData.buttons.Set(PlayerInputType.BLOCK, true);
                    break;
            }
        }

        public NetworkPlayerInputData GetInput(int inputIndex)
        {
            return testData[inputIndex][Runner.Tick % 10];
        }

        public void FighterHealthChanged(FighterManager fm)
        {
            if(fm.HealthManager.Health <= 0) fm.HealthManager.SetHealth(fm.fighterDefinition.Health);
        }
    }
}