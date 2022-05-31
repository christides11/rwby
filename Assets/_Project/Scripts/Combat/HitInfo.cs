using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public class HitInfo : HnSF.Combat.HitInfoBase
    {
        // GENERAL
        public FighterCmnStates groundHitState;
        public FighterCmnStates groundCounterHitState;
        public FighterCmnStates aerialHitState;
        public FighterCmnStates aerialCounterHitState;
        public int groundBounces;
        public int wallBounces;
        public int counterHitGroundBounces;
        public int counterHitWallBounces;
        public float groundBounceForcePercentage = 1.0f;
        public float wallBounceForcePercentage = 1.0f;
        public bool hitKills;
        [FormerlySerializedAs("hitStateGroups")] [EnumFlags] public StateGroundedGroupType hitStateGroundedGroups = StateGroundedGroupType.AERIAL | StateGroundedGroupType.GROUND;
        
        // FORCES
        public HitboxForceType hitForceType = HitboxForceType.SET;
        public HitboxForceRelation hitForceRelation = HitboxForceRelation.ATTACKER;
        public bool autolink;
        public float autolinkPercentage = 1.0f;
        // Ground
        public Vector3 groundHitForce;
        public Vector3 groundCounterHitForce;
        public Vector3 groundBlockForce;
        public float groundHitGravity = -1;
        public float groundCounterHitGravity = -1;
        // Aerial
        public Vector3 aerialHitForce;
        public Vector3 aerialCounterHitForce;
        public Vector3 aerialBlockForce;
        public float aerialHitGravity = -1;
        public float aerialCounterHitGravity = -1;

        // STUN
        public int attackerHitstop;
        public int hitstop;
        public int counterHitAddedHitstop;
        public int groundHitstun;
        public int groundCounterHitstun;
        public int aerialHitstun;
        public int aerialCounterHitstun;
        public int groundBlockstun;
        public int aerialBlockstun;
        
        public HitInfo() : base()
        {

        }

        public HitInfo(HnSF.Combat.HitInfoBase other) : base(other)
        {

        }
    }
}