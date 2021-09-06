using Fusion;
using rwby.fighters.states;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.core.content
{
    public class RubyRoseManager : FighterManager
    {
        [Networked] public Vector3 TeleportPosition { get; set; }

        protected override void SetupStates()
        {
            stateManager.AddState(new RRTeleport(), (ushort)RubyRoseStates.TELEPORT);
            base.SetupStates();
        }
    }
}