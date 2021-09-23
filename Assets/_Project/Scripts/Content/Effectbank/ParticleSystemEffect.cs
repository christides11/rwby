using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class ParticleSystemEffect : BaseEffect
    {
        [SerializeField] protected ParticleSystem[] particleSystems;

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

        [Networked] public int networkedStartTick { get; set; }
        private int predictedStartTick;
        public int startTick
        {
            get => Object.IsPredictedSpawn ? predictedStartTick : networkedStartTick;
            set
            {
                if (Object.IsPredictedSpawn)
                    predictedStartTick = value;
                else
                    networkedStartTick = value;
            }
        }

        public override void PlayEffect(bool restart = true)
        {
            if (restart)
            {
                startTick = Runner.Simulation.Tick;
            }
            playMode = AudioPlayMode.Play;
        }

        public override void PauseEffect()
        {
            playMode = AudioPlayMode.Pause;
        }

        public override void StopEffect(ParticleSystemStopBehavior stopBehavior)
        {
            for(int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Stop(true, stopBehavior);
            }
            playMode = AudioPlayMode.Stop;
        }

        public override void FixedUpdateNetwork()
        {
            if (playMode != AudioPlayMode.Play) return;

            if (Runner.IsServer)
            {
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    particleSystems[i].Simulate(Runner.DeltaTime, true, false);
                }
            } else if (Runner.IsClient && Runner.IsLastTick)
            {
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    particleSystems[i].Simulate( (Runner.Simulation.Tick - startTick) * Runner.DeltaTime , true, true);
                }
            }
        }
    }
}