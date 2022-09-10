using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class BaseCameraManager : SimulationBehaviour
    {
        public int id;
        public DummyCamera cam;

        public bool active;

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
        
        public virtual void Deactivate()
        {
            active = false;
        }
    }
}