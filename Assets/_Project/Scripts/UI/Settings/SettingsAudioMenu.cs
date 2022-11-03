using System.Collections;
using System.Collections.Generic;
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
        public OptionSlider speakerConfig;

        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            SetupOptions();
        }

        private void SetupOptions()
        {
            masterVolume.slider.maxValue = 100;
            masterVolume.slider.minValue = 0;
            masterVolume.slider.value = 100;
            soundEffectVolume.slider.maxValue = 100;
            soundEffectVolume.slider.minValue = 0;
            soundEffectVolume.slider.value = 100;
            voiceVolume.slider.maxValue = 100;
            voiceVolume.slider.minValue = 0;
            voiceVolume.slider.value = 100;
            ambienceVolume.slider.maxValue = 100;
            ambienceVolume.slider.minValue = 0;
            ambienceVolume.slider.value = 100;
            speakerConfig.options = new string[] { "Mono", "Stereo", "Quad", "Surrond", "5.1", "7.1", "Prologic" };
            speakerConfig.SetOption(1);
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
    }
}