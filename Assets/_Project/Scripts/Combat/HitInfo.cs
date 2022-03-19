using HnSF.Combat;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class HitInfo : HnSF.Combat.HitInfoBase
    {
        public int holdVelocityTime;
        public float opponentFriction;
        public float opponentGravity;
        public int hangTime;

        public string hitSoundbankName;
        public string hitSoundName;

        // Shake
        public float shakeValue;

        // Effect
        public string effectbankName;
        public string effectName;

        //
        public string blockSoundbankName;
        public string blockSoundName;

        // GENERAL
        public FighterCmnStates groundHitState;
        public FighterCmnStates groundCounterHitState;
        public FighterCmnStates aerialHitState;
        public FighterCmnStates aerialCounterHitState;
        public int groundBounces;
        public int wallBounces;
        public float groundBounceForcePercentage = 1.0f;
        public float wallBounceForcePercentage = 1.0f;
        public bool hitKills;
        public StateGroupType hitStateGroups;
        
        // FORCES
        public HitboxForceType forceType = HitboxForceType.SET;
        public HitboxForceRelation forceRelation = HitboxForceRelation.ATTACKER;
        public Vector3 groundHitForce;
        public Vector3 groundCounterHitForce;
        public Vector3 groundBlockForce;
        public Vector3 aerialHitForce;
        public Vector3 aerialCounterHitForce;
        public Vector3 aerialBlockForce;
        public bool autolink;
        public float autolinkPercentage = 1.0f;
        
        // STUN
        public int attackerHitstop;
        public int hitstop;
        public int counterHitAddedHitstop;
        public int groundHitstun;
        public int aerialHitstun;
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