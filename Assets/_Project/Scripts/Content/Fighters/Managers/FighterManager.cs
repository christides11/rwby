using Cysharp.Threading.Tasks;
using Fusion;    
using HnSF.Combat;
using HnSF.Fighters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(FighterStateManager), typeof(Fusion.HitboxManager), typeof(FighterPhysicsManager), typeof(FighterBoxManager), typeof(FighterHitManager), typeof(FighterCombatManager))]
    [OrderAfter(typeof(FighterInputManager))]
    public class FighterManager : NetworkBehaviour, IFighterBase, ITargetable, IContentLoad
    {
        public IFighterCombatManager CombatManager
        {
            get { return combatManager; }
        }
        public IFighterStateManager StateManager
        {
            get { return stateManager; }
        }
        public IFighterPhysicsManager PhysicsManager
        {
            get { return physicsManager; }
        }
        public FighterInputManager InputManager { get { return inputManager; } }
        public FighterCombatManager FCombatManager { get { return combatManager; } }
        public FighterStateManager FStateManager { get { return stateManager; } }
        public FighterStatManager StatManager { get { return statManager; } }
        public FighterPhysicsManager FPhysicsManager { get { return physicsManager; } }
        public HealthManager HealthManager{ get { return healthManager; } }
        public FighterBoxManager BoxManager { get { return boxManager; } }
        public SoundbankContainer SoundbankContainer { get { return soundbankContainer; } }
        public Transform TargetOrigin { get { return targetOrigin; } }
        public IEnumerable<ModGUIDContentReference> loadedContent
        {
            get { return GetLoadedContentList(); }
        }

        [Networked] public bool TargetableNetworked { get; set; }
        public bool Targetable { get { return TargetableNetworked; } }

        [Networked] public NetworkBool HardTargeting { get; set; }
        [Networked] public NetworkObject CurrentTarget { get; set; }
        [Networked] public NetworkBool Visible { get; set; }

        // Stats
        [Networked] public NetworkBool StoredRun { get; set; }
        [Networked] public int CurrentJump { get; set; }
        [Networked] public int CurrentAirDash { get; set; }
        [Networked] public float CurrentFallMultiplier { get; set; } = 1.0f;
        [Networked, Capacity(10)] public NetworkArray<bool> attackEventInput { get; }

        [Header("References")]
        [NonSerialized] public NetworkManager networkManager;
        [SerializeField] protected FighterInputManager inputManager;
        [SerializeField] protected FighterCombatManager combatManager;
        [SerializeField] protected FighterStateManager stateManager;
        [SerializeField] protected FighterPhysicsManager physicsManager;
        [SerializeField] protected FighterBoxManager boxManager;
        [SerializeField] protected FighterStatManager statManager;
        [SerializeField] protected HealthManager healthManager;
        public IFighterDefinition fighterDefinition;
        [SerializeField] protected CapsuleCollider capsuleCollider;
        [SerializeField] protected SoundbankContainer soundbankContainer;
        public FighterEffector fighterEffector;
        public FighterAnimator fighterAnimator;
        [SerializeField] protected Transform targetOrigin;
        public ParticleSystemEffect guardEffect;
        public Transform visualTransform;
        public Transform myTransform;
        [SerializeReference] public IContentLoad[] contentLoaders = new IContentLoad[0];

        [Header("Lock On")]
        public LayerMask lockonLayerMask;
        public LayerMask lockonVisibilityLayerMask;
        public float lockonMaxDistance = 20;
        public float lockonFudging = 0.1f;

        [Header("Debug")] public bool FRAMEBYFRAME = false;

        public virtual async UniTask<bool> OnFighterLoaded()
        {
            return true;
        }

        public virtual void Awake()
        {
            //SessionManagerClassic = SessionManagerClassic.singleton;
            networkManager = NetworkManager.singleton;
            stateManager.movesets = fighterDefinition.GetMovesets();
            foreach (var moveset in stateManager.movesets)
            {
                (moveset as Moveset).Initialize();
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            TargetableNetworked = true;
            Visible = true;
            combatManager.Cleanup();
            stateManager.SetMoveset(0);
            statManager.SetupStats((stateManager.movesets[0] as Moveset).fighterStats);
        }

        public float hitstopShakeDistance = 0.5f;
        public int hitstopDir = 1;
        public int hitstopShakeFrames = 1;

        public override void Render()
        {
            base.Render();
            visualTransform.localPosition = Vector3.zero;
            if (FCombatManager.HitStop == 0) return;
            Vector3 dir = shakeDirs[currentShakeDirection].z * transform.forward
                          + shakeDirs[currentShakeDirection].x * transform.right;
            visualTransform.localPosition = dir * hitstopShakeDistance * hitstopDir;
        }

        public int val = 100;
        public override void FixedUpdateNetwork()
        {
            inputManager.FeedInput();
            if (Runner.Simulation.IsResimulation && Runner.Simulation.IsFirstTick)
            {
                FPhysicsManager.ResimulationResync();
            }

            if (FRAMEBYFRAME && !(Input.GetKeyDown(KeyCode.X) || Input.GetKey(KeyCode.C)))
            {
                FPhysicsManager.Freeze();
                return;
            }
            boxManager.ResetAllBoxes();
            visualTransform.gameObject.SetActive(Visible);

            HitstopShake();
            HandleLockon();

            
            if (FCombatManager.HitStop == 0)
            {
                if (FCombatManager.BlockStun > 0)
                {
                    FCombatManager.BlockStun--;
                }
                FPhysicsManager.CheckIfGrounded();
                FStateManager.Tick();
                FPhysicsManager.Tick();
            }
            else
            {
                FCombatManager.hitstopCounter++;
                FPhysicsManager.Freeze();
                if(FCombatManager.hitstopCounter == FCombatManager.HitStop)
                {
                    FCombatManager.HitStop = 0;
                    currentShakeDirection = 0;
                    FCombatManager.hitstopCounter = 0;
                }
            }
        }
        
        public Vector3[] shakeDirs;
        [Networked] public sbyte currentShakeDirection { get; set; }

        protected void HitstopShake()
        {
            // Shake during hitstop.
            if (FCombatManager.HitStop != 0
                && (FCombatManager.HitStun > 0 || FCombatManager.BlockStun > 0)
                && FCombatManager.HitStop % hitstopShakeFrames == 0)
            {
                Vector3 dir = shakeDirs[currentShakeDirection].z * transform.forward
                    + shakeDirs[currentShakeDirection].x * transform.right;
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
            PickLockonTarget(lockonMaxDistance);
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

        public void PickLockonTarget(float maxDistance)
        {
            CurrentTarget = null;
            var target = GetLockonTarget(maxDistance);
            if(target != null)
            {
                CurrentTarget = target.GetComponent<NetworkObject>();
            }
        }

        private Collider[] lockonResultList = new Collider[10];
        public GameObject GetLockonTarget(float maxDistance)
        {
            int hitCount = Runner.GetPhysicsScene().OverlapSphere(GetCenter(), maxDistance, lockonResultList, 
                lockonLayerMask, QueryTriggerInteraction.UseGlobal);
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
            for(int i = 0; i < hitCount; i++)
            {
                Collider c = lockonResultList[i];
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
            return closestTarget;
        }

        public Vector3 GetCenter()
        {
            return transform.position + Vector3.up;
        }

        public bool TryBlock()
        {
            if(InputManager.GetBlock(out int bOff).isDown == false)
            {
                return false;
            }
            if (FPhysicsManager.IsGroundedNetworked)
            {
                FStateManager.ChangeState((int)FighterCmnStates.BLOCK_HIGH);
            }
            else
            {
                FStateManager.ChangeState((int)FighterCmnStates.BLOCK_AIR);
            }
            return true;
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
                FStateManager.ChangeState((int)FighterCmnStates.WALL_RUN_H);
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

            Physics.Raycast(transform.position + new Vector3(0, 1, 0),
                translatedLeft.normalized, out leftRay, wallCheckDistance, wallLayerMask);

            Physics.Raycast(transform.position + new Vector3(0, 1, 0),
                movementRightForward, out rightForwardRay, wallCheckDistance, wallLayerMask);

            Physics.Raycast(transform.position + new Vector3(0, 1, 0),
                -translatedLeft.normalized, out rightRay, wallCheckDistance, wallLayerMask);

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
            //StoredRun = false;
            CurrentAirDash = 0;
            CurrentJump = 0;
            FPhysicsManager.forceGravity = 0;
            combatManager.ResetString();
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

        public virtual Vector3 GetVisualMovementVector(int frame = 0)
        {
            Vector2 movement = ExtDebug.DeadZoner(InputManager.GetMovement(frame), InputConstants.movementDeadzone);
            if(movement == Vector2.zero)
            {
                return Vector3.zero;
            }
            return GetVisualMovementVector(movement.x, movement.y);
        }

        public virtual Vector3 GetVisualMovementVector(float horizontal, float vertical)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return forward * vertical + right * horizontal;
        }

        public virtual void RotateTowards(Vector3 direction, float speed)
        {
            Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, speed * Runner.DeltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
        }

        public void SetVisualRotation(Vector3 direction)
        {
            physicsManager.SetRotation(direction);
        }
        
        public void SetTargetable(bool value)
        {
            TargetableNetworked = value;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public Bounds GetBounds()
        {
            return capsuleCollider.bounds;
        }

        public virtual List<ModGUIDContentReference> GetLoadedContentList()
        {
            List<ModGUIDContentReference> references = new List<ModGUIDContentReference>();

            foreach (var contentLoad in contentLoaders)
            {
                references.AddRange(contentLoad.loadedContent);
            }
            
            return references;
        }
    }
}