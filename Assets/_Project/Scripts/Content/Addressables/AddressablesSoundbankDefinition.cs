using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "SoundbankDefinition", menuName = "rwby/Content/Addressables/SoundbankDefinition")]
    public class AddressablesSoundbankDefinition : ISoundbankDefinition
    {
        public override string Name { get { return soundbankName; } }
        public override List<SoundbankSoundEntry> Sounds { get { return sounds; } }
        public override Dictionary<string, int> SoundMap { get { return soundMap; } }
        [SerializeField] private string soundbankName;
        [SerializeField] private List<SoundbankSoundEntry> sounds = new List<SoundbankSoundEntry>();

        [NonSerialized] public Dictionary<string, int> soundMap = new Dictionary<string, int>();

        private void OnValidate()
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                sounds[i].index = i;
            }
        }

        private void OnEnable()
        {
            soundMap.Clear();
            for (int i = 0; i < sounds.Count; i++)
            {
                soundMap.Add(sounds[i].Name, i);
            }
        }
    }
}