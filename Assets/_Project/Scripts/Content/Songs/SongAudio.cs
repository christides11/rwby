using NaughtyAttributes;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "SongAudio", menuName = "rwby/SongAudio")]
    public class SongAudio : ScriptableObject
    {
        public AudioClip[] audioClips;
        [Range(0.0f, 1.0f)]public float volume = 1.0f;
        [Range(-3.0f, 3.0f)]public float pitch = 1.0f;

        public SongLoopType loopType = SongLoopType.IntroLoop;
        [ShowIf("loopType", SongLoopType.IntroLoop)] public double introBoundary;
        [ShowIf("loopType", SongLoopType.IntroLoop)] public double loopingBoundary;
    }
}