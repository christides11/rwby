using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Audio;

namespace rwby
{
    [OrderAfter(typeof(FighterStateManager))]
    public class FighterSounder : NetworkBehaviour
    {
        private Settings gameSettings;
        
        [HideInInspector] public Dictionary<ModObjectSetContentReference, int> bankMap = new Dictionary<ModObjectSetContentReference, int>();
        [HideInInspector] public List<ISoundbankDefinition> banks = new List<ISoundbankDefinition>();

        [Networked] public NetworkRNG rng { get; set; } = new NetworkRNG(0);
        [Networked, Capacity(10)] public FighterSoundsRoot sounds { get; set; }
        [Networked] public int soundBufferPos { get; set; } = 0;
        [Networked, Capacity(10)] public NetworkArray<NetworkBool> isCurrentSound => default;

        private bool dirty;

        private FighterSoundsRoot currentSoundsRepresentation;

        public AudioSource[] soundObjects = new AudioSource[10];

        private AudioMixerGroup sfxMixerGroup;
        
        private void Awake()
        {
            dirty = true;
            gameSettings = GameManager.singleton.settings;
            sfxMixerGroup = gameSettings.audioMixer.FindMatchingGroups("sfx")[0];
        }

        public override void Spawned()
        {
            base.Spawned();
            if(Object.HasStateAuthority) rng = new NetworkRNG(327);
        }

        public void AddSFXs(SoundReference[] wantedSounds)
        {
            foreach (var sRef in wantedSounds)
            {
                AddSFX(sRef);
            }
        }

        public void AddSFX(SoundReference wantedSound)
        {
            var tempRNG = rng;
            var temp = sounds;
            temp.sounds.Set(soundBufferPos % 10, new FighterSoundNode()
            {
                bank = bankMap[wantedSound.soundbank.reference]+1,
                sound = banks[bankMap[wantedSound.soundbank.reference]].SoundMap[wantedSound.sound]+1,
                createFrame = Runner.Tick,
                parented = wantedSound.parented,
                pos = wantedSound.offset,
                volume = wantedSound.volume,
                minDist = wantedSound.minDist,
                maxDist = wantedSound.maxDist,
                pitch = 1.0f + tempRNG.RangeInclusive(wantedSound.pitchDeviMin, wantedSound.pitchDeviMax)
            });
            isCurrentSound.Set(soundBufferPos % 10, true);
            soundBufferPos++;
            sounds = temp;
            rng = tempRNG;
            dirty = true;
        }

        public void ClearCurrentSounds(bool removeSounds = false)
        {
            if (removeSounds)
            {
                var temp = sounds;
                for (int i = 0; i < temp.sounds.Length; i++)
                {
                    if (isCurrentSound[i]) temp.sounds.Set(i, new FighterSoundNode());
                    isCurrentSound.Set(i, false);
                }
                sounds = temp;
            }
            dirty = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsResimulation)
            {
                if (Runner.IsLastTick && !AreSoundsUpToDate())
                {
                    SyncEffects();
                }
                return;
            }

            if (dirty)
            {
                SyncEffects();
                dirty = false;
            }
            
        }

        private void SyncEffects()
        {
            for (int i = 0; i < soundObjects.Length; i++)
            {
                if (sounds.sounds[i].bank == 0)
                {
                    if (soundObjects[i])
                    {
                        soundObjects[i].Stop();
                    }
                    continue;
                }
                
                if (currentSoundsRepresentation.sounds[i] != sounds.sounds[i])
                {
                    var e = GetEffect(sounds.sounds[i]);
                    if (!soundObjects[i]) soundObjects[i] = GameObject.Instantiate(gameSettings.audioSourcePrefab);
                    if(sounds.sounds[i].parented) soundObjects[i].transform.SetParent(transform, false);    
                    soundObjects[i].clip = e;
                    soundObjects[i].transform.localPosition = sounds.sounds[i].pos;
                    soundObjects[i].volume = sounds.sounds[i].volume;
                    soundObjects[i].minDistance = sounds.sounds[i].minDist;
                    soundObjects[i].maxDistance = sounds.sounds[i].maxDist;
                    soundObjects[i].outputAudioMixerGroup = sfxMixerGroup;
                    soundObjects[i].pitch = sounds.sounds[i].pitch;
                    soundObjects[i].Play();
                }

                float calcTime = Runner.DeltaTime * (Runner.Tick - sounds.sounds[i].createFrame);
                if (Mathf.Abs(soundObjects[i].time - calcTime) > Runner.DeltaTime)
                {
                    if (soundObjects[i].clip.length < soundObjects[i].time)
                    {
                        soundObjects[i].Play();
                        soundObjects[i].time = calcTime;
                    }
                }
            }
            
            currentSoundsRepresentation = sounds;
        }

        private void ClearEffects()
        {
            var temp = sounds;
            for (int i = 0; i < temp.sounds.Length; i++)
            {
                temp.sounds.Set(i, new FighterSoundNode());
            }
            sounds = temp;
        }

        private bool AreSoundsUpToDate()
        {
            for (int i = 0; i < sounds.sounds.Length; i++)
            {
                if(currentSoundsRepresentation.sounds[i] != sounds.sounds[i]) return false;
            }
            return true;
        }
        
        public AudioClip GetEffect(FighterSoundNode soundNode)
        {
            return banks[soundNode.bank - 1].Sounds[soundNode.sound - 1].clip;
        }
        
        /*
        public AudioClip GetEffect(SoundReference animation)
        {
            return banks[bankMap[animation.effectbank]].GetEffect(animation.effect).clip;
        }*/
        
        public void RegisterBank(ModObjectSetContentReference bank)
        {
            if (bankMap.ContainsKey(bank)) return;
            banks.Add(ContentManager.singleton.GetContentDefinition<ISoundbankDefinition>(
                ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference(bank.modGUID, (int)ContentType.Soundbank, bank.contentGUID))));
            bankMap.Add(bank, banks.Count-1);
        }
    }
}