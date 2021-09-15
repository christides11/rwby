using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class NetworkedAudioClip : NetworkBehaviour, IPredictedSpawnBehaviour
    {
        public AudioSource audioSource;

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
        }

        public void PredictedSpawnFailed()
        {
            Debug.Log("Failed lol");
        }

        public void PredictedSpawnRender()
        {

        }

        public void PredictedSpawnSpawned()
        {

        }

        public void PredictedSpawnSuccess()
        {
            Debug.Log("Woo yea baby that's what I'm-");
        }

        public void PredictedSpawnUpdate()
        {

        }
    }
}
