using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SAirDash : FighterState
    {
        public override string GetName()
        {
            return "Air Dash";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.PhysicsManager.forceGravity = 0;
            manager.apexTime = manager.StatManager.MaxJumpTime / 2.0f;
            manager.gravity = (-2.0f * manager.StatManager.MaxJumpHeight) / Mathf.Pow(manager.apexTime, 2.0f);
        }

        public override void OnUpdate()
        {
            if(manager.StateManager.CurrentStateFrame == manager.StatManager.airDashStartup)
            {
                Vector3 movement = manager.GetMovementVector();
                if(movement == Vector3.zero)
                {
                    if (manager.HardTargeting)
                    {
                        movement = manager.CurrentTarget.transform.position - manager.transform.position;
                        movement.y = 0;
                        movement.Normalize();
                    }
                    else
                    {
                        movement = manager.GetMovementVector(0, 1);
                    }
                }

                manager.PhysicsManager.forceMovement = movement * manager.StatManager.airDashForce;

                /*
                if(manager.PhysicsManager.forceMovement.magnitude > manager.StatManager.airDashMaxMagnitude)
                {
                    manager.PhysicsManager.forceMovement = manager.PhysicsManager.forceMovement.normalized * manager.StatManager.airDashMaxMagnitude;
                }*/
            }

            if(manager.StateManager.CurrentStateFrame > manager.StatManager.airDashGravityDelay)
            {
                manager.PhysicsManager.forceGravity += manager.gravity * manager.StatManager.fallGravityMultiplier * manager.Runner.DeltaTime;
                manager.PhysicsManager.forceGravity = Mathf.Clamp(manager.PhysicsManager.forceGravity, -manager.StatManager.MaxFallSpeed, float.MaxValue);
            }

            if(manager.StateManager.CurrentStateFrame > manager.StatManager.airDashFrictionDelay)
            {
                manager.PhysicsManager.ApplyMovementFriction(manager.StatManager.airDashFriction);
            }

            if (CheckInterrupt()) return;
            manager.StateManager.IncrementFrame();
        }

        public override bool CheckInterrupt()
        {
            if (manager.TryAttack() || manager.TryAirJump() || manager.TryWallRun())
            {
                return true;
            }
            if (manager.StateManager.CurrentStateFrame > manager.StatManager.airDashFrames)
            {
                manager.StateManager.ChangeState((ushort)FighterCmnStates.FALL);
                return true;
            }
            return false;
        }
    }
}