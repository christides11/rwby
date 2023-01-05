using System;
using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using UnityEngine;
using UnityEngine.Events;

namespace rwby.ui
{
    public class SettingsProfileCustomizationMenu : MenuBase
    {
        private ProfilesManager profilesManager;
        private ProfileDefinition currentProfile;
        [SerializeField] private SettingsProfilesMenu profilesMenu;

        [Header("Settings")] 
        public ContentButtonInputField profileName;
        public OptionSlider lockOnType;

        public ButtonFloatSlider controller_horizontalDeadzone;
        public ButtonFloatSlider controller_verticalDeadzone;
        public ButtonFloatSlider controller_horizontalSpeed;
        public ButtonFloatSlider controller_verticalSpeed;
        public ButtonFloatSlider controller_horizontalSpeedLockedOn;
        public ButtonFloatSlider controller_verticalSpeedLockedOn;
        
        public ButtonFloatSlider keyboard_horizontalDeadzone;
        public ButtonFloatSlider keyboard_verticalDeadzone;
        public ButtonFloatSlider keyboard_horizontalSpeed;
        public ButtonFloatSlider keyboard_verticalSpeed;
        public ButtonFloatSlider keyboard_horizontalSpeedLockedOn;
        public ButtonFloatSlider keyboard_verticalSpeedLockedOn;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            profilesManager = GameManager.singleton.profilesManager;
            currentProfile = profilesManager.Profiles[profilesMenu.currentSelectedProfile];
            gameObject.SetActive(true);
            SetupOptions();
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }

        public void BUTTON_Back()
        {
            profilesMenu.Back();
        }

        public void BUTTON_SaveProfile()
        {
            GatherSettings();
            profilesManager.ApplyProfile(currentProfile, profilesMenu.currentSelectedProfile);
            profilesManager.SaveProfiles();
            profilesMenu.Back();
        }

        public void BUTTON_TestProfile()
        {
            
        }

        private UnityAction screenClosedEvent;
        public void BUTTON_RemapControls()
        {
            GameManager.singleton.localPlayerManager.AutoAssignControllers();
            var profilesManager = GameManager.singleton.profilesManager;
            profilesManager.ApplyProfileToPlayer(0, profilesMenu.currentSelectedProfile);
            screenClosedEvent = () => ApplyRemappedControls(profilesMenu.currentSelectedProfile);
            GameManager.singleton.cMapper.onScreenClosed += screenClosedEvent;
            GameManager.singleton.cMapper.Open();
        }

        private void ApplyRemappedControls(int profilesMenuCurrentSelectedProfile)
        {
            GatherSettings();
            profilesManager.ApplyProfile(currentProfile, profilesMenu.currentSelectedProfile);
         
            var playerID = 0;
            GameManager.singleton.profilesManager.ApplyControlsToProfile(playerID, profilesMenu.currentSelectedProfile);
            GameManager.singleton.profilesManager.SaveProfiles();
            GameManager.singleton.profilesManager.RestoreDefaultControls(playerID);
            GameManager.singleton.profilesManager.ApplyProfileToPlayer(playerID,
                GameManager.singleton.localPlayerManager.GetPlayer(playerID).profile);
            GameManager.singleton.cMapper.onScreenClosed -= screenClosedEvent;
            
            currentProfile = profilesManager.Profiles[profilesMenu.currentSelectedProfile];
        }

        private void GatherSettings()
        {
            currentProfile.profileName = profileName.inputField.text;
            currentProfile.lockOnType = lockOnType.currentOption;
            currentProfile.controllerCam.deadzoneHoz = controller_horizontalDeadzone.slider.value;
            currentProfile.controllerCam.deadzoneVert = controller_verticalDeadzone.slider.value;
            currentProfile.controllerCam.speedHoz = controller_horizontalSpeed.slider.value;
            currentProfile.controllerCam.speedVert = controller_verticalSpeed.slider.value;
            currentProfile.controllerCam.speedLockOnHoz = controller_horizontalSpeedLockedOn.slider.value;
            currentProfile.controllerCam.speedLockOnHoz = controller_verticalSpeedLockedOn.slider.value;
            
            currentProfile.keyboardCam.deadzoneHoz = keyboard_horizontalDeadzone.slider.value;
            currentProfile.keyboardCam.deadzoneVert = keyboard_verticalDeadzone.slider.value;
            currentProfile.keyboardCam.speedHoz = keyboard_horizontalSpeed.slider.value;
            currentProfile.keyboardCam.speedVert = keyboard_verticalSpeed.slider.value;
            currentProfile.keyboardCam.speedLockOnHoz = keyboard_horizontalSpeedLockedOn.slider.value;
            currentProfile.keyboardCam.speedLockOnHoz = keyboard_verticalSpeedLockedOn.slider.value;
        }
        
        private void SetupOptions()
        {
            profileName.inputField.text = currentProfile.profileName;
            ConfigureOptionsSlider(new []{"Hold", "Toggle"}, currentProfile.lockOnType, lockOnType);
            ConfigureFloatSlider(0, .9f, currentProfile.controllerCam.deadzoneHoz, controller_horizontalDeadzone);
            ConfigureFloatSlider(0, .9f, currentProfile.controllerCam.deadzoneVert, controller_verticalDeadzone);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.controllerCam.speedHoz, controller_horizontalSpeed);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.controllerCam.speedVert, controller_verticalSpeed);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.controllerCam.speedLockOnHoz, controller_horizontalSpeedLockedOn);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.controllerCam.speedLockOnVert, controller_verticalSpeedLockedOn);

            ConfigureFloatSlider(0, .9f, currentProfile.keyboardCam.deadzoneHoz, keyboard_horizontalDeadzone);
            ConfigureFloatSlider(0, .9f, currentProfile.keyboardCam.deadzoneVert, keyboard_verticalDeadzone);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.keyboardCam.speedHoz, keyboard_horizontalSpeed);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.keyboardCam.speedVert, keyboard_verticalSpeed);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.keyboardCam.speedLockOnHoz, keyboard_horizontalSpeedLockedOn);
            ConfigureFloatSlider(.1f, 5.0f, currentProfile.keyboardCam.speedLockOnVert, keyboard_verticalSpeedLockedOn);
        }

        private void ConfigureOptionsSlider(string[] options, int option, OptionSlider optionSlider)
        {
            optionSlider.options = options;
            optionSlider.SetOption(option);
        }

        private void ConfigureFloatSlider(float minValue, float maxValue, float value, ButtonFloatSlider floatSlider)
        {
            floatSlider.slider.minValue = minValue;
            floatSlider.slider.maxValue = maxValue;
            floatSlider.slider.value = value;
        }
    }
}