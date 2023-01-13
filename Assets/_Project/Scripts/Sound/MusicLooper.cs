using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        private float currentVolume;

        public void Play(SongAudio wantedSong, float volume = 1.0f)
        {
            this.song = wantedSong;
            ClearTracks();
            currentVolume = wantedSong.volume;
            referenceAudioSource.volume = wantedSong.volume * volume;
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

        public async UniTask FadeOut(float fadeTime = 0.5f)
        {
            float currentTime = 0;
            float start = currentVolume;
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                foreach (var t in tracks)
                {
                    t.SetVolume(Mathf.Lerp(start, 0.0f, currentTime / fadeTime));
                }

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
        }
    }
}