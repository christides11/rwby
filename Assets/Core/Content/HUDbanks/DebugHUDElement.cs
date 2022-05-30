using System.Collections;
using System.Collections.Generic;
using rwby;
using TMPro;
using UnityEngine;

namespace rwby
{
    public class DebugHUDElement : HUDElement
    {
        public TextMeshProUGUI simulationFrame;
        public TextMeshProUGUI position;
        public TextMeshProUGUI stateFrame;
        public TextMeshProUGUI stateName;
        public TextMeshProUGUI velocityTotal;
        public TextMeshProUGUI velocityMovement;
        public TextMeshProUGUI speed;
    }
}