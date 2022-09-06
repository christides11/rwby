using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class ProjectilebankEntry
    {
        [HideInInspector] public int index;
        public string id;
        public BaseProjectile baseProjectile;
    }
}