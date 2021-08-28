using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class HitInfo : HnSF.Combat.HitInfo
    {
        public int holdVelocityTime;
        public Vector3 opponentForceAir;
        public float opponentFriction;
        public float opponentGravity;
        public AudioClip hitSound;
        public int hangTime;

        public HitBlockType blockType;
        public ushort blockstun;
        public Vector3 blockForce;
        public Vector3 blockForceAir;

        public HitInfo() : base()
        {

        }

        public HitInfo(HnSF.Combat.HitInfoBase other) : base(other)
        {

        }
    }
}