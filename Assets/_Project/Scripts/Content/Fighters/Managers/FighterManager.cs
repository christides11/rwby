using Fusion;
using HnSF.Combat;
using HnSF.Fighters;
using rwby.fighters.states;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterPhysicsManager), typeof(FighterStateManager), typeof(FighterHurtboxManager), typeof(FighterHitboxManager), typeof(FighterCombatManager))]
    public class FighterManager : NetworkBehaviour, IFighterBase, ITargetable
    {
        public FighterInputManager InputManager { get { return inputManager; } }
        public FighterCombatManager CombatManager { get { return combatManager; } }
        public FighterStateManager StateManager { get { return stateManager; } }
        public FighterStatManager StatManager { get { return statManager; } }
        public FighterPhysicsManager PhysicsManager { get { return physicsManager; } }
        public FighterHurtboxManager HurtboxManager { get { return hurtboxManager; } }

        [Networked] public NetworkBool TargetableNetworked { get; set; }
        public bool Targetable { get { return TargetableNetworked; } }

        [Header("References")]
        [NonSerialized] public NetworkManager networkManager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterCombatManager combatManager;
        [SerializeField] protected FighterStateManager stateManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterHurtboxManager hurtboxManager;
        [SerializeField] protected FighterStatManager statManager;
        [SerializeField] protected IFighterDefinition fighterDefinition;

        [Header("Vars")]
        public float apexTime;
        public float gravity;
        public float initialJumpVelocity;
        public float fallMulti = 2.0f;

        public void OnFighterLoaded()
        {

        }

        public void Awake()
        {
            networkManager = NetworkManager.singleton;
            combatManager.movesets = fighterDefinition.GetMovesets();
        }

        public override void Spawned()
        {
            base.Spawned();
            statManager.SetupStats();
            combatManager.SetMoveset(0);
            SetupStates();
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            Tick();
        }

        public void Tick()
        {
            // Shake during hitstop (only when you got hit by an attack).
            if (CombatManager.HitStop > 0
                && CombatManager.HitStun > 0)
            {
                //Vector3 pos = visual.transform.localPosition;
                //pos.x = (Mathf.Sign(pos.x) > 0 ? -1 : 0) * .02f;
                //visual.transform.localPosition = pos;
            }

            if (CombatManager.HitStop == 0)
            {
                //HandleLockon();
                PhysicsManager.CheckIfGrounded();
                StateManager.Tick();
                PhysicsManager.Tick();
            }
            else
            {
                PhysicsManager.Freeze();
            }
            //HurtboxManager.Tick();
        }

        public void LateTick()
        {
            throw new System.NotImplementedException();
        }
        protected virtual void SetupStates()
        {
            stateManager.AddState(new SIdle(), (ushort)FighterCmnStates.IDLE);
            stateManager.AddState(new SWalk(), (ushort)FighterCmnStates.WALK);
            stateManager.AddState(new SRun(), (ushort)FighterCmnStates.RUN);
            stateManager.AddState(new SJump(), (ushort)FighterCmnStates.JUMP);
            stateManager.AddState(new SFall(), (ushort)FighterCmnStates.FALL);
            StateManager.AddState(new SJumpSquat(), (ushort)FighterCmnStates.JUMPSQUAT);

            StateManager.ChangeState((ushort)FighterCmnStates.FALL);
        }


        public Vector3 GetMovementVector(float horizontal, float vertical)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetVisualBasedDirection(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public void RotateVisual(Vector3 direction, float speed)
        {
            throw new System.NotImplementedException();
        }

        public void SetTargetable(bool value)
        {
            TargetableNetworked = value;
        }

        public void SetVisualRotation(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}