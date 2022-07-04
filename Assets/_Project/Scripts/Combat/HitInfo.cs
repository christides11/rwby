using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class HitInfo : HnSF.Combat.HitInfoBase
    {
        [System.Serializable]
        public struct HitInfoGroup
        {
            public FighterCmnStates hitState;
            public int groundBounces;
            public int wallBounces;
            public float groundBounceForcePercentage;
            public float wallBounceForcePercentage;
            public bool hitKills;

            public HitboxForceType hitForceType;
            public HitboxForceRelation hitForceRelation;
            public bool autolink;
            public float autolinkPercentage;
            
            // FORCES
            public Vector3 hitForce;
            public Vector3 blockForce;
            public float hitGravity;
            public AnimationCurve pullPushCurve;
            public float pullPushMaxDistance;
            
            public int attackerHitstop;
            public int hitstop;
            public int hitstun;
            public int blockstun;
        }

        public StateGroundedGroupType hitStateGroundedGroups;
        public HitInfoGroup groundGroup;
        public HitInfoGroup groundCounterHitGroup;
        public HitInfoGroup aerialGroup;
        public HitInfoGroup aerialCounterHitGroup;

        public HitInfo() : base()
        {

        }

        public HitInfo(HnSF.Combat.HitInfoBase other) : base(other)
        {

        }
    }
}