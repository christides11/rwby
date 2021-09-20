using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public class SoundbankSoundEntry
    {
        [HideInInspector] public int index;
        public string id;
        public AudioClip clip;
    }
}