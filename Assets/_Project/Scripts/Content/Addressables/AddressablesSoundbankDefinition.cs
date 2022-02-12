using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "AddressablesMapDefinition", menuName = "rwby/Content/Addressables/SoundbankDefinition")]
    public class AddressablesSoundbankDefinition : ISoundbankDefinition
    {
        public override string Name { get { return soundbankName; } }
        public override List<SoundbankSoundEntry> Sounds { get { return sounds; } }
        public override Dictionary<string, int> SoundMap { get { return soundMap; } }
        [SerializeField] private string soundbankName;
        [SerializeField] private List<SoundbankSoundEntry> sounds = new List<SoundbankSoundEntry>();

        [NonSerialized] public Dictionary<string, int> soundMap = new Dictionary<string, int>();

        public override async UniTask<bool> Load()
        {
            if (soundMap.Count > 0) return true;
            for(int i = 0; i < sounds.Count; i++)
            {
                sounds[i].index = i;
                soundMap.Add(sounds[i].Name, i);
            }
            return true;
        }

        public override bool Unload()
        {
            soundMap.Clear();
            return true;
        }
    }
}