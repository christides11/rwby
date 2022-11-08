using System.Collections.Generic;
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
        
        [Header("Counter-Attack")]
        public OptionSlider afterBlock;
        public OptionSlider afterHit;
        public OptionSlider afterRecovery;
        public OptionSlider afterThrowBreak;
        
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
            /*
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
            // RECOVERY
            staggerRecovery.options = new[] { "Disabled", "Enabled" };
            staggerRecovery.SetOption(cpuHandler.cpuSettings[index].staggerRecovery);
            staggerRecovery.OnValueChanged += OnOptionChanged;
            aerialRecovery.options = new[] { "Off", "In Place", "Forwards", "Backwards", "Left", "Right", "Random" };
            aerialRecovery.SetOption(cpuHandler.cpuSettings[index].airRecovery);
            aerialRecovery.OnValueChanged += OnOptionChanged;
            groundRecovery.options = new[] { "Off", "In Place", "Forwards", "Backwards", "Left", "Right", "Random" };
            groundRecovery.SetOption(cpuHandler.cpuSettings[index].groundRecovery);
            groundRecovery.OnValueChanged += OnOptionChanged;
            // COUNTER-ATTACK
            List<string> aOpts = new List<string>();
            aOpts.Add("None");
            foreach (var a in cpuHandler.cpuAtkOptions[index])
            {
                aOpts.Add(a.nickname);
            }
            
            afterBlock.options = aOpts.ToArray();
            afterBlock.SetOption(cpuHandler.cpuSettings[index].afterBlock);
            afterBlock.OnValueChanged += OnOptionChanged;
            afterHit.options = aOpts.ToArray();
            afterHit.SetOption(cpuHandler.cpuSettings[index].afterHit);
            afterHit.OnValueChanged += OnOptionChanged;*/
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
            trainingCPUSettingsDefinition.staggerRecovery = staggerRecovery.currentOption;
            trainingCPUSettingsDefinition.staggerRecovery = staggerRecovery.currentOption;
            // COUNTER-ATTACK
            trainingCPUSettingsDefinition.afterBlock = afterBlock.currentOption;
            trainingCPUSettingsDefinition.afterHit = afterHit.currentOption;
            cpuHandler.cpuSettings.Set(index, trainingCPUSettingsDefinition);
        }
    }
}