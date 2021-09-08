using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SWallRunV : FighterState
    {
        public override string GetName()
        {
            return "Wall Run (Vertical)";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.PhysicsManager.forceMovement = Vector3.zero;
            manager.PhysicsManager.forceGravity = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override bool CheckInterrupt()
        {
            return base.CheckInterrupt();
        }
    }
}