using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    public class MusicLooper : MonoBehaviour
    {
        public AudioSource[] audioSources;
        public AudioClip song;

        public double introBoundary;
        public double loopPointBoundary;
        
        private int nextSource = 0;
        private double nextEventTime;

        private void Start()
        {
            Play();
        }

        public void Play()
        {
            audioSources[nextSource].clip = song;
            audioSources[nextSource].PlayScheduled(AudioSettings.dspTime);
            audioSources[nextSource].SetScheduledEndTime(AudioSettings.dspTime + introBoundary);
            nextSource = GetNextSource(nextSource);
            
            audioSources[nextSource].clip = song;
            audioSources[nextSource].PlayScheduled(AudioSettings.dspTime + introBoundary);
            audioSources[nextSource].time = (float)introBoundary;
            audioSources[nextSource].SetScheduledStartTime(AudioSettings.dspTime + introBoundary);
            audioSources[nextSource].SetScheduledEndTime(AudioSettings.dspTime + loopPointBoundary);

            nextEventTime = AudioSettings.dspTime + loopPointBoundary;
        }

        public void Pause()
        {
            
        }
        
        public void Stop()
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                audioSources[i].Stop();
            }
        }

        private void Update()
        {
            double time = AudioSettings.dspTime;
            if (time + 1.0f > nextEventTime)
            {
                int flip = GetNextSource(nextSource);

                audioSources[flip].time = (float)introBoundary;
                audioSources[flip].PlayScheduled(nextEventTime);
                
                nextEventTime = nextEventTime + (loopPointBoundary - introBoundary);

                nextSource = flip;
            }
        }

        public int GetNextSource(int val)
        {
            return val == 0 ? 1 : 0;
        }
    }
}