using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [OrderAfter(typeof(FighterStateManager))]
    public class FighterProjectileManager : NetworkBehaviour
    {
        [HideInInspector] public Dictionary<ModObjectSetContentReference, int> bankMap = new Dictionary<ModObjectSetContentReference, int>();
        [HideInInspector] public List<IProjectilebankDefinition> banks = new List<IProjectilebankDefinition>();
        
        [Networked, Capacity(10)] public NetworkLinkedList<NetworkObject> projectiles => default;
        [Networked, Capacity(10)] public NetworkLinkedList<ProjectileOverrideMode> overrideMode => default;
        [Networked] public int latestProjectileIndex { get; set; } = 0;

        public void CreateProjectile(CreateProjectileDefinition projectileCreateDefinition, Vector3 posBase = default, bool trackProjectile = true)
        {
            latestProjectileIndex++;
            if (latestProjectileIndex >= 10) latestProjectileIndex = 0;
            if (projectiles[latestProjectileIndex] != null)
            {
                if(overrideMode[latestProjectileIndex] == ProjectileOverrideMode.REMOVE_AND_DESTROY) Runner.Despawn(projectiles[latestProjectileIndex], true);
            }

            overrideMode.Set(latestProjectileIndex, projectileCreateDefinition.overrideMode);

            int bank = bankMap[projectileCreateDefinition.projectilebank];
            int projectileInx = banks[bank].ProjectileMap[projectileCreateDefinition.projectile] + 1;
            
            Vector3 spawnPos = posBase +
                               (projectileCreateDefinition.offset.x * transform.right)
                               + (projectileCreateDefinition.offset.z * transform.forward)
                               + (projectileCreateDefinition.offset.y * transform.up);
            Vector3 spawnRot = transform.eulerAngles + projectileCreateDefinition.rotation;
            var predictionKey = new NetworkObjectPredictionKey() { Byte0 = (byte) Runner.Simulation.Tick, Byte1 = (byte)Object.InputAuthority.PlayerId, Byte2 = (byte)latestProjectileIndex, Byte3 = (byte)(bank+projectileInx) };
            
            var projectileObj = Runner.Spawn(GetProjectile(projectileCreateDefinition.projectilebank, projectileCreateDefinition.projectile), spawnPos, Quaternion.Euler(spawnRot),
                Object.InputAuthority, predictionKey: predictionKey, onBeforeSpawned: (runner, o) =>
                {
                    InitializeProjectile(runner, o, bank, projectileInx); 
                });

            projectiles.Set(latestProjectileIndex, projectileObj.Object);
        }

        private void InitializeProjectile(NetworkRunner runner, NetworkObject networkObject, int bank, int projectileInx)
        {
            BaseProjectile bp = networkObject.GetBehaviour<BaseProjectile>();
            bp.bank = bank;
            bp.projectile = projectileInx;
            bp.owner = Object;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
        }
        
        public BaseProjectile GetProjectile(ModObjectSetContentReference projectilebank, string projectile)
        {
            return banks[bankMap[projectilebank]].GetProjectile(projectile).baseProjectile;
        }

        public void RegisterBank(ModObjectSetContentReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            banks.Add(ContentManager.singleton.GetContentDefinition<IProjectilebankDefinition>(
                ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference(bank.modGUID, (int)ContentType.Projectilebank, bank.contentGUID))));
            bankMap.Add(bank, banks.Count-1);
        }
    }
}