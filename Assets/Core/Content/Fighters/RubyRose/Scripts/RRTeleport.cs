using rwby.core.content;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.core
{
    public class RRTeleport : FighterState
    {

        public override void Initialize()
        {

        }

        public override void OnUpdate()
        {
            if (manager.PhysicsManager.IsGrounded)
            {
                manager.PhysicsManager.ApplyMovementFriction();
            }
            else
            {
                manager.PhysicsManager.HandleGravity(manager.StatManager.MaxFallSpeed, manager.StatManager.hitstunGravity, 1.0f);
            }

            if(manager.StateManager.CurrentStateFrame == 19)
            {
                RubyRoseManager rrm = (manager as RubyRoseManager);
                bool buttonHeld = true;
                for(int i = 0; i < 19; i++)
                {
                    if(manager.InputManager.GetAbility3(out int buttonOff, i).isDown == false)
                    {
                        buttonHeld = false;
                        break;
                    }
                }

                if (manager.CurrentTarget)
                {
                    PickLockonPosition(buttonHeld);
                }
                else
                {
                    rrm.TeleportPosition = rrm.transform.forward * rrm.teleportLockoffOffset.z
                        + rrm.transform.right * rrm.teleportLockoffOffset.x
                        + rrm.transform.up * rrm.teleportLockoffOffset.y;
                }
                manager.Visible = false;
                manager.TargetableNetworked = false;
            }

            if(manager.StateManager.CurrentStateFrame == 26)
            {
                manager.TargetableNetworked = true;
                RubyRoseManager rrm = (manager as RubyRoseManager);
                manager.PhysicsManager.SetPosition(rrm.TeleportPosition);
                manager.gravity = rrm.teleportGravity;
                manager.apexTime = 1;
            }

            if(manager.StateManager.CurrentStateFrame == 27)
            {
                manager.Visible = true;
                Vector3 modiDir = manager.CurrentTarget.transform.position - manager.transform.position;
                modiDir.y = 0;
                manager.SetVisualRotation(modiDir.normalized);
            }
        }

        private void PickLockonPosition(bool buttonHeld)
        {
            RubyRoseManager rrm = (manager as RubyRoseManager);
            rrm.TeleportPosition = manager.CurrentTarget.transform.position + new Vector3(0, rrm.teleportHeight, 0);
            Vector3 modiDir = manager.CurrentTarget.transform.position - manager.transform.position;
            modiDir.y = 0;
            if (buttonHeld)
            {
                rrm.TeleportPosition += modiDir.normalized * rrm.teleportDist;
            }
            else
            {
                rrm.TeleportPosition -= modiDir.normalized * rrm.teleportDist;
            }
        }

        public override void OnLateUpdate()
        {
            manager.StateManager.IncrementFrame();
        }

        public override void OnInterrupted()
        {
            base.OnInterrupted();
            manager.Visible = true;
            manager.TargetableNetworked = true;
        }

        public override bool CheckInterrupt()
        {
            return base.CheckInterrupt();
        }
    }
}