using System;
using HnSF.Combat;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public class HitInfo : HnSF.Combat.HitInfoBase
    {
        [System.Serializable]
        public struct HitInfoGroup
        {
            
            [FormerlySerializedAs("hitState")] public FighterCmnStates groundHitState;
            public FighterCmnStates airHitState;
            public bool groundBounce;
            public bool wallBounce;
            public float groundBounceForce;
            public float wallBounceForce;
            public bool noKill;
            public bool unblockable;

            public HitboxForceType hitForceType;
            public HitboxForceRelation hitForceRelation;
            public Vector3 hitForceRelationOffset;
            public bool autolink;
            public float gravityAutolinkPercentage;
            public float movementAutolinkPercentage;
            
            // FORCES
            [FormerlySerializedAs("hitForce")] public Vector3 groundHitForce;
            public Vector3 aerialHitForce;
            public float pullPushMultiplier;
            public float pullPushMaxDistance;
            public bool blockLift;
            
            public int attackerHitstop;
            public int hitstop;
            public int hitstun;
            public int blockstun;
            public int untech;

            public float initialProration;
            public float comboProration;

            public int damage;
            public int chipDamage;

            public ModObjectSetContentReference hitEffectbank;
            public string hitEffect;
            public ModObjectSetContentReference blockEffectbank;
            public string blockEffect;
            public ModObjectSetContentReference hitSoundbank;
            public string hitSound;

            public CameraShakeStrength hitCameraShakeStrength;
            public CameraShakeStrength blockCameraShakeStrength;
            public int cameraShakeLength;

            public bool ignoreProration;
            public bool ignoreHitstunScaling;
            public bool ignorePushbackScaling;
        }
        
        [SerializeField] private bool groundedFoldoutGroup;
        [SerializeField] private bool groundedCounterHitFoldoutGroup;

        public StateGroundedGroupType hitStateGroundedGroups;
        [FormerlySerializedAs("groundGroup")] public HitInfoGroup hit;
        public HitInfoGroup counterhit;

        public HitInfo() : base()
        {

        }

        public HitInfo(HnSF.Combat.HitInfoBase other) : base(other)
        {

        }
    }
}