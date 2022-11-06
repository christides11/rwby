using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GraphicsConfigurator.API.URP;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;
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

        public void ApplyAllSettings()
        {
            ApplyAudioSettings();
            ApplyVideoSettings();
        }
        
        public void ApplyAudioSettings()
        {
            var audioMixer = GameManager.singleton.settings.audioMixer;

            float masterVolume = Remap(currentSettings.masterVolume, 0.0f, 100.0f, 0.001f, 1.0f);
            audioMixer.SetFloat("masterVol", Mathf.Log(masterVolume) * 20);
            float sfxVolume = Remap(currentSettings.soundEffectVolume, 0.0f, 100.0f, 0.001f, 1.0f);
            audioMixer.SetFloat("sfxVol", Mathf.Log(sfxVolume) * 20);
            float voiceVolume = Remap(currentSettings.voiceVolume, 0.0f, 100.0f, 0.001f, 1.0f);
            audioMixer.SetFloat("voiceVol", Mathf.Log(voiceVolume) * 20);
            float ambienceVolume = Remap(currentSettings.ambienceVolume, 0.0f, 100.0f, 0.001f, 1.0f);
            audioMixer.SetFloat("ambienceVol", Mathf.Log(ambienceVolume) * 20);
            float musicVolume = Remap(currentSettings.musicVolume, 0.0f, 100.0f, 0.001f, 1.0f);
            audioMixer.SetFloat("musicVol", Mathf.Log(musicVolume) * 20);

            if(AudioSettings.speakerMode != (AudioSpeakerMode)(currentSettings.speakerConfiguration+1))
                AudioSettings.speakerMode = (AudioSpeakerMode)(currentSettings.speakerConfiguration+1);
        }
        
        private float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
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
            var rendererAsset = GameManager.singleton.settings.rendererData;
            var ssaoFeature = rendererAsset.rendererFeatures
                .FirstOrDefault(f => f.name == "ScreenSpaceAmbientOcclusion");

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
                    Configuring.CurrentURPA.MainLightShadowResolution(ShadowResolution._512);
                    Configuring.CurrentURPA.AddtionalLightShadowResolution(ShadowResolution._512);
                    pipelineAsset.shadowDistance = 100;
                    break;
                case 2:
                    Configuring.CurrentURPA.MainLightShadowsCasting(true);
                    Configuring.CurrentURPA.AdditionalLightsShadowsCasting(true);
                    Configuring.CurrentURPA.MainLightShadowResolution(ShadowResolution._2048);
                    Configuring.CurrentURPA.AddtionalLightShadowResolution(ShadowResolution._1024);
                    pipelineAsset.shadowDistance = 100;
                    break;
                case 3:
                    Configuring.CurrentURPA.MainLightShadowsCasting(true);
                    Configuring.CurrentURPA.AdditionalLightsShadowsCasting(true);
                    Configuring.CurrentURPA.MainLightShadowResolution(ShadowResolution._4096);
                    Configuring.CurrentURPA.AddtionalLightShadowResolution(ShadowResolution._2048);
                    pipelineAsset.shadowDistance = 100;
                    break;
            }

            switch (currentSettings.ambientOcclusion)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }
        
        public void SaveSettings()
        {
            SaveLoadJsonService.Save(SETTINGS_PATH, currentSettings, true);
        }
    }
}