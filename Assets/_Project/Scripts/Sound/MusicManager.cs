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
        
        public void Play(SongAudio song)
        {
            var musicLooper = GameObject.Instantiate(musicLooperPrefab, transform, false);
            currentlyPlaying.Add(musicLooper);
            musicLooper.Play(song);
        }

        public void FadeAll()
        {
            for (int i = currentlyPlaying.Count-1; i >= 0; i--)
            {
                Fade(i);
            }
        }
        
        public void Fade(int index)
        {
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