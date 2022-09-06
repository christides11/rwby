using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "ProjectilebankDefinition", menuName = "rwby/Content/Addressables/Projectilebank")]
    public class AddressablesProjectilebankDefinition : IProjectilebankDefinition
    {
        public override string Name { get { return projectilebankName; } }
        public override List<ProjectilebankEntry> Projectiles { get { return projectiles; } }
        public override Dictionary<string, int> ProjectileMap { get { return projectileMap; } }
        [SerializeField] private string projectilebankName;
        [SerializeField] private List<ProjectilebankEntry> projectiles = new List<ProjectilebankEntry>();

        [NonSerialized] public Dictionary<string, int> projectileMap = new Dictionary<string, int>();

        private void OnValidate()
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].index = i;
            }
        }

        private void OnEnable()
        {
            projectileMap.Clear();
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectileMap.Add(projectiles[i].id, i);
            }
        }
    }
}