using rwby.ui;
using UnityEngine;

namespace rwby.core.training
{
    public class TrainingUIcpuItem : MonoBehaviour
    {
        public PlayerPointerEventTrigger removeButton;
        public PlayerPointerEventTrigger characterButton;

        [Header("Basics")] 
        public OptionSlider status;
        public OptionSlider guard;
        public OptionSlider guardDirection;
        public OptionSlider techThrow;
        public OptionSlider counterHit;
        public OptionSlider shield;

        [Header("Recovery")]
        public OptionSlider staggerRecovery;
        public OptionSlider aerialRecovery;
        public OptionSlider groundRecovery;
        
        [Header("Guages")]
        public OptionSlider lifeGuage;
        //
        public OptionSlider auraGuage;
        // 

        private TrainingCPUHandler cpuHandler;
        private int index = 0;
        
        public void Init(TrainingCPUHandler cpuHandler, int index)
        {
            this.cpuHandler = cpuHandler;
            this.index = index;
            // BASICS
            status.options = new[] { "Standing", "Jumping", "CPU", "Controller" };
            status.SetOption(cpuHandler.cpuSettings[index].status);
            status.OnValueChanged += OnOptionChanged;
            guard.options = new[] { "No Guard", "Guard All", "Guard After First", "Guard Only First", "Random", "Hold Guard" };
            guard.SetOption(cpuHandler.cpuSettings[index].guard);
            guard.OnValueChanged += OnOptionChanged;
            guardDirection.options = new[] { "Disabled", "Enabled" };
            guardDirection.SetOption(cpuHandler.cpuSettings[index].guardDirection);
            guardDirection.OnValueChanged += OnOptionChanged;
            techThrow.options = new[] { "Disabled", "Enabled" };
            techThrow.SetOption(cpuHandler.cpuSettings[index].techThrow);
            techThrow.OnValueChanged += OnOptionChanged;
            counterHit.options = new[] { "Default", "All Hits", "Random" };
            counterHit.SetOption(cpuHandler.cpuSettings[index].counterHit);
            counterHit.OnValueChanged += OnOptionChanged;
            shield.options = new[] { "Off", "On" };
            shield.SetOption(cpuHandler.cpuSettings[index].shield);
            shield.OnValueChanged += OnOptionChanged;
        }

        private void OnOptionChanged(int value)
        {
            var trainingCPUSettingsDefinition = cpuHandler.cpuSettings[index];
            trainingCPUSettingsDefinition.status = status.currentOption;
            trainingCPUSettingsDefinition.guard = guard.currentOption;
            trainingCPUSettingsDefinition.guardDirection = guardDirection.currentOption;
            trainingCPUSettingsDefinition.techThrow = techThrow.currentOption;
            trainingCPUSettingsDefinition.counterHit = counterHit.currentOption;
            trainingCPUSettingsDefinition.shield = shield.currentOption;
            cpuHandler.cpuSettings.Set(index, trainingCPUSettingsDefinition);
        }
    }
}