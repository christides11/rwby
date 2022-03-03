using System;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "FighterStats", menuName = "rwby/fighter/stats")]
    public class FighterStats : ScriptableObject
    {
        [Serializable]
        public class UDictionaryFighterStatInt : UDictionary<FighterIntBaseStats, int> { }
        
        [Serializable]
        public class UDictionaryFighterStatFloat : UDictionary<FighterFloatBaseStats, float> { }
        
        [Serializable]
        public class UDictionaryFighterStatAnimationCurve : UDictionary<FighterAnimationCurveBaseStats, AnimationCurve> { }

        [UDictionary.Split(45,55)] public UDictionaryFighterStatInt intStats;
        [UDictionary.Split(45,55)] public UDictionaryFighterStatFloat floatStats;
        [UDictionary.Split(45,55)] public UDictionaryFighterStatAnimationCurve animationCurveStats;

        [Button]
        public void UpdateStats()
        {
            intStats.cache = null;
            floatStats.cache = null;
            animationCurveStats.cache = null;
        }
        /*
        [Header("Ground Movement")]
        public float groundFriction;
        public float jumpSquatFriction;

        public float walkMinSpeed;
        public float walkMaxSpeed;
        public float walkBaseAcceleration;
        public float walkAcceleration;
        public float walkRotationSpeed;
        public AnimationCurve walkAccelerationFromDot;

        public float runMinSpeed;
        public float runMaxSpeed;
        public float runBaseAcceleration;
        public float runAcceleration;
        public float runRotationSpeed;
        public AnimationCurve runAccelerationFromDot;

        [Header("Jump")]
        public int jumpSquatFrames;
        public int jumpMinimumFrames = 5;
        public int jumpMaxTime = 45;
        public float jumpMaxHeight = 4;
        public float jumpConversedHorizontalMomentum;
        public float jumpHorizontalVelocity;
        public float fallGravityMultiplier = 2.0f;

        [Header("Air Jump")]
        public int airJumps;
        public int airJumpMinimumFrames = 5;
        public int airJumpMaxTime = 45;
        public float airJumpMaxHeight = 4;
        public float airJumpConversedHorizontalMomentum;
        public float airJumpHorizontalMomentum;

        [Header("Air Dash")]
        public int airDashes;
        public int airDashFrames;
        public int airDashStartup = 3;
        public float airDashMaxMagnitude;
        public float airDashForce;
        public int airDashGravityDelay;
        public int airDashFrictionDelay;
        public float airDashFriction;

        [Header("Wall Jump")]
        public float MaxWallJumpTime;
        public float MaxWallJumpHeight;
        public float WallJumpConversedMomentum;
        public float WallJumpHorizontalMomentum;

        [Header("Aerial Movement")]
        public float aerialMaxFallSpeed;
        public float aerialMaxSpeed;
        public float aerialBaseAcceleration;
        public float aerialAcceleration;
        public float aerialDeceleration;
        public AnimationCurve aerialAccelerationFromDot;
        public float aerialRotationSpeed;

        [Header("Hitstun")]
        public int hitstunHoldPositionFor = 10;
        public float hitstunGravity;
        public float hitstunMaxFallSpeed;
        public float hitstunGroundFriction;

        [Header("Wall Run")]
        public int wallRunHorizontalTime;
        public float wallRunHorizontalSpeed;
        public float wallRunGravity;
        public AnimationCurve wallRunGravityCurve;

        [Header("Other")]
        public float inertiaFriction;*/
    }
}