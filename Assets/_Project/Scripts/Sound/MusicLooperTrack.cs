using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class MusicLooperTrack : MonoBehaviour
    {
        private AudioSource[] audioSources = null;
        private int nextSource = 0;
        private double nextEventTime = -1;
        
        private double introBoundary;
        private double loopingBoundary;

        public void Play(AudioSource referenceAudioSource, AudioClip audioClip, SongLoopType loopType, 
            double introBoundary, double loopBoundary)
        {
            SetupAudioSources(referenceAudioSource);
            this.introBoundary = introBoundary;
            this.loopingBoundary = loopBoundary;
            
            audioSources[nextSource].clip = audioClip;
            audioSources[nextSource].PlayScheduled(AudioSettings.dspTime);
            audioSources[nextSource].SetScheduledEndTime(AudioSettings.dspTime + introBoundary);
            
            nextSource = GetNextSource(nextSource);
            
            audioSources[nextSource].clip = audioClip;
            audioSources[nextSource].PlayScheduled(AudioSettings.dspTime + introBoundary);
            audioSources[nextSource].time = (float)introBoundary;
            audioSources[nextSource].SetScheduledStartTime(AudioSettings.dspTime + introBoundary);
            audioSources[nextSource].SetScheduledEndTime(AudioSettings.dspTime + loopingBoundary);

            nextEventTime = AudioSettings.dspTime + loopingBoundary;
        }

        private void SetupAudioSources(AudioSource referenceAudioSource)
        {
            if (audioSources != null) return;
            audioSources = new AudioSource[2];
            for (int i = 0; i < audioSources.Length; i++)
            {
                audioSources[i] = gameObject.AddComponent<AudioSource>();
                audioSources[i].playOnAwake = referenceAudioSource.playOnAwake;
                audioSources[i].outputAudioMixerGroup = referenceAudioSource.outputAudioMixerGroup;
                audioSources[i].priority = referenceAudioSource.priority;
                audioSources[i].volume = referenceAudioSource.volume;
                audioSources[i].pitch = referenceAudioSource.pitch;
                audioSources[i].panStereo = referenceAudioSource.panStereo;
                audioSources[i].spatialBlend = referenceAudioSource.spatialBlend;
                audioSources[i].reverbZoneMix = referenceAudioSource.reverbZoneMix;
                audioSources[i].dopplerLevel = referenceAudioSource.dopplerLevel;
                audioSources[i].spread = referenceAudioSource.spread;
                audioSources[i].rolloffMode = referenceAudioSource.rolloffMode;
                audioSources[i].minDistance = referenceAudioSource.minDistance;
                audioSources[i].maxDistance = referenceAudioSource.maxDistance;
                audioSources[i].SetCustomCurve(AudioSourceCurveType.Spread, referenceAudioSource.GetCustomCurve(AudioSourceCurveType.Spread));
                audioSources[i].SetCustomCurve(AudioSourceCurveType.CustomRolloff, referenceAudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
                audioSources[i].SetCustomCurve(AudioSourceCurveType.SpatialBlend, referenceAudioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
                audioSources[i].SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, referenceAudioSource.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
                audioSources[i].Stop();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                audioSources[i].Stop();
            }
        }
        
        public int GetNextSource(int val)
        {
            return val == 0 ? 1 : 0;
        }

        public void SetVolume(float volume)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                audioSources[i].volume = volume;
            }
        }
        
        private void Update()
        {
            if (nextEventTime < 0) return;
            double time = AudioSettings.dspTime;
            if (time + 1.0f > nextEventTime)
            {
                int flip = GetNextSource(nextSource);

                audioSources[flip].Stop();
                audioSources[flip].time = (float)introBoundary;
                audioSources[flip].PlayScheduled(nextEventTime);
                audioSources[flip].SetScheduledEndTime(nextEventTime + (loopingBoundary - introBoundary));

                nextEventTime = nextEventTime + (loopingBoundary - introBoundary);

                nextSource = flip;
            }
        }
    }
}