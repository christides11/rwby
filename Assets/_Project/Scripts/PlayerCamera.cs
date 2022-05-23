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
    [OrderBefore(typeof(FighterInputManager), typeof(FighterManager))]
    public class PlayerCamera : SimulationBehaviour
    {
        public enum CameraState
        {
            THIRDPERSON,
            LOCK_ON
        }

        public Camera Cam { get { return cam; } }

        public static PlayerCamera instance;

        [SerializeField] private FighterManager followTarget;
        private GameObject LockOnTarget;
        private ITargetable lockOnTargetable;
        Rewired.Player p = null;

        [Header("References")]
        [SerializeField] private Camera cam;

        [Header("Cinemachine")]
        public CinemachineBrain cinemachineBrain;
        [System.NonSerialized] public CinemachineStateDrivenCamera virtualStateDrivenCamera;
        [System.NonSerialized] public Animator virtualCameraAnimator;
        public CinemachineTargetGroup targetGroup;
        [System.NonSerialized] public CinemachineVirtualCamera[] virtualCameras;
        [System.NonSerialized] public CinemachinePOV[] virtualCameraPOV;
        [System.NonSerialized] public CinemachineShake[] virtualCameraShake;
        [System.NonSerialized] public CinemachineInputProvider[] inputProvider;

        [Header("Lock On")]
        public LayerMask lockonLayerMask;
        public LayerMask lockonVisibilityLayerMask;
        public float lockonMaxDistance = 20;
        public float lockonFudging = 0.1f;
        public CameraState currentCameraState = CameraState.THIRDPERSON;

        void Awake()
        {
            instance = this;
            currentCameraState = CameraState.THIRDPERSON;
            p = ReInput.players.GetPlayer(0);
            virtualStateDrivenCamera = GameObject.Instantiate(GameManager.singleton.settings.playerVirtualCameraPrefab, transform.position, Quaternion.identity);
            
            virtualCameraAnimator = virtualStateDrivenCamera.GetComponent<Animator>();
            inputProvider = virtualStateDrivenCamera.GetComponentsInChildren<CinemachineInputProvider>();
            virtualCameras = virtualStateDrivenCamera.GetComponentsInChildren<CinemachineVirtualCamera>();

            virtualCameraPOV = new CinemachinePOV[virtualCameras.Length];
            virtualCameraShake = new CinemachineShake[virtualCameras.Length];
            for(int i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameraPOV[i] = virtualCameras[i].GetCinemachineComponent<CinemachinePOV>();
                virtualCameraShake[i] = virtualCameras[i].GetComponent<CinemachineShake>();
            } 
        }

        public override void Render()
        {
            base.Render();
            CamUpdate();
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
            if (!followTarget || followTarget.HardTargeting == false || followTarget.CurrentTarget == null) return;
            LockOnToTarget(followTarget.CurrentTarget.gameObject);
        }

        private void LockOnToTarget(GameObject target)
        {
            LockOnTarget = target;
            lockOnTargetable = target.GetComponent<ITargetable>();
            targetGroup.m_Targets[1].target = lockOnTargetable.TargetOrigin;
            virtualStateDrivenCamera.Follow = targetGroup.transform;
            virtualStateDrivenCamera.LookAt = targetGroup.transform;
            SetVirtualCameraInputs(0);
            virtualCameraAnimator.Play("Target");
            lockon2DMode = false;
            currentCameraState = CameraState.LOCK_ON;
        }

        bool lockon2DMode = false;
        private void HandleLockOn()
        {
            cinemachineBrain.ManualUpdate();
            Vector3 lookDir = (lockOnTargetable.TargetOrigin.position - followTarget.TargetOrigin.position).normalized;
            lookDir.y = 0;
            targetGroup.transform.rotation = Quaternion.LookRotation(lookDir);
            targetGroup.transform.rotation *= Quaternion.Euler(0, -90, 0);
            TryLockoff();
        }

        private void TryLockoff()
        {
            if (followTarget.HardTargeting == true && followTarget.CurrentTarget != null) return;
            virtualStateDrivenCamera.Follow = followTarget.TargetOrigin;
            virtualStateDrivenCamera.LookAt = followTarget.TargetOrigin;
            SetVirtualCameraInputs(1);
            virtualCameraAnimator.Play("Follow");
            currentCameraState = CameraState.THIRDPERSON;
        }

        private void SetVirtualCameraInputs(int currentIndex)
        {
            for(int i = 0; i < 2; i++)
            {
                virtualCameraPOV[i].m_VerticalAxis = virtualCameraPOV[currentIndex].m_VerticalAxis;
                virtualCameraPOV[i].m_HorizontalAxis = virtualCameraPOV[currentIndex].m_HorizontalAxis;
            }
        }

        public virtual void Update()
        {
            Vector2 stickInput = p.GetAxis2D(Action.Camera_X, Action.Camera_Y);
            bool cameraSwitch = p.GetButtonDown(Action.Camera_Switch);

            for (int i = 0; i < inputProvider.Length; i++)
            {
                inputProvider[i].input = stickInput;
            }

            if (cameraSwitch && currentCameraState == CameraState.LOCK_ON)
            {
                lockon2DMode = !lockon2DMode;
                if (lockon2DMode)
                {
                    virtualCameraAnimator.Play("Target2D");
                }
                else
                {
                    virtualCameraAnimator.Play("Target");
                }
            }
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
            virtualStateDrivenCamera.Follow = target.TargetOrigin;
            virtualStateDrivenCamera.LookAt = target.TargetOrigin;
            targetGroup.m_Targets[0].target = target.TargetOrigin;
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

        public void ShakeCamera(float intensity, float time)
        {
            for (int i = 0; i < virtualCameraShake.Length; i++)
            {
                virtualCameraShake[i].ShakeCamera(intensity, time);
            }
        }
    }
}