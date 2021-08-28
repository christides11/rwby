using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace rwby
{
    public class FighterStatManager : NetworkBehaviour
    {
        public FighterStats cmnStats;

        [Networked] public StatFloat GroundFriction { get; set; }
        [Networked] public StatFloat JumpSquatFriction { get; set; }

        // Walking
        [Networked] public StatFloat WalkMaxSpeed { get; set; }
        [Networked] public StatFloat WalkBaseAccel { get; set; }
        [Networked] public StatFloat WalkAcceleration { get; set; }
        public float walkRotationSpeed;
        public AnimationCurve WalkAccelFromDot;

        // Running
        [Networked] public StatFloat RunMaxSpeed { get; set; }
        [Networked] public StatFloat RunBaseAccel { get; set; }
        [Networked] public StatFloat RunAcceleration { get; set; }
        public float runRotationSpeed;
        public AnimationCurve RunAccelFromDot;

        // Jump
        [Networked] public StatInt MinJumpFrames { get; set; }
        [Networked] public StatFloat MaxJumpTime { get; set; }
        [Networked] public StatFloat MaxJumpHeight { get; set; }
        [Networked] public StatInt JumpSquatFrames { get; set; }
        public float fallGravityMultiplier;

        // Air Jump
        public int airJumps;
        public int minAirJumpTime;
        [Networked] public StatFloat MaxAirJumpTime { get; set; }
        [Networked] public StatFloat MaxAirJumpHeight { get; set; }
        [Networked] public StatFloat AirJumpHorizontalMomentum { get; set; }

        // Aerial
        [Networked] public StatFloat MaxFallSpeed { get; set; }
        [Networked] public StatFloat AerialMaxSpeed { get; set; }
        [Networked] public StatFloat AerialBaseAcceleration { get; set; }
        [Networked] public StatFloat AerialAcceleration { get; set; }  
        [Networked] public StatFloat AerialDeceleration {  get; set; }
        public AnimationCurve AerialAccelFromDot;
        public float hitstunGravity;

        // Air dash
        public int airDashes;
        public int airDashFrames;
        public int airDashStartup = 3;
        public float airDashMaxMagnitude;
        public float airDashForce;
        public int airDashGravityDelay;

        public void SetupStats()
        {
            GroundFriction = new StatFloat(cmnStats.groundFriction);
            JumpSquatFriction = new StatFloat(cmnStats.jumpSquatFriction);

            WalkBaseAccel = new StatFloat(cmnStats.walkBaseAcceleration);
            WalkAcceleration = new StatFloat(cmnStats.walkAcceleration);
            WalkMaxSpeed = new StatFloat(cmnStats.walkMaxSpeed);
            WalkAccelFromDot = cmnStats.walkAccelerationFromDot;
            walkRotationSpeed = cmnStats.walkRotationSpeed;

            RunBaseAccel = new StatFloat(cmnStats.runBaseAcceleration);
            RunAcceleration = new StatFloat(cmnStats.runAcceleration);
            RunMaxSpeed = new StatFloat(cmnStats.runMaxSpeed);
            RunAccelFromDot = cmnStats.runAccelerationFromDot;
            runRotationSpeed = cmnStats.runRotationSpeed;

            MinJumpFrames = new StatInt(cmnStats.jumpMinimumFrames);
            MaxJumpTime = new StatFloat(((float)cmnStats.jumpMaxTime)/60.0f);
            MaxJumpHeight = new StatFloat(cmnStats.jumpMaxHeight);
            JumpSquatFrames = new StatInt(cmnStats.jumpSquatFrames);
            fallGravityMultiplier = cmnStats.fallGravityMultiplier;

            airJumps = cmnStats.airJumps;
            minAirJumpTime = cmnStats.airJumpMinimumFrames;
            MaxAirJumpTime = new StatFloat(((float)cmnStats.airJumpMaxTime)/60.0f);
            MaxAirJumpHeight = new StatFloat(cmnStats.airJumpMaxHeight);
            AirJumpHorizontalMomentum = new StatFloat(cmnStats.airJumpHorizontalMomentum);

            MaxFallSpeed = new StatFloat(cmnStats.aerialMaxFallSpeed);
            AerialMaxSpeed = new StatFloat(cmnStats.aerialMaxSpeed);
            AerialBaseAcceleration = new StatFloat(cmnStats.aerialBaseAcceleration);
            AerialAcceleration = new StatFloat(cmnStats.aerialAcceleration);
            AerialDeceleration = new StatFloat(cmnStats.aerialDeceleration);
            AerialAccelFromDot = cmnStats.aerialAccelerationFromDot;
            hitstunGravity = cmnStats.hitstunGravity;

            airDashes = cmnStats.airDashes;
            airDashFrames = cmnStats.airDashFrames;
            airDashStartup = cmnStats.airDashStartup;
            airDashMaxMagnitude = cmnStats.airDashMaxMagnitude;
            airDashForce = cmnStats.airDashForce;
            airDashGravityDelay = cmnStats.airDashGravityDelay;
        }
    }
}