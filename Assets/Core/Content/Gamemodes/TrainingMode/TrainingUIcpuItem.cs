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
        public OptionSlider cpuStateSlider;

        public void Init()
        {
            cpuStateSlider.options = new[] { "Standing", "Jumping" };
            cpuStateSlider.SetOption(0);
        }
    }
}