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
    [OrderBefore(typeof(Fusion.HitboxManager), typeof(FighterPhysicsManager), typeof(FighterStateManager), typeof(FighterHurtboxManager), typeof(FighterHitboxManager), typeof(FighterCombatManager))]
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

        // Stats
        [Networked] public NetworkBool StoredRun { get; set; }
        [Networked] public float currentAerialJump { get; set; }
        [Networked] public float currentAerialDash { get; set; }
        [Networked] public float apexTime { get; set; }
        [Networked] public float gravity { get; set; }
        [Networked, Capacity(10)] public NetworkArray<NetworkBool> attackEventInput { get; set; }

        [Header("References")]
        [NonSerialized] public NetworkManager networkManager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterCombatManager combatManager;
        [SerializeField] protected FighterStateManager stateManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterHurtboxManager hurtboxManager;
        [SerializeField] protected FighterStatManager statManager;
        [SerializeField] protected IFighterDefinition fighterDefinition;
        public Transform visualTransform;

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
            if (Object.HasInputAuthority)
            {
                ClientManager.local.camera = GameObject.Instantiate(GameManager.singleton.settings.playerCameraPrefab, transform.position, transform.rotation);
                ClientManager.local.camera.SetLookAtTarget(visualTransform);
            }
            combatManager.Cleanup();
            combatManager.SetMoveset(0);
            statManager.SetupStats();
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
            /*if (CombatManager.HitStop > 0
                && CombatManager.HitStun > 0)
            {
                //Vector3 pos = visual.transform.localPosition;
                //pos.x = (Mathf.Sign(pos.x) > 0 ? -1 : 0) * .02f;
                //visual.transform.localPosition = pos;
            }*/

            if (CombatManager.HitStop == 0)
            {
                if (CombatManager.BlockStun > 0)
                {
                    CombatManager.BlockStun--;
                }
                //HandleLockon();
                PhysicsManager.CheckIfGrounded();
                StateManager.Tick();
                PhysicsManager.Tick();
            }
            else
            {
                CombatManager.HitStop--;
                PhysicsManager.Freeze();
            }
        }

        public void LateTick()
        {

        }
        protected virtual void SetupStates()
        {
            stateManager.AddState(new SIdle(), (ushort)FighterCmnStates.IDLE);
            stateManager.AddState(new SWalk(), (ushort)FighterCmnStates.WALK);
            stateManager.AddState(new SRun(), (ushort)FighterCmnStates.RUN);
            stateManager.AddState(new SRunBrake(), (ushort)FighterCmnStates.RUN_BRAKE);
            stateManager.AddState(new SJump(), (ushort)FighterCmnStates.JUMP);
            stateManager.AddState(new SJumpAir(), (ushort)FighterCmnStates.AIR_JUMP);
            stateManager.AddState(new SFall(), (ushort)FighterCmnStates.FALL);
            StateManager.AddState(new SJumpSquat(), (ushort)FighterCmnStates.JUMPSQUAT);
            StateManager.AddState(new SAirDash(), (ushort)FighterCmnStates.AIR_DASH);
            StateManager.AddState(new SAttack(), (ushort)FighterCmnStates.ATTACK);

            StateManager.AddState(new SFlinchGround(), (ushort)FighterCmnStates.FLINCH_GROUND);
            StateManager.AddState(new SFlinchAir(), (ushort)FighterCmnStates.FLINCH_AIR);

            StateManager.AddState(new SBlockHigh(), (ushort)FighterCmnStates.BLOCK_HIGH);
            StateManager.AddState(new SBlockLow(), (ushort)FighterCmnStates.BLOCK_LOW);
            StateManager.AddState(new SBlockAir(), (ushort)FighterCmnStates.BLOCK_AIR);

            StateManager.ChangeState((ushort)FighterCmnStates.FALL);
        }

        public virtual bool TryAttack()
        {
            int man = CombatManager.TryAttack();
            if (man != -1)
            {
                CombatManager.SetAttack(man);
                StateManager.ChangeState((int)FighterCmnStates.ATTACK);
                return true;
            }
            return false;
        }

        public virtual bool TryAttack(int attackIdentifier, int attackMoveset = -1, bool resetFrameCounter = true)
        {
            if (attackIdentifier != -1)
            {
                if (attackMoveset != -1)
                {
                    CombatManager.SetAttack(attackIdentifier, attackMoveset);
                }
                else
                {
                    CombatManager.SetAttack(attackIdentifier);
                }
                StateManager.ChangeState((int)FighterCmnStates.ATTACK, resetFrameCounter ? 0 : StateManager.CurrentStateFrame);
                return true;
            }
            return false;
        }

        public bool TryJump()
        {
            if (InputManager.GetJump(out int bOff).firstPress == false)
            {
                return false;
            }
            if(physicsManager.IsGroundedNetworked == false)
            {
                return false;
            }
            StateManager.ChangeState((ushort)FighterCmnStates.JUMPSQUAT);
            return true;
        }

        public bool TryAirJump()
        {
            if(InputManager.GetJump(out int bOff).firstPress == false)
            {
                return false;
            }
            if(currentAerialJump == statManager.airJumps)
            {
                return false;
            }
            currentAerialJump++;
            StateManager.ChangeState((ushort)FighterCmnStates.AIR_JUMP);
            return true;
        }

        public bool TryAirDash()
        {
            if(InputManager.GetDash(out int bOff).firstPress == false)
            {
                return false;
            }
            if(currentAerialDash == statManager.airDashes)
            {
                return false;
            }
            currentAerialDash++;
            StateManager.ChangeState((ushort)FighterCmnStates.AIR_DASH);
            return true;
        }

        public bool TryBlock()
        {
            if(InputManager.GetBlock(out int bOff).isDown == false)
            {
                return false;
            }
            StateManager.ChangeState((ushort)FighterCmnStates.BLOCK_HIGH);
            return true;
        }

        public bool TryLandCancel()
        {
            PhysicsManager.CheckIfGrounded();
            if (PhysicsManager.IsGroundedNetworked)
            {
                StateManager.ChangeState((ushort)FighterCmnStates.IDLE);
                return true;
            }
            return false;
        }

        public virtual void ResetVariablesOnGround()
        {
            StoredRun = false;
            currentAerialDash = 0;
            currentAerialJump = 0;
            apexTime = 0;
            gravity = 0;
            PhysicsManager.forceGravity = 0;
        }


        /// <summary>
        /// Translates the movement vector based on the look transform's forward.
        /// </summary>
        /// <param name="frame">The frame we want to check the movement input for.</param>
        /// <returns>A direction vector based on the camera's forward.</returns>
        public virtual Vector3 GetMovementVector(int frame = 0)
        {
            Vector2 movement = InputManager.GetMovement(frame);
            if(movement.magnitude < InputConstants.movementDeadzone)
            {
                return Vector3.zero;
            }
            return GetMovementVector(movement.x, movement.y);
        }

        public virtual Vector3 GetMovementVector(float horizontal, float vertical)
        {
            Vector3 forward = inputManager.GetCameraForward();
            Vector3 right = inputManager.GetCameraRight();

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return forward * vertical + right * horizontal;
        }

        public Vector3 GetVisualBasedDirection(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public virtual void RotateVisual(Vector3 direction, float speed)
        {
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, speed * Runner.DeltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        public void SetTargetable(bool value)
        {
            TargetableNetworked = value;
        }

        public void SetVisualRotation(Vector3 direction)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}