using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [OrderAfter(typeof(FighterStateManager))]
    public class FighterSounder : NetworkBehaviour
    {
        [HideInInspector] public Dictionary<ModObjectSetContentReference, int> bankMap = new Dictionary<ModObjectSetContentReference, int>();
        [HideInInspector] public List<ISoundbankDefinition> banks = new List<ISoundbankDefinition>();

        [Networked, Capacity(10)] public FighterSoundsRoot sounds { get; set; }
        [Networked] public int soundBufferPos { get; set; } = 0;
        [Networked, Capacity(10)] public NetworkLinkedList<int> currentSoundIndex => default;

        private bool dirty;

        private FighterSoundsRoot currentSoundsRepresentation;

        public AudioSource[] soundObjects = new AudioSource[10];

        public AudioSource audioSourceGOPrefab;
        
        private void Awake()
        {
            dirty = true;
        }
        
        public void AddSFXs(SoundReference[] wantedSounds)
        {
            var temp = sounds;
            for (int i = 0; i < wantedSounds.Length; i++)
            {
                temp.sounds.Set(soundBufferPos % 10, new FighterSoundNode()
                {
                    bank = bankMap[wantedSounds[i].soundbank]+1,
                    sound = banks[bankMap[wantedSounds[i].soundbank]].SoundMap[wantedSounds[i].sound]+1,
                    createFrame = Runner.Tick,
                    parented = wantedSounds[i].parented,
                    pos = wantedSounds[i].offset,
                    volume = wantedSounds[i].volume,
                    minDist = wantedSounds[i].minDist,
                    maxDist = wantedSounds[i].maxDist
                });

                currentSoundIndex.Add(soundBufferPos);
                soundBufferPos++;
            }
            sounds = temp;
            dirty = true;
        }

        public void ClearCurrentSounds(bool removeSounds = false)
        {
            if (removeSounds)
            {
                var temp = sounds;
                for (int i = 0; i < currentSoundIndex.Count; i++)
                {
                    temp.sounds.Set((currentSoundIndex[i] % 10), new FighterSoundNode());
                }
                sounds = temp;
            }
            currentSoundIndex.Clear();
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
                    if (!soundObjects[i]) soundObjects[i] = GameObject.Instantiate(audioSourceGOPrefab);
                    if(sounds.sounds[i].parented) soundObjects[i].transform.SetParent(transform, false);    
                    soundObjects[i].clip = e;
                    soundObjects[i].transform.localPosition = sounds.sounds[i].pos;
                    soundObjects[i].volume = sounds.sounds[i].volume;
                    soundObjects[i].minDistance = sounds.sounds[i].minDist;
                    soundObjects[i].maxDistance = sounds.sounds[i].maxDist;
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
                ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference(bank.modGUID, (int)ContentType.Soundbank, bank.contentGUID))));
            bankMap.Add(bank, banks.Count-1);
        }
    }
}