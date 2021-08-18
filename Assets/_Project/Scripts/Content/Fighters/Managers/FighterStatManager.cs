using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace rwby
{
    public class FighterStatManager : NetworkBehaviour
    {
        [Networked] public StatFloat GroundFriction { get; set; }
        [Networked] public StatInt MinJumpFrames { get; set; }
        [Networked] public StatFloat MaxJumpTime { get; set; }
        [Networked] public StatFloat MaxJumpHeight { get; set; }
        [Networked] public StatInt JumpSquatFrames { get; set; }
        [Networked] public StatFloat MaxFallSpeed { get; set; }

        public void SetupStats()
        {
            MinJumpFrames = new StatInt(5);
            MaxJumpTime = new StatFloat(45.0f/60.0f);
            MaxJumpHeight = new StatFloat(4.0f);
            JumpSquatFrames = new StatInt(4);
            MaxFallSpeed = new StatFloat(20);
        }
    }
}