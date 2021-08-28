using HnSF.Fighters;
using UnityEngine;
using Fusion;
using rwby;
using ThirdPersonCameraWithLockOn;
using Cinemachine;
using Rewired;

namespace rwby
{
    public class PlayerCamera : MonoBehaviour
    {
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

        private Transform followTarget;

        Rewired.Player p = null;

        void Awake()
        {
            p = ReInput.players.GetPlayer(0);
            thirdPersonaCamera.SetCameraState(ThirdPersonCamera.CamStates.Off);
            freeLook = GameObject.Instantiate(GameManager.singleton.settings.followVirtualCameraPrefab, transform.position, Quaternion.identity).GetComponent<CinemachineVirtualCamera>();
            inputProvider = freeLook.GetComponent<CinemachineInputProvider>();
        }

        public virtual void CamUpdate()
        {
            cinemachineBrain.ManualUpdate();
            thirdPersonaCamera.ManualUpdate();
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

        public void SetLookAtTarget(Transform target)
        {
            thirdPersonaCamera.Follow = target;
            freeLook.Follow = target;
            freeLook.LookAt = target;
            followTarget = target;
        }

        public void SetLockOnTarget(FighterManager entityTarget)
        {
            
            if (entityTarget == null)
            {
                thirdPersonaCamera.ExitLockOn();
                return;
            }
            thirdPersonaCamera.InitiateLockOn(entityTarget.gameObject);
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