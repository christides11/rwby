using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct SoundReference
    {
        public ModObjectSetContentReference soundbank;
        public string sound;
        public bool parented;
        public Vector3 offset;
        public float volume;
        public float minDist;
        public float maxDist;
        public float pitchDeviMin;
        public float pitchDeviMax;
    }
}