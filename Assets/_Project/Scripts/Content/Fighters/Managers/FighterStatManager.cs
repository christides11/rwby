using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace rwby
{
    public class FighterStatManager : NetworkBehaviour
    {

        public float GroundFriction = 0;
        public float JumpSquatFriction = 0;

        // Walking
        public float WalkMinSpeed = 0;
        public float WalkMaxSpeed;
        public float WalkBaseAccel;
        public float WalkAcceleration;
        public float walkRotationSpeed;
        public AnimationCurve WalkAccelFromDot;

        // Running
        public float RunMinSpeed;
        public float RunMaxSpeed;
        public float RunBaseAccel;
        public float RunAcceleration;
        public float runRotationSpeed;
        public AnimationCurve RunAccelFromDot;

        // Jump
        public int MinJumpFrames;
        public float MaxJumpTime;
        public float MaxJumpHeight;
        public int JumpSquatFrames;
        public float JumpConversedMomentum;
        public float JumpHorizontalVelocity;
        public float fallGravityMultiplier;

        // Air Jump
        public int airJumps;
        public int minAirJumpTime;
        public float MaxAirJumpTime;
        public float MaxAirJumpHeight;
        public float AirJumpHorizontalMomentum;
        public float AirJumpConversedMomentum;

        // Wall Jump
        public float MaxWallJumpTime;
        public float MaxWallJumpHeight;
        public float WallJumpConversedMomentum;
        public float WallJumpHorizontalMomentum;

        // Aerial
        public float MaxFallSpeed;
        public float AerialMaxSpeed;
        public float AerialBaseAcceleration;
        public float AerialAcceleration;
        public float AerialDeceleration;
        public AnimationCurve AerialAccelFromDot;
        public float hitstunGravity;
        public float aerialRotationSpeed;

        // Air dash
        public int airDashes;
        public int airDashFrames;
        public int airDashStartup = 3;
        public float airDashMaxMagnitude;
        public float airDashForce;
        public int airDashGravityDelay;
        public int airDashFrictionDelay;
        public float airDashFriction;

        // Wall run
        public int wallRunHorizontalTime;
        public float wallRunHorizontalSpeed;
        public float wallRunGravity;
        public AnimationCurve wallRunGravityCurve;

        public void SetupStats(FighterStats cmnStats)
        {
            GroundFriction = new StatFloat(cmnStats.groundFriction);
            JumpSquatFriction = new StatFloat(cmnStats.jumpSquatFriction);

            WalkBaseAccel = new StatFloat(cmnStats.walkBaseAcceleration);
            WalkAcceleration = new StatFloat(cmnStats.walkAcceleration);
            WalkMinSpeed = cmnStats.walkMinSpeed;
            WalkMaxSpeed = new StatFloat(cmnStats.walkMaxSpeed);
            WalkAccelFromDot = cmnStats.walkAccelerationFromDot;
            walkRotationSpeed = cmnStats.walkRotationSpeed;

            RunBaseAccel = new StatFloat(cmnStats.runBaseAcceleration);
            RunAcceleration = new StatFloat(cmnStats.runAcceleration);
            RunMinSpeed = cmnStats.runMinSpeed;
            RunMaxSpeed = new StatFloat(cmnStats.runMaxSpeed);
            RunAccelFromDot = cmnStats.runAccelerationFromDot;
            runRotationSpeed = cmnStats.runRotationSpeed;

            MinJumpFrames = new StatInt(cmnStats.jumpMinimumFrames);
            MaxJumpTime = new StatFloat(((float)cmnStats.jumpMaxTime)/60.0f);
            MaxJumpHeight = new StatFloat(cmnStats.jumpMaxHeight);
            JumpSquatFrames = new StatInt(cmnStats.jumpSquatFrames);
            JumpHorizontalVelocity = cmnStats.jumpHorizontalVelocity;
            JumpConversedMomentum = cmnStats.jumpConversedHorizontalMomentum;
            fallGravityMultiplier = cmnStats.fallGravityMultiplier;
            

            airJumps = cmnStats.airJumps;
            minAirJumpTime = cmnStats.airJumpMinimumFrames;
            MaxAirJumpTime = new StatFloat(((float)cmnStats.airJumpMaxTime)/60.0f);
            MaxAirJumpHeight = new StatFloat(cmnStats.airJumpMaxHeight);
            AirJumpHorizontalMomentum = new StatFloat(cmnStats.airJumpHorizontalMomentum);
            AirJumpConversedMomentum = cmnStats.airJumpConversedHorizontalMomentum;

            MaxWallJumpTime = (float)cmnStats.MaxWallJumpTime / 60.0f;
            MaxWallJumpHeight = cmnStats.MaxWallJumpHeight;
            WallJumpConversedMomentum = cmnStats.WallJumpConversedMomentum;
            WallJumpHorizontalMomentum = cmnStats.WallJumpHorizontalMomentum;

            MaxFallSpeed = new StatFloat(cmnStats.aerialMaxFallSpeed);
            AerialMaxSpeed = new StatFloat(cmnStats.aerialMaxSpeed);
            AerialBaseAcceleration = new StatFloat(cmnStats.aerialBaseAcceleration);
            AerialAcceleration = new StatFloat(cmnStats.aerialAcceleration);
            AerialDeceleration = new StatFloat(cmnStats.aerialDeceleration);
            AerialAccelFromDot = cmnStats.aerialAccelerationFromDot;
            hitstunGravity = cmnStats.hitstunGravity;
            aerialRotationSpeed = cmnStats.aerialRotationSpeed;

            airDashes = cmnStats.airDashes;
            airDashFrames = cmnStats.airDashFrames;
            airDashStartup = cmnStats.airDashStartup;
            airDashMaxMagnitude = cmnStats.airDashMaxMagnitude;
            airDashForce = cmnStats.airDashForce;
            airDashGravityDelay = cmnStats.airDashGravityDelay;
            airDashFrictionDelay = cmnStats.airDashFrictionDelay;
            airDashFriction = cmnStats.airDashFriction;

            wallRunHorizontalTime = cmnStats.wallRunHorizontalTime;
            wallRunHorizontalSpeed = cmnStats.wallRunHorizontalSpeed;
            wallRunGravity = cmnStats.wallRunGravity;
            wallRunGravityCurve = cmnStats.wallRunGravityCurve;
        }
    }
}