using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using UnityEngine;

namespace rwby.core.training
{
    public class TrainingUIcpuItem : MonoBehaviour
    {
        public PlayerPointerEventTrigger removeButton;
        public PlayerPointerEventTrigger characterButton;
        public OptionSlider cpuBehaviourSlider;
        public OptionSlider dummyStanceSlider;
        public OptionSlider blockSlider;
        public OptionSlider blockDirectionSlider;

        private TrainingCPUHandler cpuHandler;
        private int index = 0;
        
        public void Init(TrainingCPUHandler cpuHandler, int index)
        {
            this.cpuHandler = cpuHandler;
            this.index = index;
            cpuBehaviourSlider.options = new[] { "Dummy", "CPU" };
            cpuBehaviourSlider.SetOption(cpuHandler.cpuSettings[index].behaviour);
            cpuBehaviourSlider.OnValueChanged -= OnBehaviourChanged;
            cpuBehaviourSlider.OnValueChanged += OnBehaviourChanged;
            dummyStanceSlider.options = new[] { "Standing", "Jumping" };
            dummyStanceSlider.SetOption(cpuHandler.cpuSettings[index].stance);
            dummyStanceSlider.OnValueChanged -= OnStanceChanged;
            dummyStanceSlider.OnValueChanged += OnStanceChanged;
            blockSlider.options = new[] { "Doesn't Block", "Block All", "After Initial Hit", "Initial Hit Only", "Hold Block", "Random" };
            blockSlider.SetOption(cpuHandler.cpuSettings[index].block);
            blockDirectionSlider.options = new[] { "Disabled", "Enabled" };
            blockDirectionSlider.SetOption(cpuHandler.cpuSettings[index].blockDirectionSwitch);
        }

        private void OnStanceChanged(int value)
        {
            var trainingCPUSettingsDefinition = cpuHandler.cpuSettings[index];
            trainingCPUSettingsDefinition.stance = value;
            cpuHandler.cpuSettings.Set(index, trainingCPUSettingsDefinition);
        }

        private void OnBehaviourChanged(int value)
        {
            var trainingCPUSettingsDefinition = cpuHandler.cpuSettings[index];
            trainingCPUSettingsDefinition.behaviour = value;
            cpuHandler.cpuSettings.Set(index, trainingCPUSettingsDefinition);
        }
    }
}