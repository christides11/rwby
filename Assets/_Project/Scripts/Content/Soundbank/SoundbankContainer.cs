using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class SoundbankContainer : NetworkBehaviour
    {
        public Dictionary<string, int> soundbankMap = new Dictionary<string, int>();
        public List<ISoundbankDefinition> soundbanks = new List<ISoundbankDefinition>();

        [SerializeField] protected NetworkedAudioClip audioSourcePrefab;

        public void AddSoundbank(string soundbankName, ISoundbankDefinition soundbank)
        {
            soundbanks.Add(soundbank);
            soundbankMap.Add(soundbankName, soundbanks.Count-1);
        }

        public ISoundbankDefinition GetSoundbank(string soundbankName)
        {
            return soundbanks[soundbankMap[soundbankName]];
        }

        public void PlaySound(string sndbnkName, string soundName)
        {
            int soundbankIndex = soundbankMap[sndbnkName];
            int soundIndex = soundbanks[soundbankIndex].SoundMap[soundName];

            if (Object.HasStateAuthority || Object.HasInputAuthority)
            {
                var key = new NetworkObjectPredictionKey { Byte0 = (byte)Runner.Simulation.Tick, Byte1 = (byte)Object.InputAuthority.PlayerId, Byte2 = (byte)soundbankIndex, Byte3 = (byte)soundIndex };
                NetworkedAudioClip no = Runner.Spawn(audioSourcePrefab, transform.position, Quaternion.identity, null, null, key);
                no.PlaySound(this, soundbankIndex, soundIndex);
            }else if (Object.IsProxy)
            {
                if (Runner.IsResimulation == false)
                {
                    Debug.Log("Proxy play sound.");
                }
            }
        }
    }
}