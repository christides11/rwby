using Fusion;
using Fusion.KCC;
using HnSF.Fighters;
using UnityEngine;

namespace rwby
{
    public class FighterPhysicsManager : NetworkBehaviour, IFighterPhysicsManager
    {
        [Networked] public NetworkBool IsGroundedNetworked { get; set; }
        public bool IsGrounded { get { return IsGroundedNetworked; } }

        [Networked] public Vector3 forceMovement { get; set; }
        [Networked] public float forceGravity { get; set; }

        [Networked] public Vector3 conservedInertia { get; set; }

        [SerializeField] protected FighterManager manager;
        //public FighterCC kCC;
        public KCC kcc;
        public CapsuleCollider cc => kcc.Collider;

        [SerializeField] protected Vector3 rotationDir;

        public void ResimulationResync()
        {
            if (!Mathf.Approximately(ecbOffset, localECBOffset) 
                || !Mathf.Approximately(ecbRadius, localECBRadius) 
                || !Mathf.Approximately(ecbHeight, localECBHeight))
            {
                
                //kCC.Motor.SetCapsuleDimensions(ecbRadius, ecbHeight, ecbOffset);
                localECBOffset = this.ecbOffset;
                localECBRadius = this.ecbRadius;
                localECBHeight = this.ecbHeight;
            }
        }
        
        public void Tick()
        {
            kcc.SetInputDirection(forceMovement.magnitude.IsAlmostZero() ? Vector3.zero : forceMovement.normalized);
            kcc.SetKinematicVelocity(forceMovement);
            kcc.SetDynamicVelocity(new Vector3(0, forceGravity, 0));
            //kCC.SetMovement(forceMovement, forceGravity);
            if(rotationDir == Vector3.zero)
            {
                kcc.SetLookRotation(Quaternion.LookRotation(transform.forward));
            //    kCC.SetRotation(transform.forward);
            }
            else
            {
                kcc.SetLookRotation(Quaternion.LookRotation(rotationDir));
                //    kCC.SetRotation(rotationDir);
            }
            rotationDir = Vector3.zero;
        }

        public void Move(Vector3 position)
        {
            kcc.SetPosition(position);//.TeleportRPC(position, 0, 0);
            //kCC.Motor.MoveCharacter(position);
        }

        [Networked] public float ecbOffset { get; set; } = 1;
        [Networked] public float ecbRadius { get; set; } = 1;
        [Networked] public float ecbHeight { get; set; } = 2;

        public float localECBOffset = 1;
        public float localECBRadius = 1;
        public float localECBHeight = 2;

        public void SetECB(float ecbCenter, float ecbRadius, float ecbHeight)
        {
            this.ecbOffset = ecbCenter;
            this.ecbRadius = ecbRadius;
            this.ecbHeight = ecbHeight;
            //kCC.Motor.SetCapsuleDimensions(ecbRadius, ecbHeight, ecbCenter);
            kcc.SetShape(EKCCShape.Capsule, ecbRadius, ecbHeight, ecbCenter);
            localECBOffset = this.ecbOffset;
            localECBRadius = this.ecbRadius;
            localECBHeight = this.ecbHeight;
        }

        public void SnapECB()
        {
            Vector3 newECBPosition = transform.position + new Vector3(0, ecbOffset - (ecbHeight/2.0f), 0);
            kcc.SetPosition(newECBPosition);//.SetShape(EKCCShape.Capsule, 0, 0, newECBPosition);
            //kCC.Motor.SetPosition(newECBPosition);
        }

        public Vector3 ECBCenter()
        {
            return transform.position + new Vector3(0, ecbOffset, 0);
        }

        public void SetPosition(Vector3 position, bool bypassInterpolation = true)
        {
            kcc.SetPosition(position);
            //kCC.Motor.SetPosition(position, bypassInterpolation);
        }

        public void ForceUnground()
        {
            kcc.Data.HasJumped = true;
            //kCC.Motor.ForceUnground();
        }

        public void SetRotation(Vector3 rot, bool bypassInterpolation = true)
        {
            rotationDir = rot;
            kcc.SetLookRotation(Quaternion.LookRotation(rotationDir));
            //kCC.Motor.SetRotation(Quaternion.LookRotation(rotationDir), bypassInterpolation);
        }

        public void ForceSetRotation(Quaternion rot, bool bypassinterpolation = true)
        {
            kcc.SetLookRotation(rot);
            //kCC.Motor.SetRotation(Quaternion.LookRotation(rotationDir), bypassInterpolation);
        }

        public Vector3 GetOverallForce()
        {
            return new Vector3(0, forceGravity, 0) + forceMovement;
        }

        public void CheckIfGrounded()
        {
            IsGroundedNetworked = kcc.Data.IsGrounded;
            //IsGroundedNetworked = kCC.Motor.GroundingStatus.IsStableOnGround;
        }

        public void Freeze()
        {
            kcc.SetInputDirection(Vector3.zero);
            kcc.SetKinematicVelocity(Vector3.zero);
            kcc.SetDynamicVelocity(Vector3.zero);
        }

        public void ResetForces()
        {
            forceMovement = Vector3.zero;
            forceGravity = 0;
        }

        public void SetGrounded(bool value)
        {
            IsGroundedNetworked = value;
        }

        public virtual void ApplyMovementFriction(float friction)
        {
            forceMovement = Vector3.MoveTowards(forceMovement, Vector3.zero, friction * Runner.DeltaTime);
        }
        
        public virtual void ApplyGravityFriction(float friction)
        {
            forceGravity = Mathf.MoveTowards(forceGravity, 0, friction * Runner.DeltaTime);
        }

        public virtual Vector3 HandleMovement(float baseAccel, float movementAccel, float deceleration, float minSpeed, float maxSpeed, AnimationCurve accelFromDot)
        {
            return HandleMovement(manager.GetMovementVector(), baseAccel, movementAccel, deceleration, minSpeed, maxSpeed, accelFromDot);
        }
        
        public virtual Vector3 HandleMovement(Vector3 movement, float baseAccel, float movementAccel, float deceleration, float minSpeed, float maxSpeed, AnimationCurve accelFromDot)
        {
            float realAcceleration = baseAccel + (movement.magnitude * movementAccel);

            if (movement.magnitude > 1.0f)
            {
                movement.Normalize();
            }

            // Calculated our wanted movement force.
            float accel = movement == Vector3.zero ? deceleration : realAcceleration * accelFromDot.Evaluate(Vector3.Dot(movement, forceMovement.normalized));
            Vector3 goalVelocity = movement.normalized * (minSpeed + (movement.magnitude * (maxSpeed - minSpeed)));
            
            return Vector3.MoveTowards(forceMovement, goalVelocity, accel * Runner.DeltaTime) - forceMovement;
        }

        public virtual void HandleGravity(float maxFallSpeed, float gravity)
        {
            forceGravity = Mathf.MoveTowards(forceGravity, -maxFallSpeed, gravity * Runner.DeltaTime);
        }
    }
}