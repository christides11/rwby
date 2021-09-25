using Cysharp.Threading.Tasks;
using Fusion;    
using HnSF.Combat;
using HnSF.Fighters;
using KinematicCharacterController;
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
        public SoundbankContainer SoundbankContainer { get { return soundbankContainer; } }
        public EffectbankContainer EffectbankContainer { get { return effectbankContainer; } }
        public Transform TargetOrigin { get { return targetOrigin; } }

        [Networked] public NetworkBool TargetableNetworked { get; set; }
        public bool Targetable { get { return TargetableNetworked; } }

        [Networked] public NetworkBool HardTargeting { get; set; }
        [Networked] public NetworkObject CurrentTarget { get; set; }
        [Networked] public NetworkBool Visible { get; set; }

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
        [SerializeField] protected CapsuleCollider capsuleCollider;
        [SerializeField] protected SoundbankContainer soundbankContainer;
        [SerializeField] protected EffectbankContainer effectbankContainer;
        [SerializeField] protected Transform targetOrigin;
        public ParticleSystemEffect guardEffect;
        public Transform visualTransform;

        [Header("Lock On")]
        public LayerMask lockonLayerMask;
        public LayerMask lockonVisibilityLayerMask;
        public float lockonMaxDistance = 20;
        public float lockonFudging = 0.1f;

        public virtual async UniTask<bool> OnFighterLoaded()
        {
            return true;
        }

        public virtual void Awake()
        {
            networkManager = NetworkManager.singleton;
            combatManager.movesets = fighterDefinition.GetMovesets();
        }

        public override void Spawned()
        {
            base.Spawned();
            TargetableNetworked = true;
            Visible = true;
            if (Object.HasInputAuthority)
            {
                ClientManager.local.camera = GameObject.Instantiate(GameManager.singleton.settings.playerCameraPrefab, transform.position, transform.rotation);
                ClientManager.local.camera.SetLookAtTarget(this);
            }
            combatManager.Cleanup();
            combatManager.SetMoveset(0);
            statManager.SetupStats((combatManager.movesets[0] as Moveset).fighterStats);
            SetupStates();
        }

        public float hitstopShakeDistance = 0.5f;
        public int hitstopDir = 1;
        public int hitstopShakeFrames = 1;

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            visualTransform.gameObject.SetActive(Visible);

            HitstopShake();

            HandleLockon();

            if (CombatManager.HitStop == 0)
            {
                if (CombatManager.BlockStun > 0)
                {
                    CombatManager.BlockStun--;
                }
                PhysicsManager.CheckIfGrounded();
                StateManager.Tick();
                PhysicsManager.Tick();
            }
            else
            {
                CombatManager.hitstopCounter++;
                PhysicsManager.Freeze();
                if(CombatManager.hitstopCounter == CombatManager.HitStop)
                {
                    CombatManager.HitStop = 0;
                    currentShakeDirection = 0;
                }
            }
        }

        public Vector3[] shakeDirs;
        [Networked] public sbyte currentShakeDirection { get; set; }

        protected void HitstopShake()
        {
            // Shake during hitstop.
            if (CombatManager.HitStop != 0
                && (CombatManager.HitStun > 0 || CombatManager.BlockStun > 0)
                && CombatManager.HitStop % hitstopShakeFrames == 0)
            {
                Vector3 dir = shakeDirs[currentShakeDirection].z * transform.forward
                    + shakeDirs[currentShakeDirection].x * transform.right;
                physicsManager.SetPosition(transform.position + (dir * hitstopShakeDistance * hitstopDir), true);
                hitstopDir *= -1;
                
                if(hitstopDir == 1)
                {
                    currentShakeDirection++;
                    if(currentShakeDirection >= shakeDirs.Length)
                    {
                        currentShakeDirection = 0;
                    }
                }
            }
        }

        private void HandleLockon()
        {
            if(HardTargeting == false)
            {
                TryLockon();
            }
            else
            {
                TryLockoff();
            }
        }

        private void TryLockon()
        {
            if (inputManager.GetLockOn(out int bOffset).firstPress == false) return;
            PickLockonTarget();
            HardTargeting = true;
        }

        private void TryLockoff()
        {
            float dist = 0;
            if(CurrentTarget != null)
            {
                dist = Vector3.Distance(transform.position, CurrentTarget.transform.position);
            }
            if ((dist <= lockonMaxDistance + 0.5f) 
                && (inputManager.GetLockOn(out int bOffset).firstPress == false && (CurrentTarget != null && CurrentTarget.GetComponent<ITargetable>().Targetable == true)) ) return;
            CurrentTarget = null;
            HardTargeting = false;
        }

        private void PickLockonTarget()
        {
            Collider[] list = Physics.OverlapSphere(GetCenter(), lockonMaxDistance, lockonLayerMask);
            // The direction of the lockon defaults to the forward of the camera.
            Vector3 referenceDirection = GetMovementVector(0, 1);
            // If the movement stick is pointing in a direction, then our lockon should
            // be based on that angle instead.
            Vector2 movementDir = InputManager.GetMovement(0);
            if (movementDir.magnitude >= InputConstants.movementDeadzone)
            {
                referenceDirection = GetMovementVector(movementDir.x, movementDir.y);
            }

            // Loop through all targets and find the one that matches the angle the best.
            GameObject closestTarget = null;
            float closestAngle = -2.0f;
            float closestDistance = Mathf.Infinity;
            foreach (Collider c in list)
            {
                // Ignore self.
                if (c.gameObject == gameObject)
                {
                    continue;
                }
                // Only objects with ILockonable can be locked on to.
                if (c.TryGetComponent(out ITargetable targetLockonComponent) == false)
                {
                    continue;
                }
                // The target can not be locked on to right now.
                if (targetLockonComponent.Targetable == false)
                {
                    continue;
                }

                Vector3 targetDistance = targetLockonComponent.GetBounds().center - GetBounds().center;
                // If we can't see the target, it can not be locked on to.
                if (Physics.Raycast(GetBounds().center, targetDistance.normalized, out RaycastHit h, targetDistance.magnitude, lockonVisibilityLayerMask))
                {
                    continue;
                }

                targetDistance.y = 0;
                float currAngle = Vector3.Dot(referenceDirection, targetDistance.normalized);
                bool withinFudging = Mathf.Abs(currAngle - closestAngle) <= lockonFudging;
                // Targets have similar positions, choose the closer one.
                if (withinFudging)
                {
                    if (targetDistance.sqrMagnitude < closestDistance)
                    {
                        closestTarget = c.gameObject;
                        closestAngle = currAngle;
                        closestDistance = targetDistance.sqrMagnitude;
                    }
                }
                // Target is closer to the angle than the last one, this is the new target.
                else if (currAngle > closestAngle)
                {
                    closestTarget = c.gameObject;
                    closestAngle = currAngle;
                    closestDistance = targetDistance.sqrMagnitude;
                }
            }

            if(closestTarget != null)
            {
                CurrentTarget = closestTarget.GetComponent<NetworkObject>();
            }
        }

        public Vector3 GetCenter()
        {
            return transform.position + Vector3.up;
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

            StateManager.AddState(new SWallRunH(), (ushort)FighterCmnStates.WALL_RUN_H);
            StateManager.AddState(new SWallJump(), (ushort)FighterCmnStates.WALL_JUMP);

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
            if (PhysicsManager.IsGroundedNetworked)
            {
                StateManager.ChangeState((ushort)FighterCmnStates.BLOCK_HIGH);
            }
            else
            {
                StateManager.ChangeState((ushort)FighterCmnStates.BLOCK_AIR);
            }
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

        [Header("Walls")]
        public LayerMask wallLayerMask;
        public float wallCheckDistance = 0.7f;
        public GameObject currentWall;
        public float ceilingCheckDistance = 1.2f;
        public RaycastHit wallRayHit;
        public float wallRunDot = -0.9f;

        public float sideWallDistance;
        public int wallSide;
        public float wallRunHozMultiplier = 1.0f;

        public RaycastHit lastWallHit;

        public virtual bool TryWallRun()
        {
            if (InputManager.GetMovement(0).magnitude < InputConstants.movementDeadzone) return false;

            RaycastHit rh = DetectWall(out wallSide);
            if (rh.collider == null) return false;

            float dotProduct = Vector3.Dot(rh.normal, GetMovementVector().normalized);
            if (dotProduct < wallRunDot)
            {
                lastWallHit = rh;
                StateManager.ChangeState((int)FighterCmnStates.WALL_RUN_H);
                return true;
            }
            return false;
        }

        RaycastHit forwardRay;
        RaycastHit leftForwardRay;
        RaycastHit rightForwardRay;
        RaycastHit leftRay;
        RaycastHit rightRay;
        /// <summary>
        /// Check if there's a wall in the movement direction we're pointing.
        /// </summary>
        /// <returns>The RaycastHit result.</returns>
        public virtual RaycastHit DetectWall(out int wallDir, bool useCharacterForward = false)
        {
            //Get stick direction.
            Vector3 movement = GetMovementVector();
            Vector3 translatedMovement = (useCharacterForward || movement.magnitude < InputConstants.movementDeadzone)
                ? transform.forward
                : GetMovementVector();
            translatedMovement.y = 0;
            Vector3 translatedLeft = Vector3.Cross(translatedMovement, Vector3.up);

            Vector3 movementLeftForward = (translatedLeft + translatedMovement).normalized;
            Vector3 movementRightForward = ((-translatedLeft) + translatedMovement).normalized;

            //Physics.Raycast(transform.position + new Vector3(0, 1, 0),
            //    translatedMovement.normalized, out forwardRay, wallCheckDistance, wallLayerMask);

            //Debug.DrawRay(transform.position + new Vector3(0, 1, 0),
            //    translatedMovement.normalized * wallCheckDistance, Color.red);

            Physics.Raycast(transform.position + new Vector3(0, 1, 0),
                movementLeftForward, out leftForwardRay, wallCheckDistance, wallLayerMask);

            //Debug.DrawRay(transform.position + new Vector3(0, 1, 0),
            //    movementLeftForward * wallCheckDistance, Color.red);

            Physics.Raycast(transform.position + new Vector3(0, 1, 0),
                translatedLeft.normalized, out leftRay, wallCheckDistance, wallLayerMask);

            //Debug.DrawRay(transform.position + new Vector3(0, 1, 0),
            //    translatedLeft.normalized * wallCheckDistance, Color.red);

            Physics.Raycast(transform.position + new Vector3(0, 1, 0),
                movementRightForward, out rightForwardRay, wallCheckDistance, wallLayerMask);

            //Debug.DrawRay(transform.position + new Vector3(0, 1, 0),
            //    movementRightForward * wallCheckDistance, Color.red);

            Physics.Raycast(transform.position + new Vector3(0, 1, 0),
                -translatedLeft.normalized, out rightRay, wallCheckDistance, wallLayerMask);

            //Debug.DrawRay(transform.position + new Vector3(0, 1, 0),
            //    -translatedLeft.normalized * wallCheckDistance, Color.red);

            FixRaycastHit(ref forwardRay);
            FixRaycastHit(ref leftRay);
            FixRaycastHit(ref leftForwardRay);
            FixRaycastHit(ref rightForwardRay);
            FixRaycastHit(ref rightRay);

            /*
            if (forwardRay.collider != null
                && forwardRay.distance <= leftForwardRay.distance
                && forwardRay.distance <= rightForwardRay.distance
                && forwardRay.distance <= leftRay.distance
                && forwardRay.distance <= rightRay.distance)
            {
                wallDir = 1;
                return forwardRay;
            }
            else*/ if (
               leftForwardRay.collider != null
               && leftForwardRay.distance <= forwardRay.distance
               && leftForwardRay.distance <= leftRay.distance
               && leftForwardRay.distance <= rightForwardRay.distance
               && leftForwardRay.distance <= rightRay.distance)
            {
                wallDir = -1;
                return leftForwardRay;
            }
            else if (
               leftRay.collider != null
               && leftRay.distance <= forwardRay.distance
               && leftRay.distance <= leftForwardRay.distance
               && leftRay.distance <= rightForwardRay.distance
               && leftRay.distance <= rightRay.distance)
            {
                wallDir = -1;
                return leftRay;
            }
            else if (
                rightRay.collider != null
                && rightRay.distance <= forwardRay.distance
                && rightRay.distance <= leftForwardRay.distance
                && rightRay.distance <= rightForwardRay.distance
                && rightRay.distance <= leftRay.distance)
            {
                wallDir = 1;
                return rightRay;
            }
            else
            {
                wallDir = 1;
                return rightForwardRay;
            }
        }

        private void FixRaycastHit(ref RaycastHit rh)
        {
            if (rh.collider == null)
            {
                rh.distance = Mathf.Infinity;
            }
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
            Vector2 movement = ExtDebug.DeadZoner(InputManager.GetMovement(frame), InputConstants.movementDeadzone);
            if(movement == Vector2.zero)
            {
                return Vector3.zero;
            }
            return GetMovementVector(movement.x, movement.y);
        }

        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
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

        public virtual void RotateVisual(Vector3 direction, float speed)
        {
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, speed * Runner.DeltaTime, 0.0f);
            physicsManager.SetRotation(newDirection);
        }

        public void SetTargetable(bool value)
        {
            TargetableNetworked = value;
        }

        public void SetVisualRotation(Vector3 direction)
        {
            physicsManager.SetRotation(direction);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public Bounds GetBounds()
        {
            return capsuleCollider.bounds;
        }
    }
}