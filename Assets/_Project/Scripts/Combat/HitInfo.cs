using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class HitInfo : HnSF.Combat.HitInfo
    {
        public bool attachToEntity = true;
        public string attachTo;
        
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

        public ushort blockHitstopAttacker;
        public ushort blockHitstopDefender;

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

        // ?
        public bool forcesRestand;
        public bool hardKnockdown;
        public bool causesTrip;

        public HitInfo() : base()
        {

        }

        public HitInfo(HnSF.Combat.HitInfoBase other) : base(other)
        {

        }
    }
}