using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class HealthManager : NetworkBehaviour
    {
        public delegate void EmptyDelegate(HealthManager healthManager);

        public event EmptyDelegate OnHealthIncreased;
        public event EmptyDelegate OnHealthDecreased;
        
        [Networked(OnChanged = nameof(OnChangedHealth))] public int Health { get; set; }
        
        public static void OnChangedHealth(Changed<HealthManager> changed)
        {
            changed.LoadOld();
            int oldHealth = changed.Behaviour.Health;
            changed.LoadNew();
            if (changed.Behaviour.Health > oldHealth)
            {
                changed.Behaviour.OnHealthIncreased?.Invoke(changed.Behaviour);
            }

            if (changed.Behaviour.Health < oldHealth)
            {
                changed.Behaviour.OnHealthDecreased?.Invoke(changed.Behaviour);
            }
        }

        public void ModifyHealth(int amt)
        {
            Health += amt;
        }
    }
}