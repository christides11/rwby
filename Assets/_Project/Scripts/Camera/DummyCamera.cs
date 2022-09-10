using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class DummyCamera : SimulationBehaviour
    {
        public Camera camera;
        public CinemachineBrain brain;
        public CamHandleCutout cutoutHandler;

        public virtual void Initialize()
        {
            Runner.AddSimulationBehaviour(cutoutHandler, null);
        }
    }
}