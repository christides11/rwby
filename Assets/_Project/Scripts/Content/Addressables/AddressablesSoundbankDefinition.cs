using Cysharp.Threading.Tasks;
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

        public Dictionary<string, int> soundMap = new Dictionary<string, int>();

        private void OnValidate()
        {
            for(int i = 0; i < sounds.Count; i++)
            {
                sounds[i].index = i;
            }
        }

        private void OnEnable()
        {
            for(int i = 0; i < sounds.Count; i++)
            {
                soundMap.Add(sounds[i].Name, i);
            }
        }

        public override async UniTask<bool> Load()
        {
            return true;
        }

        public override bool Unload()
        {
            return true;
        }
    }
}