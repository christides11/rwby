#if FUSION_USE_ADDRESSABLES

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Fusion
{

    public class NetworkPrefabSourceAddressables : NetworkPrefabSource<NetworkPrefabSourceAddressables.Entry>
    {

        public NetworkPrefabSourceAddressables() : base("Addressables")
        {
        }

        [Serializable]
        public class Entry : EntryBase, IEquatable<Entry>
        {
            public AssetReferenceGameObject Address;

            public bool Equals(Entry other)
            {
                return other.Address == Address;
            }

            public override NetworkObject LoadPrefab()
            {
                if (!Address.IsValid())
                {
                    Address.LoadAssetAsync();
                }
                if (Address.IsDone)
                {
                    var prefab = (GameObject)Address.Asset;
                    if (prefab)
                    {
                        return prefab.GetComponent<NetworkObject>();
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }

            public override string ToString()
            {
                return $"[Address: {Address}]";
            }
        }
    }
}

#endif