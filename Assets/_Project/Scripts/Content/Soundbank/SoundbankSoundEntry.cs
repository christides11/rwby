using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public class SoundbankSoundEntry
    {
        [FormerlySerializedAs("name")] public string Name;
        [HideInInspector] public int index;
        public AudioClip clip;
    }
}