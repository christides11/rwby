using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class MusicLooper : MonoBehaviour
    {
        [SerializeField] private MusicLooperTrack trackPrefab;
        public List<MusicLooperTrack> tracks = new List<MusicLooperTrack>();
        [SerializeField] private AudioSource referenceAudioSource;

        private int nextSource = 0;
        private double nextEventTime;

        public SongAudio song;

        public void Play(SongAudio wantedSong)
        {
            this.song = wantedSong;
            ClearTracks();
            referenceAudioSource.volume = wantedSong.volume;
            referenceAudioSource.pitch = wantedSong.pitch;
            for (int i = 0; i < song.audioClips.Length; i++)
            {
                var trackObj = new GameObject($"Track{i+1}").AddComponent<MusicLooperTrack>();
                trackObj.transform.SetParent(transform);
                tracks.Add(trackObj);
                trackObj.Play(referenceAudioSource, song.audioClips[i], song.loopType, 
                    song.introBoundary, song.loopingBoundary);
            }
        }

        private void ClearTracks()
        {
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                Destroy(tracks[i].gameObject);
                tracks.RemoveAt(i);
            }
        }

        public void Pause()
        {
            
        }
        
        public void Stop()
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                tracks[i].Stop();
            }
        }

        private void Update()
        {
            
        }
    }
}