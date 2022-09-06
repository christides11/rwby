using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace rwby
{
    public class IProjectilebankDefinition : IContentDefinition
    {
        public virtual List<ProjectilebankEntry> Projectiles { get; }
        public virtual Dictionary<string, int> ProjectileMap { get; }

        public override UniTask<bool> Load()
        {
            throw new System.NotImplementedException();
        }

        public virtual ProjectilebankEntry GetProjectile(string projectile)
        {
            return Projectiles[ProjectileMap[projectile]];
        }

        public override bool Unload()
        {
            throw new System.NotImplementedException();
        }
    }
}