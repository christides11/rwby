using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSourcePrefab;

        public List<AudioSource> playingSounds = new List<AudioSource>();

        public void Play(ModObjectItemReference audioClip, float volume, float pitch, Vector3 position)
        {
            var guidRef = new ModContentGUIDReference()
            {
                contentGUID = audioClip.contentReference.contentGUID,
                contentType = (int)ContentType.Soundbank,
                modGUID = audioClip.contentReference.modGUID
            };
            var rawRef = ContentManager.singleton.ConvertModContentGUIDReference(guidRef);

            var soundbank = ContentManager.singleton.GetContentDefinition(rawRef);
            if (soundbank == null) return;
            Play(((ISoundbankDefinition)soundbank).GetEffect(audioClip.item).clip, volume, pitch, position);
        }
        
        public void Play(AudioClip audioClip, float volume, float pitch, Vector3 position)
        {
            var aSource = GameObject.Instantiate(audioSourcePrefab, position, Quaternion.identity);
            aSource.volume = volume;
            aSource.pitch = pitch;
            aSource.clip = audioClip;
            aSource.Play();
        }
    }
}