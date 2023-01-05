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
            
            List<CinemachinePOV> povs = new List<CinemachinePOV>();
            virtualCameraShake = new CinemachineShake[virtualCameras.Length];
            for(int i = 0; i < virtualCameras.Length; i++)
            {
                var p = virtualCameras[i].GetCinemachineComponent<CinemachinePOV>();
                if (p != null)
                {
                    povs.Add(p);
                }
                virtualCameraShake[i] = virtualCameras[i].GetComponent<CinemachineShake>();
            }
            virtualCameraPOV = povs.ToArray();

            var t = virtualStateDrivenCamera.GetComponentsInChildren<FusionCinemachineCollider>();
            foreach (var v in t)
            {
                v.runner = Runner;
            }

            virtualCameras[0].GetCinemachineComponent<CinemachineFramingTransposer>().m_GroupFramingMode =
                CinemachineFramingTransposer.FramingMode.None;
            virtualCameras[1].GetCinemachineComponent<CinemachineFramingTransposer>().m_MaximumDistance = 15;
            virtualCameras[1].GetCinemachineComponent<CinemachineFramingTransposer>().m_MaximumFOV = 50;
            virtualCameras[1].GetCinemachineComponent<CinemachineFramingTransposer>().m_MinimumFOV = 50;
        }
        
        public override void AssignControlTo(ClientManager clientManager, int playerID)
        {
            base.AssignControlTo(clientManager, playerID);
            this.clientManager = clientManager;
            this.playerID = playerID;
            p = ReInput.players.GetPlayer(playerID);
            OnControllerTypeChanged(playerID, GameManager.singleton.localPlayerManager.GetPlayerControllerType(playerID));
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

        public float targetWeight = 1.0f;
        private void LockOnToTarget(GameObject target)
        {
            LockOnTarget = target;
            lockOnTargetable = target.GetComponent<ITargetable>();
            targetGroup.m_Targets[1].target = lockOnTargetable.TargetOrigin;
            targetGroup.m_Targets[1].weight = targetWeight;
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

        public override void ShakeCamera(float strength, float time)
        {
            foreach (var vcs in virtualCameraShake)
            {
                vcs.ShakeCamera(strength, time);
            }
        }

        public override void StopShaking()
        {
            foreach (var vcs in virtualCameraShake)
            {
                vcs.Reset();
            }
        }

        public override Vector2 GetLookDirection()
        {
            switch (currentCameraState)
            {
                case CameraState.THIRDPERSON:
                    return new Vector2(virtualCameraPOV[0].m_HorizontalAxis.Value, virtualCameraPOV[0].m_VerticalAxis.Value);
                    break;
                case CameraState.LOCK_ON:
                    return lockon2DMode == true ? new Vector2(virtualCameraPOV[1].m_HorizontalAxis.Value, virtualCameraPOV[1].m_VerticalAxis.Value)
                        : new Vector2(virtualCameraPOV[1].m_HorizontalAxis.Value, virtualCameraPOV[1].m_VerticalAxis.Value);
                    break;
            }
            return Vector2.zero;
        }

        public override void SetLookDirection(Vector2 lookDir)
        {
            foreach (var pov in virtualCameraPOV)
            {
                pov.m_HorizontalAxis.Value = lookDir.x;
                pov.m_VerticalAxis.Value = lookDir.y;
            }
        }
    }
}