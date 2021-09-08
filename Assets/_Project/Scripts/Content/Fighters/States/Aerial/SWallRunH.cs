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

            float v = Vector3.SignedAngle(manager.transform.forward,
                -manager.lastWallHit.normal,
                Vector3.up);

            Vector3 c = -Vector3.Cross(Vector3.up, manager.lastWallHit.normal);
            manager.SetVisualRotation((v <= 0 ? 1 : -1) * c);
            //controller.visualTransform.rotation = Quaternion.LookRotation((v <= 0 ? 1 : -1) * c, Vector3.up);

            manager.PhysicsManager.SetPosition(manager.lastWallHit.point + ((manager.lastWallHit.normal) * (manager.GetBounds().size.x / 2.0f)));
        }

        public override void OnUpdate()
        {
            UpdatePosition();

            manager.PhysicsManager.forceMovement =
                ((manager.transform.forward
                * manager.StatManager.wallRunHorizontalSpeed
                * manager.wallRunHozMultiplier))
                +
                (manager.transform.right * manager.wallSide
                * manager.StatManager.wallRunHorizontalSpeed
                * manager.wallRunHozMultiplier);

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
                out rHit, manager.wallCheckDistance * 1.25f, manager.wallLayerMask);

            //Debug.DrawRay(manager.transform.position + new Vector3(0, 1, 0),
            //    manager.transform.right * manager.wallSide * manager.wallCheckDistance);

            if (rHit.collider == null)
            {
                return;
            }

            float v = Vector3.SignedAngle(manager.transform.forward, -(rHit.normal), Vector3.up);

            Vector3 c = -Vector3.Cross(Vector3.up, rHit.normal);

            manager.SetVisualRotation((v <= 0 ? 1 : -1) * c);
            manager.PhysicsManager.SetPosition(rHit.point + ((rHit.normal.normalized) * (manager.GetBounds().size.x / 2.0f)));
        }

        public override bool CheckInterrupt()
        {
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
