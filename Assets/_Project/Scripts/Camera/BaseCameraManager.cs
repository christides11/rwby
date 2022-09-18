using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fusion;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterInputManager), typeof(FighterManager))]
    public class BaseCameraManager : SimulationBehaviour
    {
        [ReadOnly] public int id;
        [ReadOnly] public DummyCamera cam;
        [ReadOnly] public bool active;

        public virtual void Initialize(CameraSwitcher switcher)
        {
            
        }
        
        public virtual void Activate()
        {
            active = true;
        }
        
        public virtual void AssignControlTo(ClientManager clientManager, int playerID)
        {
            
        }

        public virtual void SetTarget(FighterManager fighterManager)
        {
            
        }

        public virtual void ShakeCamera(float strength, float time)
        {
            
        }

        public virtual void StopShaking()
        {
            
        }
        
        public virtual void Deactivate()
        {
            active = false;
        }
    }
}