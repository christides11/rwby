using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.fighters.states
{
    public class SWallRunH : FighterState
    {
        public override string GetName()
        {
            return "Wall Run (Horizontal)";
        }

        public override void Initialize()
        {
            base.Initialize();
            manager.PhysicsManager.forceMovement = Vector3.zero;
            manager.PhysicsManager.forceGravity = 0;

            Vector3 c = -Vector3.Cross(Vector3.up, manager.lastWallHit.normal);

            manager.SetVisualRotation(-manager.wallSide * c);
            manager.PhysicsManager.SetPosition(manager.lastWallHit.point + ((manager.lastWallHit.normal) * (manager.GetBounds().size.x / 2.0f)));

            if (manager.currentAerialJump == manager.StatManager.airJumps)
            {
                manager.currentAerialJump--;
            }
            if(manager.currentAerialDash == manager.StatManager.airDashes)
            {
                manager.currentAerialDash--;
            }
        }

        public override void OnUpdate()
        {
            UpdatePosition();
            manager.PhysicsManager.forceMovement = manager.transform.forward * manager.StatManager.wallRunHorizontalSpeed;
            manager.PhysicsManager.forceGravity = manager.StatManager.wallRunGravityCurve.Evaluate((float)manager.StateManager.CurrentStateFrame / (float)manager.StatManager.wallRunHorizontalTime) 
                * -manager.StatManager.wallRunGravity;

            if (!CheckInterrupt())
            {
                manager.StateManager.IncrementFrame();
            }
        }

        void UpdatePosition()
        {
            RaycastHit rHit;
            Physics.Raycast(manager.transform.position + new Vector3(0, 1, 0),
                 (manager.transform.right * (manager.wallSide)).normalized,
                out rHit, manager.wallCheckDistance * 2f, manager.wallLayerMask);

            //Debug.DrawRay(manager.transform.position + new Vector3(0, 1, 0),
            //    manager.transform.right * manager.wallSide * manager.wallCheckDistance, Color.red, 5f);
            //Debug.DrawRay(manager.transform.position + new Vector3(0, 1, 0), manager.transform.forward * 4, Color.green, 5f);

            if (rHit.collider == null)
            {
                return;
            }

            float v = Vector3.SignedAngle(manager.transform.forward, -(rHit.normal), Vector3.up);

            Vector3 c = -Vector3.Cross(Vector3.up, rHit.normal);

            //Debug.DrawRay(rHit.point, rHit.normal * (manager.GetBounds().size.x/2.0f), Color.red, 1f);
            //manager.PhysicsManager.SetPosition(rHit.point + ((rHit.normal) * (manager.GetBounds().size.x / 2.0f)));
            //manager.SetVisualRotation((v <= 0 ? 1 : -1) * c);
            manager.SetVisualRotation(-manager.wallSide * c);
            //manager.PhysicsManager.SetPosition(rHit.point + ((rHit.normal) * (manager.GetBounds().size.x / 2.0f)));
            //Debug.DrawRay(rHit.point, rHit.normal * (manager.GetBounds().size.x / 2.0f), Color.red, 10f);
        }

        public override bool CheckInterrupt()
        {
            if(manager.InputManager.GetJump(out int bOff).firstPress == true)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.WALL_JUMP);
                return true;
            }
            if (manager.StateManager.CurrentStateFrame > manager.StatManager.wallRunHorizontalTime
                || manager.DetectWall(out int v, true).collider == null)
            {
                manager.StateManager.ChangeState((int)FighterCmnStates.FALL);
                return true;
            }
            return false;
        }
    }
}
