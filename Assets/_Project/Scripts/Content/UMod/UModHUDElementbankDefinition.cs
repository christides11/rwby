using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "HUDElementbank", menuName = "rwby/Content/UMod/HUDElementbankDefinition")]
    public class UModHUDElementbankDefinition : IHUDElementbankDefinition
    {
        public override string Name { get { return bankName; } }
        public override string Description { get { return description; } }
        public override List<HUDElementbankEntry> HUDElements => hudElements;
        public override Dictionary<string, int> HUDElementMap => bankMap;

        [SerializeField] private string bankName;
        [SerializeField] [TextArea] private string description;

        public List<HUDElementbankEntry> hudElements = new List<HUDElementbankEntry>();
        [NonSerialized] public Dictionary<string, int> bankMap = new Dictionary<string, int>();
        
        public override async UniTask<bool> Load()
        {
            if (bankMap.Count > 0) return true;
            for(int i = 0; i < hudElements.Count; i++)
            {
                bankMap.Add(hudElements[i].name, i);
            }
            return true;
        }
        
        public override GameObject GetHUDElement(string name)
        {
            return hudElements[bankMap[name]].element.gameObject;
        }

        public override bool Unload()
        {
            bankMap.Clear();
            return true;
        }
    }
}