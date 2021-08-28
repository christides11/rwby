using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "FighterStats", menuName = "rwby/fighter/stats")]
    public class FighterStats : ScriptableObject
    {
        [Header("Ground Movement")]
        public float groundFriction;
        public float jumpSquatFriction;

        public float walkMaxSpeed;
        public float walkBaseAcceleration;
        public float walkAcceleration;
        public float walkRotationSpeed;
        public AnimationCurve walkAccelerationFromDot;

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

        [Header("Aerial Movement")]
        public float aerialMaxFallSpeed;
        public float aerialMaxSpeed;
        public float aerialBaseAcceleration;
        public float aerialAcceleration;
        public float aerialDeceleration;
        public AnimationCurve aerialAccelerationFromDot;

        [Header("Hitstun")]
        public int hitstunHoldPositionFor = 10;
        public float hitstunGravity;
        public float hitstunMaxFallSpeed;
        public float hitstunGroundFriction;

        [Header("Other")]
        public float inertiaFriction;
    }
}