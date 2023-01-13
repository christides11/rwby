using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class MusicManager : MonoBehaviour
    {
        public MusicLooper musicLooperPrefab;

        public List<MusicLooper> currentlyPlaying = new List<MusicLooper>();

        public List<MusicLooper> currentlyFading = new List<MusicLooper>();
        
        public void Play(SongAudio song, float volume = 1.0f)
        {
            var musicLooper = GameObject.Instantiate(musicLooperPrefab, transform, false);
            currentlyPlaying.Add(musicLooper);
            musicLooper.Play(song, volume);
        }

        public void FadeAll(float timeToFade = 0.5f)
        {
            for (int i = currentlyPlaying.Count-1; i >= 0; i--)
            {
                Fade(i, timeToFade);
            }
        }
        
        public void Fade(int index, float timeToFade = 0.5f)
        {
            _ = currentlyPlaying[index].FadeOut(timeToFade);
            currentlyFading.Add(currentlyPlaying[index]);
            currentlyPlaying.RemoveAt(index);
        }

        public void StopCurrentlyPlaying(bool destroyPlayers = false)
        {
            for (int i = currentlyPlaying.Count-1; i >= 0; i--)
            {
                currentlyPlaying[i].Stop();
                if (destroyPlayers)
                {
                    Destroy(currentlyPlaying[i].gameObject);
                    currentlyPlaying.RemoveAt(i);
                }
            }
        }

        public void StopCurrentlyFading(bool destroyPlayers = false)
        {
            for (int i = currentlyFading.Count-1; i >= 0; i--)
            {
                currentlyPlaying[i].Stop();
                if (destroyPlayers)
                {
                    Destroy(currentlyPlaying[i].gameObject);
                    currentlyPlaying.RemoveAt(i);
                }
            }
        }
    }
}