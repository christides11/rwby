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
            cpuBehaviourSlider.OnValueChanged += OnOptionChanged;
            dummyStanceSlider.options = new[] { "Standing", "Jumping" };
            dummyStanceSlider.SetOption(cpuHandler.cpuSettings[index].stance);
            dummyStanceSlider.OnValueChanged += OnOptionChanged;
            blockSlider.options = new[] { "Doesn't Block", "Block All", "After Initial Hit", "Initial Hit Only", "Hold Block", "Random" };
            blockSlider.SetOption(cpuHandler.cpuSettings[index].block);
            blockSlider.OnValueChanged += OnOptionChanged;
            blockDirectionSlider.options = new[] { "Disabled", "Enabled" };
            blockDirectionSlider.SetOption(cpuHandler.cpuSettings[index].blockDirectionSwitch);
            blockDirectionSlider.OnValueChanged += OnOptionChanged;
        }

        private void OnOptionChanged(int value)
        {
            var trainingCPUSettingsDefinition = cpuHandler.cpuSettings[index];
            trainingCPUSettingsDefinition.behaviour = cpuBehaviourSlider.currentOption;
            trainingCPUSettingsDefinition.stance = dummyStanceSlider.currentOption;
            trainingCPUSettingsDefinition.block = blockSlider.currentOption;
            trainingCPUSettingsDefinition.blockDirectionSwitch = blockDirectionSlider.currentOption;
            cpuHandler.cpuSettings.Set(index, trainingCPUSettingsDefinition);
        }
    }
}