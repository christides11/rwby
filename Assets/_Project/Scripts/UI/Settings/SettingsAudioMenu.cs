using System.Collections;
using System.Collections.Generic;
using Rewired;
using rwby.ui.mainmenu;
using UnityEngine;

namespace rwby.ui
{
    public class SettingsAudioMenu : MenuBase
    {
        public ButtonIntSlider masterVolume;
        public ButtonIntSlider soundEffectVolume;
        public ButtonIntSlider voiceVolume;
        public ButtonIntSlider ambienceVolume;
        public ButtonIntSlider musicVolume;
        public OptionSlider speakerConfig;

        private SettingsDataType modifiedSettings;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            modifiedSettings = GameManager.singleton.settingsManager.Settings;
            gameObject.SetActive(true);
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
            if (ReInput.players.SystemPlayer.GetButtonDown(Action.Apply))
            {
                if (!GatherSettings()) return;
                GameManager.singleton.settingsManager.SetSettings(modifiedSettings);
                GameManager.singleton.settingsManager.ApplyAudioSettings();
                GameManager.singleton.settingsManager.SaveSettings();
                modifiedSettings = GameManager.singleton.settingsManager.Settings;
            }
        }

        private bool GatherSettings()
        {
            modifiedSettings.masterVolume = (int)masterVolume.slider.value;
            modifiedSettings.soundEffectVolume = (int)soundEffectVolume.slider.value;
            modifiedSettings.voiceVolume = (int)voiceVolume.slider.value;
            modifiedSettings.ambienceVolume = (int)ambienceVolume.slider.value;
            modifiedSettings.musicVolume = (int)musicVolume.slider.value;
            modifiedSettings.speakerConfiguration = speakerConfig.currentOption;
            return true;
        }

        private void SetupOptions()
        {
            masterVolume.slider.maxValue = 100;
            masterVolume.slider.minValue = 0;
            masterVolume.slider.value = modifiedSettings.masterVolume;
            soundEffectVolume.slider.maxValue = 100;
            soundEffectVolume.slider.minValue = 0;
            soundEffectVolume.slider.value = modifiedSettings.soundEffectVolume;
            voiceVolume.slider.maxValue = 100;
            voiceVolume.slider.minValue = 0;
            voiceVolume.slider.value = modifiedSettings.voiceVolume;
            ambienceVolume.slider.maxValue = 100;
            ambienceVolume.slider.minValue = 0;
            ambienceVolume.slider.value = modifiedSettings.ambienceVolume;
            musicVolume.slider.maxValue = 100;
            musicVolume.slider.minValue = 0;
            musicVolume.slider.value = modifiedSettings.musicVolume;
            speakerConfig.options = new string[] { "Mono", "Stereo", "Quad", "Surrond", "5.1", "7.1", "Prologic" };
            speakerConfig.SetOption(modifiedSettings.speakerConfiguration);
        }
    }
}