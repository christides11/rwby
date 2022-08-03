using HnSF.Fighters;
using UnityEngine;
using Fusion;
using rwby;
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
        public Transform FollowTarget
        {
            get { return followTarget.visualTransform; }
        }

        public static PlayerCamera instance;

        [SerializeField] private FighterManager followTarget;
        private GameObject LockOnTarget;
        private ITargetable lockOnTargetable;

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

        [Header("Occulsion")] public CamHandleCutout cutoutHandler;
        
        void Awake()
        {
            instance = this;
            p = ReInput.players.GetPlayer(0);
            currentCameraState = CameraState.THIRDPERSON;
        }
        
        private ClientManager clientManager;
        private int playerID;
        private Rewired.Player p;
        [SerializeField] private ProfileDefinition currentProfile;
        private PlayerControllerType currentControllerType;
        public void Initialize(ClientManager clientManager, int playerID)
        {
            Runner.AddSimulationBehaviour(cutoutHandler, null);
            virtualStateDrivenCamera = Runner.InstantiateInRunnerScene(GameManager.singleton.settings.playerVirtualCameraPrefab, transform.position, Quaternion.identity);

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

            var t = virtualStateDrivenCamera.GetComponentsInChildren<FusionCinemachineCollider>();
            foreach (var v in t)
            {
                v.runner = Runner;
            }
            
            this.clientManager = clientManager;
            this.playerID = playerID;
            p = ReInput.players.GetPlayer(playerID);
            SetProfile(GameManager.singleton.profilesManager.GetProfile(clientManager.profiles[playerID]));
            OnControllerTypeChanged(playerID, GameManager.singleton.localPlayerManager.GetPlayerControllerType(playerID));
            GameManager.singleton.localPlayerManager.OnPlayerControllerTypeChanged += OnControllerTypeChanged;
        }

        private void OnControllerTypeChanged(int playerid, PlayerControllerType controllertype)
        {
            if (playerid != playerID) return;
            currentControllerType = controllertype;
            switch (currentControllerType)
            {
                case PlayerControllerType.GAMEPAD:
                    foreach (var vcam in virtualCameras)
                    {
                        CinemachinePOV pov = vcam.GetCinemachineComponent<CinemachinePOV>();
                        if (!pov) return;
                        pov.m_HorizontalAxis.m_SpeedMode = AxisState.SpeedMode.MaxSpeed;
                        pov.m_HorizontalAxis.m_MaxSpeed = 300;
                        pov.m_HorizontalAxis.m_AccelTime = 0.1f;
                        pov.m_HorizontalAxis.m_DecelTime = 0.1f;
                        pov.m_VerticalAxis.m_SpeedMode = AxisState.SpeedMode.MaxSpeed;
                        pov.m_VerticalAxis.m_MaxSpeed = 300;
                        pov.m_VerticalAxis.m_AccelTime = 0.1f;
                        pov.m_VerticalAxis.m_DecelTime = 0.1f;
                    }
                    break;
                case PlayerControllerType.MOUSE_AND_KEYBOARD:
                    foreach (var vcam in virtualCameras)
                    {
                        CinemachinePOV pov = vcam.GetCinemachineComponent<CinemachinePOV>();
                        if (!pov) return;
                        pov.m_HorizontalAxis.m_SpeedMode = AxisState.SpeedMode.InputValueGain;
                        pov.m_HorizontalAxis.m_MaxSpeed = 1.0f;
                        pov.m_HorizontalAxis.m_AccelTime = 0;
                        pov.m_HorizontalAxis.m_DecelTime = 0;
                        pov.m_VerticalAxis.m_SpeedMode = AxisState.SpeedMode.InputValueGain;
                        pov.m_VerticalAxis.m_MaxSpeed = 1.0f;
                        pov.m_VerticalAxis.m_AccelTime = 0;
                        pov.m_VerticalAxis.m_DecelTime = 0;
                    }
                    break;
            }
        }

        private void SetProfile(ProfileDefinition profile)
        {
            currentProfile = profile;
        }

        private ProfileDefinition.CameraVariables GetCameraControls()
        {
            return currentControllerType == PlayerControllerType.GAMEPAD
                ? currentProfile.controllerCam
                : currentProfile.keyboardCam;
        }

        public virtual void Update()
        {
            if (p == null) return;
            ProfileDefinition.CameraVariables cv = GetCameraControls();
            
            Vector2 stickInput = p.GetAxis2D(Action.Camera_X, Action.Camera_Y);
            bool cameraSwitch = p.GetButtonDown(Action.Camera_Switch);

            if (Mathf.Abs(stickInput.x) < cv.deadzoneHoz) stickInput.x = 0;
            if (Mathf.Abs(stickInput.y) < cv.deadzoneVert) stickInput.y = 0;
            stickInput.x *= currentCameraState == CameraState.LOCK_ON ? cv.speedLockOnHoz : cv.speedHoz;
            stickInput.y *= currentCameraState == CameraState.LOCK_ON ? cv.speedLockOnVert : cv.speedVert;

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

        public void SetLookAtTarget(FighterManager target)
        {
            targetGroup.m_Targets[0].target = target.TargetOrigin;
            virtualStateDrivenCamera.Follow = targetGroup.transform;
            virtualStateDrivenCamera.LookAt = targetGroup.transform;
            followTarget = target;
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
            targetGroup.m_Targets[1].weight = 0.6f;
            targetGroup.m_Targets[1].radius = 2.0f;
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
            targetGroup.m_Targets[1].target = null;
            targetGroup.m_Targets[1].weight = 0;
            targetGroup.m_Targets[1].radius = 0;
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