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

        [SerializeField] protected FighterManager manager;
        [SerializeField] protected FighterCC kCC;

        protected Vector3 rotationDir;

        public void Tick()
        {
            kCC.SetMovement(forceMovement, forceGravity);
            if(rotationDir == Vector3.zero)
            {
                kCC.SetRotation(transform.forward);
            }
            else
            {
                kCC.SetRotation(rotationDir);
            }
            rotationDir = Vector3.zero;
        }

        public void SetPosition(Vector3 position, bool bypassInterpolation = true)
        {
            kCC.Motor.SetPosition(position, bypassInterpolation);
        }

        public void SetRotation(Vector3 rot)
        {
            rotationDir = rot;
            kCC.Motor.SetRotation(Quaternion.LookRotation(rotationDir), true);
        }

        public Vector3 GetOverallForce()
        {
            return new Vector3(0, forceGravity, 0) + forceMovement;
        }

        public void CheckIfGrounded()
        {
            IsGroundedNetworked = kCC.Motor.GroundingStatus.IsStableOnGround;
        }

        public void Freeze()
        {
            kCC.SetMovement(Vector3.zero, 0);
        }

        public void ResetForces()
        {
            throw new System.NotImplementedException();
        }

        public void SetGrounded(bool value)
        {
            IsGroundedNetworked = value;
        }

        public virtual void ApplyMovementFriction(float friction = -1)
        {
            if (friction == -1)
            {
                friction = manager.StatManager.GroundFriction;
            }
            Vector3 realFriction = forceMovement.normalized * friction;
            Vector3 temp = forceMovement;
            temp.x = ApplyFriction(forceMovement.x, Mathf.Abs(realFriction.x) * Runner.DeltaTime);
            temp.z = ApplyFriction(forceMovement.z, Mathf.Abs(realFriction.z) * Runner.DeltaTime);
            forceMovement = temp;
        }

        public virtual void ApplyGravityFriction(float friction)
        {
            forceGravity = ApplyFriction(forceGravity, friction * Runner.DeltaTime);
        }

        /// <summary>
        /// Applies friction on the given value based on the traction given.
        /// </summary>
        /// <param name="value">The value to apply traction to.</param>
        /// <param name="traction">The traction to apply.</param>
        /// <returns>The new value with the traction applied.</returns>
        protected virtual float ApplyFriction(float value, float traction)
        {
            if (value > 0)
            {
                value -= traction;
                if (value < 0)
                {
                    value = 0;
                }
            }
            else if (value < 0)
            {
                value += traction;
                if (value > 0)
                {
                    value = 0;
                }
            }
            return value;
        }

        public virtual void HandleMovement(float baseAccel, float movementAccel, float deceleration, float minSpeed, float maxSpeed, AnimationCurve accelFromDot)
        {
            // Get wanted movement vector.
            Vector3 movement = manager.GetMovementVector();

            // Real Accel
            float realAcceleration = baseAccel + (movement.magnitude * movementAccel);

            if (movement.magnitude > 1.0f)
            {
                movement.Normalize();
            }

            // Calculated our wanted movement force.
            float accel = movement == Vector3.zero ? deceleration : realAcceleration * accelFromDot.Evaluate(Vector3.Dot(movement, forceMovement.normalized));
            Vector3 goalVelocity = movement.normalized * (minSpeed + (movement.magnitude * (maxSpeed - minSpeed)));

            // Move towards that goal based on our acceleration.
            forceMovement = Vector3.MoveTowards(forceMovement, goalVelocity, accel * Runner.DeltaTime);
        }

        public virtual void HandleGravity(float maxFallSpeed, float gravity, float gravityScale)
        {
            forceGravity = Mathf.MoveTowards(forceGravity, -maxFallSpeed, gravity * Runner.DeltaTime);
        }

        List<LagCompensatedHit> colllch = new List<LagCompensatedHit>();
        public LayerMask collisionBoxLayerMask;
        public virtual void ApplyCollisionPush()
        {
            /*
            FighterCollboxManager cbm = manager.collboxManager;
            int collboxCount = cbm.CollboxCount;

            for(int i = 0; i < collboxCount; i++)
            {
                switch (cbm.collisionboxes[i].Type)
                {
                    case HitboxTypes.Box:
                        break;
                    case HitboxTypes.Sphere:
                        Runner.LagCompensation.OverlapSphere(cbm.collisionboxes[i].Position, cbm.collisionboxes[i].SphereRadius, Runner.Simulation.Tick, colllch, collisionBoxLayerMask, HitOptions.DetailedHit,
                            true, QueryTriggerInteraction.UseGlobal);
                        break;
                }

                for(int j = 0; j < colllch.Count; j++)
                {
                    
                }
                //Runner.LagCompensation.PositionRotation(cbm.collisionboxes[i], Runner.Simulation.Tick, out Vector3 pos, out Quaternion rot);
            }*/
        }
    }
}