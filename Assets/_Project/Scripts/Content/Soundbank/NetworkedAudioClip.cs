using UnityEngine;
using Fusion;

namespace rwby
{
    public class NetworkedAudioClip : NetworkBehaviour, IPredictedSpawnBehaviour
    {
        public AudioSource audioSource;

        [Networked] public SoundbankContainer networkedSoundbankContainer { get; set; }
        private SoundbankContainer predictedSoundbankContainer;
        public SoundbankContainer soundbankContainer
        {
            get => Object.IsPredictedSpawn ? predictedSoundbankContainer : networkedSoundbankContainer;
            set
            {
                if (Object.IsPredictedSpawn)
                    predictedSoundbankContainer = value;
                else
                    networkedSoundbankContainer = value;
            }
        }

        [Networked] public int networkedSoundbankIndex { get; set; }
        private int predictedSoundbankIndex;
        public int soundbankIndex
        {
            get => Object.IsPredictedSpawn ? predictedSoundbankIndex : networkedSoundbankIndex;
            set
            {
                if (Object.IsPredictedSpawn)
                    predictedSoundbankIndex = value;
                else
                    networkedSoundbankIndex = value;
            }
        }

        [Networked] public int networkedSoundIndex { get; set; }
        private int predictedSoundIndex;
        public int soundIndex
        {
            get => Object.IsPredictedSpawn ? predictedSoundIndex : networkedSoundIndex;
            set
            {
                if (Object.IsPredictedSpawn)
                    predictedSoundIndex = value;
                else
                    networkedSoundIndex = value;
            }
        }

        [Networked] public int networkedSoundTick { get; set; }
        private int predictedSoundTick;
        public int soundTick
        {
            get => Object.IsPredictedSpawn ? predictedSoundTick : networkedSoundTick;
            set
            {
                if (Object.IsPredictedSpawn)
                    predictedSoundTick = value;
                else
                    networkedSoundTick = value;
            }
        }

        [Networked] public AudioPlayMode networkedPlayMode { get; set; }
        private AudioPlayMode predictedPlayMode;
        public AudioPlayMode playMode
        {
            get => Object.IsPredictedSpawn ? predictedPlayMode : networkedPlayMode;
            set
            {
                if (Object.IsPredictedSpawn)
                    predictedPlayMode = value;
                else
                    networkedPlayMode = value;
            }
        }

        public virtual void PlaySound(SoundbankContainer soundbankContainer, int soundbankIndex, int soundIndex)
        {
            this.soundbankContainer = soundbankContainer;
            this.soundbankIndex = soundbankIndex;
            this.soundIndex = soundIndex;
            playMode = AudioPlayMode.Play;
            soundTick = 0;
            if (Runner.IsResimulation == false)
            {
                audioSource.clip = soundbankContainer.soundbanks[soundbankIndex].Sounds[soundIndex].clip;
                audioSource.Play();
            }
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            audioSource.Stop();
            audioSource.clip = null;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            if (playMode != AudioPlayMode.Play)
            {
                return;
            }
            soundTick++;

            if(soundTick >= 300)
            {
                Runner.Despawn(Object);
            }
        }

        // PREDICTION //
        public virtual void PredictedSpawnSpawned()
        {
            soundTick = 0;
        }

        public virtual void PredictedSpawnUpdate()
        {

        }

        public virtual void PredictedSpawnRender()
        {

        }

        public virtual void PredictedSpawnSuccess()
        {

        }

        public virtual void PredictedSpawnFailed()
        {
            Runner.Despawn(Object, true);
        }
    }
}
