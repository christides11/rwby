using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "AssetLookupTable", menuName = "rwby/Content/UMod/AssetLookupTable")]
    public class UModAssetLookupTable : ScriptableObject
    {
        public string this[int index]
        {
            get
            {
                return table[index];
            }
        }
        
        public SerializableDictionary<int, string> table = new SerializableDictionary<int, string>();
    }
}