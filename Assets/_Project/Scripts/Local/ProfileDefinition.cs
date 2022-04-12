using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct ProfileDefinition
    {
        [SerializeField] public bool undeletable;
        [SerializeField] public byte version;
        [SerializeField] public string profileName;
        [SerializeField] public List<string> behaviours;
        [SerializeField] public List<string> joystickMaps;

        public string GetInputBehaviour(int behaviourID)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours.Contains($"\"id\":{behaviourID}")) return behaviours[i];
            }
            return string.Empty;
        }
    }
}