using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fusion;
using Rewired;
using UnityEngine;

namespace rwby
{
    //[OrderBefore(typeof(FighterInputManager), typeof(FighterManager))]
    public class LockonCameraManager : BaseCameraManager
    {
        public enum CameraState
        {
            THIRDPERSON,
            LOCK_ON
        }
        
        public CameraState currentCameraState = CameraState.THIRDPERSON;
        
        [Header("Cinemachine")]
        public CinemachineTargetGroup targetGroup;
        public CinemachineStateDrivenCamera virtualStateDrivenCamera;
        public Animator virtualCameraAnimator;
        public CinemachineVirtualCamera[] virtualCameras;
        public CinemachineInputProvider[] inputProvider = new CinemachineInputProvider[0];
        [NonSerialized] public CinemachineBrain cinemachineBrain;
        [NonSerialized] public CinemachinePOV[] virtualCameraPOV;
        [NonSerialized] public CinemachineShake[] virtualCameraShake;

        private ClientManager clientManager;
        private int playerID;
        private Rewired.Player p;
        [SerializeField] private ProfileDefinition currentProfile;
        private PlayerControllerType currentControllerType;
        private bool lockon2DMode = false;

        private CameraSwitcher switcher;

        private FighterManager followTarget;
        private GameObject LockOnTarget;
        private ITargetable lockOnTargetable;
        
        public override void Initialize(CameraSwitcher switcher)
        {
            this.switcher = switcher;
            cinemachineBrain = switcher.cam.GetComponent<CinemachineBrain>();

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
        }
        
        public override void AssignControlTo(ClientManager clientManager, int playerID)
        {
            base.AssignControlTo(clientManager, playerID);
            this.clientManager = clientManager;
            this.playerID = playerID;
            p = ReInput.players.GetPlayer(playerID);
            //SetProfile(GameManager.singleton.profilesManager.GetProfile(clientManager.profiles[playerID]));
            OnControllerTypeChanged(playerID, GameManager.singleton.localPlayerManager.GetPlayerControllerType(playerID));
            //GameManager.singleton.localPlayerManager.OnPlayerControllerTypeChanged -= OnControllerTypeChanged;
            //GameManager.singleton.localPlayerManager.OnPlayerControllerTypeChanged += OnControllerTypeChanged;
        }
        
        public override void SetTarget(FighterManager fighterManager)
        {
            base.SetTarget(fighterManager);
            SetLookAtTarget(fighterManager);
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

        /*
        private void SetProfile(ProfileDefinition profile)
        {
            currentProfile = profile;
        }

        private ProfileDefinition.CameraVariables GetCameraControls()
        {
            return currentControllerType == PlayerControllerType.GAMEPAD
                ? currentProfile.controllerCam
                : currentProfile.keyboardCam;
        }*/

        public override void Activate()
        {
            gameObject.SetActive(true);
            currentCameraState = CameraState.THIRDPERSON;
            virtualCameraAnimator.Play("Follow");
            virtualStateDrivenCamera.Priority = 1;
            base.Activate();
        }

        public override void Deactivate()
        {
            virtualCameraAnimator.Play("Disable");
            virtualStateDrivenCamera.Priority = 0;
            gameObject.SetActive(false);
            base.Deactivate();
        }

        public virtual void Update()
        {
            if (!active) return;
            if (p == null) return;
            ProfileDefinition.CameraVariables cv = switcher.cameraInputManager.GetCameraControls();

            Vector2 stickInput = switcher.cameraInputManager.GetCameraInput(currentCameraState == CameraState.LOCK_ON);
            bool cameraSwitch = p.GetButtonDown(Action.Camera_Switch);

            /*
            if (Mathf.Abs(stickInput.x) < cv.deadzoneHoz) stickInput.x = 0;
            if (Mathf.Abs(stickInput.y) < cv.deadzoneVert) stickInput.y = 0;
            stickInput.x *= currentCameraState == CameraState.LOCK_ON ? cv.speedLockOnHoz : cv.speedHoz;
            stickInput.y *= currentCameraState == CameraState.LOCK_ON ? cv.speedLockOnVert : cv.speedVert;*/

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
            if (!active) return;
            base.Render();
            CamUpdate();
        }

        public virtual void CamUpdate()
        {
            switch (currentCameraState)
            {
                case CameraState.THIRDPERSON:
                    cinemachineBrain.ManualUpdate();
                    TryLockOn();
                    break;
                case CameraState.LOCK_ON:
                    cinemachineBrain.ManualUpdate();
                    Vector3 lookDir = (lockOnTargetable.TargetOrigin.position - followTarget.TargetOrigin.position).normalized;
                    lookDir.y = 0;
                    targetGroup.transform.rotation = Quaternion.LookRotation(lookDir);
                    targetGroup.transform.rotation *= Quaternion.Euler(0, -90, 0);
                    TryLockoff();
                    break;
            }
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
        
        public void SetLookAtTarget(FighterManager target)
        {
            targetGroup.m_Targets[0].target = target.TargetOrigin;
            virtualStateDrivenCamera.Follow = targetGroup.transform;
            virtualStateDrivenCamera.LookAt = targetGroup.transform;
            followTarget = target;
        }
    }
}