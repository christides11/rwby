using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphicsConfigurator.API.URP;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace rwby
{
    public class SettingsManager : MonoBehaviour
    {
        public static string SETTINGS_PATH;
        
        public SettingsDataType Settings
        {
            get { return currentSettings; }
        }

        private SettingsDataType currentSettings = new SettingsDataType();

        [SerializeField] private ForceRenderRate forceRenderRate;
        
        private void Awake()
        {
            SETTINGS_PATH = Application.persistentDataPath + "/settings.json";
            Application.targetFrameRate = -1;
        }

        public void SetSettings(SettingsDataType wantedSettings)
        {
            currentSettings = wantedSettings;
        }

        public void LoadSettings()
        {
            if (!SaveLoadJsonService.TryLoad(SETTINGS_PATH, out SettingsDataType loadedData))
            {
                SaveLoadJsonService.Save(SETTINGS_PATH, currentSettings, true);
                return;
            }
            currentSettings = loadedData;
        }
        
        public void ApplyVideoSettings()
        {
            FullScreenMode fullscreenMode = FullScreenMode.Windowed;
            switch (currentSettings.screenMode)
            {
                case 0:
                    fullscreenMode = FullScreenMode.Windowed;
                    break;
                case 1:
                    fullscreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 2:
                    fullscreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
            }
            Screen.SetResolution(currentSettings.screenResX == 0 ? Screen.width : currentSettings.screenResX, 
                currentSettings.screenResY == 0 ? Screen.height : currentSettings.screenResY, 
                fullscreenMode);

            switch (currentSettings.vsync)
            {
                case 0:
                    QualitySettings.vSyncCount = 0;
                    QualitySettings.maxQueuedFrames = 2;
                    break;
                case 1:
                    QualitySettings.vSyncCount = 1;
                    QualitySettings.maxQueuedFrames = 2;
                    break;
                case 2:
                    QualitySettings.vSyncCount = 1;
                    QualitySettings.maxQueuedFrames = 3;
                    break;
                default:
                    QualitySettings.vSyncCount = 0;
                    QualitySettings.maxQueuedFrames = 2;
                    break;
            }

            if (currentSettings.reduceInputLatency == 1) QualitySettings.maxQueuedFrames = 0;
            
            forceRenderRate.Deactivate();
            
            if (currentSettings.vsync == 0 && currentSettings.frameRateCap > 0)
            {
                forceRenderRate.Activate(currentSettings.frameRateCap);
            }
            else
            {
                forceRenderRate.Deactivate();
            }

            switch (currentSettings.antiAliasing)
            {
                case 3:
                    Configuring.CurrentURPA.AntiAliasing(MsaaQuality._2x);
                    break;
                default:
                    Configuring.CurrentURPA.AntiAliasing(MsaaQuality.Disabled);
                    break;
            }

            switch (currentSettings.textureQuality)
            {
                case 0:
                    QualitySettings.masterTextureLimit = 3;
                    break;
                case 1:
                    QualitySettings.masterTextureLimit = 2;
                    break;
                case 2:
                    QualitySettings.masterTextureLimit = 1;
                    break;
                case 3:
                    QualitySettings.masterTextureLimit = 0;
                    break;
            }

            var pipelineAsset = GameManager.singleton.settings.pipelineAsset;

            switch (currentSettings.shadowQuality)
            {
                case 0:
                    Configuring.CurrentURPA.MainLightShadowsCasting(false);
                    Configuring.CurrentURPA.AdditionalLightsShadowsCasting(false);
                    pipelineAsset.shadowDistance = 0;
                    break;
                case 1:
                    Configuring.CurrentURPA.MainLightShadowsCasting(true);
                    Configuring.CurrentURPA.AdditionalLightsShadowsCasting(true);
                    pipelineAsset.shadowDistance = 100;
                    break;
                case 2:
                    Configuring.CurrentURPA.MainLightShadowsCasting(true);
                    Configuring.CurrentURPA.AdditionalLightsShadowsCasting(true);
                    pipelineAsset.shadowDistance = 100;
                    break;
                case 3:
                    Configuring.CurrentURPA.MainLightShadowsCasting(true);
                    Configuring.CurrentURPA.AdditionalLightsShadowsCasting(true);
                    pipelineAsset.shadowDistance = 100;
                    break;
            }
        }
        
        public void SaveSettings()
        {
            SaveLoadJsonService.Save(SETTINGS_PATH, currentSettings, true);
        }
    }
}