using HnSF.Fighters;
using UnityEngine;
using Fusion;
using rwby;
using ThirdPersonCameraWithLockOn;
using Cinemachine;
using Rewired;
using HnSF.Combat;

namespace rwby
{
    public class PlayerCamera : MonoBehaviour
    {
        public enum CameraState
        {
            THIRDPERSON,
            LOCK_ON
        }

        public Camera Cam { get { return cam; } }

        [Header("References")]
        [SerializeField] private Camera cam;
        [SerializeField] private ThirdPersonCamera thirdPersonaCamera;

        [Header("CineMachine Follow Cam")]
        public CinemachineBrain cinemachineBrain;
        public CinemachineVirtualCamera freeLook;
        public CinemachineInputProvider inputProvider;

        [Header("Mouse")]
        [SerializeField] private float mouseDeadzone = 0.05f;
        [SerializeField] private float mouseXAxisSpeed = 1.0f;
        [SerializeField] private float mouseYAxisSpeed = 1.0f;

        [Header("Controller")]
        [SerializeField] private float stickDeadzone = 0.2f;
        [SerializeField] private float stickAxialDeadZone = 0.15f;
        [SerializeField] private float stickXAxisSpeed = 1.0f;
        [SerializeField] private float stickYAxisSpeed = 1.0f;

        [SerializeField] private FighterManager followTarget;
        [SerializeField] private GameObject LockOnTarget;

        Rewired.Player p = null;

        [Header("Lock On")]
        public LayerMask lockonLayerMask;
        public LayerMask lockonVisibilityLayerMask;
        public float lockonMaxDistance = 20;
        public float lockonFudging = 0.1f;
        public CameraState currentCameraState = CameraState.THIRDPERSON;

        void Awake()
        {
            currentCameraState = CameraState.THIRDPERSON;
            p = ReInput.players.GetPlayer(0);
            thirdPersonaCamera.SetCameraState(ThirdPersonCamera.CamStates.Off);
            freeLook = GameObject.Instantiate(GameManager.singleton.settings.followVirtualCameraPrefab, transform.position, Quaternion.identity).GetComponent<CinemachineVirtualCamera>();
            inputProvider = freeLook.GetComponent<CinemachineInputProvider>();
        }

        public virtual void CamUpdate()
        {
            switch (currentCameraState)
            {
                case CameraState.THIRDPERSON:
                    HandleThirdPerson();
                    break;
                case CameraState.LOCK_ON:
                    HandleLockOn();
                    break;
            }
        }

        private void HandleThirdPerson()
        {
            cinemachineBrain.ManualUpdate();
            TryLockOn();
        }

        private void TryLockOn()
        {
            if (p.GetButton(Action.Lock_On) == false) return;

            PickLockonTarget();
        }

        private void PickLockonTarget()
        {
            Collider[] list = Physics.OverlapSphere(followTarget.GetCenter(), lockonMaxDistance, lockonLayerMask);
            // The direction of the lockon defaults to the forward of the camera.
            Vector3 referenceDirection = followTarget.GetMovementVector(0, 1);
            // If the movement stick is pointing in a direction, then our lockon should
            // be based on that angle instead.
            Vector2 movementDir = followTarget.InputManager.GetMovement(0);
            if (movementDir.magnitude >= InputConstants.movementDeadzone)
            {
                referenceDirection = followTarget.GetMovementVector(movementDir.x, movementDir.y);
            }

            // Loop through all targets and find the one that matches the angle the best.
            GameObject closestTarget = null;
            float closestAngle = -2.0f;
            float closestDistance = Mathf.Infinity;
            foreach (Collider c in list)
            {
                // Ignore self.
                if (c.gameObject == followTarget.gameObject)
                {
                    continue;
                }
                // Only objects with ILockonable can be locked on to.
                if (c.TryGetComponent(out ITargetable targetLockonComponent) == false)
                {
                    continue;
                }
                // The target can not be locked on to right now.
                if (!targetLockonComponent.Targetable)
                {
                    continue;
                }

                Vector3 targetDistance = targetLockonComponent.GetBounds().center - followTarget.GetBounds().center;
                // If we can't see the target, it can not be locked on to.
                if (Physics.Raycast(followTarget.GetBounds().center, targetDistance.normalized, out RaycastHit h, targetDistance.magnitude, lockonVisibilityLayerMask))
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

            if (closestTarget != null)
            {
                LockOnToTarget(closestTarget);
            }
        }

        private void LockOnToTarget(GameObject target)
        {
            LockOnTarget = target;
            thirdPersonaCamera.InitiateLockOn(target);
            currentCameraState = CameraState.LOCK_ON;
            followTarget.HardTargeting = true;
        }

        private void HandleLockOn()
        {
            Vector3 dir = (LockOnTarget.transform.position - followTarget.transform.position);
            dir.y = 0;
            followTarget.TargetingForward = new Vector2(dir.x, dir.z);
            thirdPersonaCamera.ManualUpdate();
            TryLockoff();
        }

        private void TryLockoff()
        {
            if (p.GetButton(Action.Lock_On) == true) return;
            thirdPersonaCamera.ExitLockOn();
            currentCameraState = CameraState.THIRDPERSON;
            followTarget.HardTargeting = false;
        }

        public virtual void Update()
        {
            Vector2 stickInput = p.GetAxis2D(Action.Camera_X, Action.Camera_Y);

            // TODO: modify input based on if M&K or controller

            inputProvider.input = stickInput;
            thirdPersonaCamera.cameraX = stickInput.x;
            thirdPersonaCamera.cameraY = stickInput.y;

            /*
            Mahou.Input.GlobalInputManager inputManager = (Mahou.Input.GlobalInputManager)GlobalInputManager.instance;
            Vector2 stickInput = inputManager.GetAxis2D(0, Input.Action.Camera_X, Input.Action.Camera_Y);

            switch (inputManager.GetCurrentInputMethod(0))
            {
                case CurrentInputMethod.MK:
                    if (Mathf.Abs(stickInput.x) <= mouseDeadzone)
                    {
                        stickInput.x = 0;
                    }
                    if (Mathf.Abs(stickInput.y) <= mouseDeadzone)
                    {
                        stickInput.y = 0;
                    }
                    stickInput.x *= mouseXAxisSpeed;
                    stickInput.y *= mouseYAxisSpeed;
                    break;
                case CurrentInputMethod.CONTROLLER:
                    if (stickInput.magnitude < stickDeadzone)
                    {
                        stickInput = Vector2.zero;
                    }
                    else
                    {
                        float d = ((stickInput.magnitude - stickDeadzone) / (1.0f - stickDeadzone));
                        d = Mathf.Min(d, 1.0f);
                        d *= d;
                        stickInput = stickInput.normalized * d;
                    }
                    if (Mathf.Abs(stickInput.x) < stickAxialDeadZone)
                    {
                        stickInput.x = 0;
                    }
                    if (Mathf.Abs(stickInput.y) < stickAxialDeadZone)
                    {
                        stickInput.y = 0;
                    }
                    stickInput.x *= stickXAxisSpeed;
                    stickInput.y *= stickYAxisSpeed;
                    break;
            }

            inputProvider.input = stickInput;
            thirdPersonaCamera.cameraX = stickInput.x;
            thirdPersonaCamera.cameraY = stickInput.y;*/
        }

        public void LookAt(Vector3 position)
        {

        }

        public Transform LookTransform()
        {
            return transform;
        }

        public void Reset()
        {

        }

        public void SetLookAtTarget(FighterManager target)
        {
            thirdPersonaCamera.Follow = target.transform;
            freeLook.Follow = target.transform;
            freeLook.LookAt = target.transform;
            followTarget = target;
        }

        public void SetLockOnTarget(FighterManager entityTarget)
        {

        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void SetRotation(Vector3 rotation)
        {
            transform.eulerAngles = rotation;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}