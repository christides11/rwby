using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct CreateProjectileDefinition
    {
        public SharedModSetContentReference projectilebank;
        public string projectile;
        public bool parented;
        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale;
        public ProjectileOverrideMode overrideMode;
    }
}