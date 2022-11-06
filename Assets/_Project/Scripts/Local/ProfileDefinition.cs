using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ProfileDefinition
    {
        [System.Serializable]
        public struct CameraVariables
        {
            public float deadzoneHoz;
            public float deadzoneVert;
            public float speedHoz;
            public float speedVert;
            public float speedLockOnHoz;
            public float speedLockOnVert;
        }

        [SerializeField] public bool undeletable;
        [SerializeField] public byte version;
        [SerializeField] public string profileName;
        [SerializeField] public List<string> behaviours;
        [SerializeField] public List<string> joystickMaps;
        [SerializeField] public List<string> keyboardMaps;
        [SerializeField] public List<string> mouseMaps;
        [SerializeField] public CameraVariables controllerCam;
        [SerializeField] public CameraVariables keyboardCam;

        public string GetInputBehaviour(int behaviourID)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours.Contains($"\"id\":{behaviourID}")) return behaviours[i];
            }
            return string.Empty;
        }

        public bool IsValid()
        {
            if (String.IsNullOrEmpty(profileName)) return false;
            return true;
        }
    }
}