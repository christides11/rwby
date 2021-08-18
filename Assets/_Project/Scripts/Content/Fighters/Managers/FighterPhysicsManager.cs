using Fusion;
using HnSF.Fighters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class FighterPhysicsManager : NetworkBehaviour, IFighterPhysicsManager
    {
        [Networked] public NetworkBool IsGroundedNetworked { get; set; }
        public bool IsGrounded { get { return IsGroundedNetworked; } }

        [Networked] public Vector3 forceMovement { get; set; }
        [Networked] public float forceGravity { get; set; }

        [SerializeField] protected FighterCharacterController cc;

        public void Tick()
        {
            cc.Move(forceMovement, forceGravity);
            //cc.SetMovement(forceMovement, Vector3.zero, new Vector3(0, forceGravity, 0));
        }

        public Vector3 GetOverallForce()
        {
            return new Vector3(0, forceGravity, 0) + forceMovement;
        }

        public void CheckIfGrounded()
        {
            IsGroundedNetworked = cc.ncc.Grounded;
        }

        public void Freeze()
        {
            //cc.SetMovement(Vector3.zero, Vector3.zero);
        }

        public void ResetForces()
        {
            throw new System.NotImplementedException();
        }

        public void SetGrounded(bool value)
        {
            IsGroundedNetworked = value;
        }
    }
}