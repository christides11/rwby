using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace rwby
{
    public class FighterStatManager : NetworkBehaviour
    {
        //public Dictionary<FighterIntBaseStats, int> baseIntStats = new Dictionary<FighterIntBaseStats, int>();
        //public Dictionary<FighterFloatBaseStats, float> baseFloatStats = new Dictionary<FighterFloatBaseStats, float>();

        // Walking
        public AnimationCurve WalkAccelFromDot;

        // Running
        public AnimationCurve RunAccelFromDot;

        // Aerial
        public AnimationCurve AerialAccelFromDot;

        // Air dash

        // Wall run
        public AnimationCurve wallRunGravityCurve;

        private FighterStats fs;
        
        public FighterStats GetFighterStats()
        {
            return fs;
        }
        
        public void InitStats()
        {
            /*
            foreach (int i in Enum.GetValues(typeof(FighterIntBaseStats)))  
            {
                baseIntStats.Add((FighterIntBaseStats)i, 0);
            }

            foreach (int i in Enum.GetValues(typeof(FighterFloatBaseStats)))
            {
                baseFloatStats.Add((FighterFloatBaseStats)i, 0);
            }*/
        }
        
        public void SetupStats(FighterStats cmnStats)
        {
            fs = cmnStats;
            /*
            baseFloatStats[FighterFloatBaseStats.GENERAL_groundFriction] = cmnStats.groundFriction;
            baseFloatStats[FighterFloatBaseStats.JUMPSQUAT_friction] = cmnStats.jumpSquatFriction;

            baseFloatStats[FighterFloatBaseStats.WALK_baseAccel] = cmnStats.walkBaseAcceleration;
            baseFloatStats[FighterFloatBaseStats.WALK_acceleration] = cmnStats.walkAcceleration;
            baseFloatStats[FighterFloatBaseStats.WALK_minSpeed] = cmnStats.walkMinSpeed;
            baseFloatStats[FighterFloatBaseStats.WALK_maxSpeed] = cmnStats.walkMaxSpeed;
            baseFloatStats[FighterFloatBaseStats.WALK_rotationSpeed] = cmnStats.walkRotationSpeed;
            WalkAccelFromDot = cmnStats.walkAccelerationFromDot;

            baseFloatStats[FighterFloatBaseStats.RUN_baseAccel] = cmnStats.runBaseAcceleration;
            baseFloatStats[FighterFloatBaseStats.RUN_acceleration] = cmnStats.runAcceleration;
            baseFloatStats[FighterFloatBaseStats.RUN_minSpeed] = cmnStats.runMinSpeed;
            baseFloatStats[FighterFloatBaseStats.RUN_maxSpeed] = cmnStats.runMaxSpeed;
            baseFloatStats[FighterFloatBaseStats.RUN_rotationSpeed] = cmnStats.runRotationSpeed;
            RunAccelFromDot = cmnStats.runAccelerationFromDot;

            baseIntStats[FighterIntBaseStats.JUMP_minFrames] = cmnStats.jumpMinimumFrames;
            baseFloatStats[FighterFloatBaseStats.JUMP_maxTime] = ((float)cmnStats.jumpMaxTime) / 60.0f;
            baseFloatStats[FighterFloatBaseStats.JUMP_height] = cmnStats.jumpMaxHeight;
            baseIntStats[FighterIntBaseStats.JUMPSQUAT_frames] = cmnStats.jumpSquatFrames;
            baseFloatStats[FighterFloatBaseStats.JUMP_horizontalMomentum] = cmnStats.jumpHorizontalVelocity;
            baseFloatStats[FighterFloatBaseStats.JUMP_conservedMomentum] = cmnStats.jumpConversedHorizontalMomentum;
            baseFloatStats[FighterFloatBaseStats.FALL_gravityMultiplier] = cmnStats.fallGravityMultiplier;

            baseIntStats[FighterIntBaseStats.AIRJUMP_count] = cmnStats.airJumps;
            baseIntStats[FighterIntBaseStats.AIRJUMP_minFrames] = cmnStats.airJumpMinimumFrames;
            baseFloatStats[FighterFloatBaseStats.AIRJUMP_maxTime] = ((float)cmnStats.airJumpMaxTime) / 60.0f;
            baseFloatStats[FighterFloatBaseStats.AIRJUMP_height] = cmnStats.airJumpMaxHeight;
            baseFloatStats[FighterFloatBaseStats.AIRJUMP_horizontalMomentum] = cmnStats.airJumpHorizontalMomentum;
            baseFloatStats[FighterFloatBaseStats.AIRJUMP_conservedMomentum] =
                cmnStats.airJumpConversedHorizontalMomentum;
            
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

            baseIntStats[FighterIntBaseStats.AIRDASH_count] = cmnStats.airDashes;
            baseIntStats[FighterIntBaseStats.AIRDASH_frames] = cmnStats.airDashFrames;
            baseIntStats[FighterIntBaseStats.AIRDASH_startup] = cmnStats.airDashStartup;
            airDashMaxMagnitude = cmnStats.airDashMaxMagnitude;
            airDashForce = cmnStats.airDashForce;
            baseIntStats[FighterIntBaseStats.AIRDASH_gravityDelay] = cmnStats.airDashGravityDelay;
            baseIntStats[FighterIntBaseStats.AIRDASH_frictionDelay] = cmnStats.airDashFrictionDelay;
            airDashFriction = cmnStats.airDashFriction;

            baseIntStats[FighterIntBaseStats.WALLRUN_HORIZONTAL_maxFrames] = cmnStats.wallRunHorizontalTime;
            wallRunHorizontalSpeed = cmnStats.wallRunHorizontalSpeed;
            wallRunGravity = cmnStats.wallRunGravity;
            wallRunGravityCurve = cmnStats.wallRunGravityCurve;*/
        }
    }
}