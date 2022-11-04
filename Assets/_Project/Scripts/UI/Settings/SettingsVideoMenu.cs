using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui
{
    public class SettingsVideoMenu : MenuBase
    {
        public OptionSlider screen;
        public OptionSlider resolution;
        public OptionSlider vsync;
        public OptionSlider reduceInputLatency;
        public ButtonIntSlider frameRateCap;
        public OptionSlider antiAliasing;

        public OptionSlider textureQuality;
        
        public OptionSlider shadowQuality;
        public OptionSlider ambientOcclusion;
        
        public OptionSlider depthOfField;
        public OptionSlider bloom;
        public OptionSlider lensFlares;
        public OptionSlider vignette;
        public ButtonIntSlider motionBlurStrength;
        public ButtonIntSlider fieldOfView;

        private SettingsDataType modifiedSettings;

        private Resolution[] resolutions;
        private List<string> resStrings = new List<string>();

        public rwby.ui.SettingsMenu settingsMenu;

        private Player rewiredPlayer;
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            resolutions = Screen.resolutions;
            modifiedSettings = GameManager.singleton.settingsManager.Settings;
            gameObject.SetActive(true);
            rewiredPlayer = settingsMenu.playerID == -1
                ? ReInput.players.GetSystemPlayer()
                : ReInput.players.GetPlayer(settingsMenu.playerID);
            SetupOptions();
        }
        
        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            modifiedSettings = null;
            return true;
        }

        private void Update()
        {
            if (rewiredPlayer.GetButtonDown(Action.Apply))
            {
                if (!GatherSettings()) return;
                GameManager.singleton.settingsManager.SetSettings(modifiedSettings);
                GameManager.singleton.settingsManager.ApplyVideoSettings();
                GameManager.singleton.settingsManager.SaveSettings();
                modifiedSettings = GameManager.singleton.settingsManager.Settings;
            }
        }

        private bool GatherSettings()
        {
            int currentRes = resolution.currentOption;
            if (currentRes != 0)
            {
                var wantedResolution = resolutions[currentRes-1];
                modifiedSettings.screenResX = wantedResolution.width;
                modifiedSettings.screenResY = wantedResolution.height;
            }

            modifiedSettings.screenMode = screen.currentOption;
            modifiedSettings.vsync = vsync.currentOption;
            modifiedSettings.reduceInputLatency = reduceInputLatency.currentOption;
            modifiedSettings.frameRateCap = (int)frameRateCap.slider.value;
            if (modifiedSettings.frameRateCap < 10) modifiedSettings.frameRateCap = 0;
            modifiedSettings.antiAliasing = antiAliasing.currentOption;
            modifiedSettings.textureQuality = textureQuality.currentOption;
            modifiedSettings.shadowQuality = shadowQuality.currentOption;
            modifiedSettings.ambientOcclusion = ambientOcclusion.currentOption;
            return true;
        }

        private void SetupOptions()
        {
            resStrings.Clear();
            int currentRes = 0;
            
            resStrings.Add("Custom");
            foreach (var res in resolutions)
            {
                resStrings.Add($"{res.width}x{res.height} ({res.refreshRate})");
                if (res.width == Screen.width && res.height == Screen.height)
                    currentRes = Array.IndexOf(resolutions, res) + 1;
            }

            resolution.options = resStrings.ToArray();
            resolution.SetOption(currentRes);

            screen.options = new[] { "Windowed", "Fullscreen", "Exclusive" };
            screen.SetOption(modifiedSettings.screenMode);

            vsync.options = new[] { "Off", "On", "Triple Buffering" };
            vsync.SetOption(modifiedSettings.vsync);
            
            reduceInputLatency.options = new[] { "Off", "On" };
            reduceInputLatency.SetOption(modifiedSettings.reduceInputLatency);

            frameRateCap.slider.maxValue = 360;
            frameRateCap.slider.minValue = 0;
            frameRateCap.slider.value = modifiedSettings.frameRateCap;
            frameRateCap.valueText.text = modifiedSettings.frameRateCap.ToString();

            antiAliasing.options = new[] { "Off", "FXAA", "SMAA", "MSAA" };
            antiAliasing.SetOption(modifiedSettings.antiAliasing);

            textureQuality.options = new[] { "Lowest", "Low", "Medium", "High" };
            textureQuality.SetOption(modifiedSettings.textureQuality);
            
            shadowQuality.options = new[] { "Off", "Low", "Medium", "High" };
            shadowQuality.SetOption(modifiedSettings.shadowQuality);
            
            ambientOcclusion.options = new[] { "Off", "Low", "Medium", "High" };
            ambientOcclusion.SetOption(modifiedSettings.ambientOcclusion);
        }
    }
}