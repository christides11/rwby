#if UNITY_EDITOR && FUSION_USE_ADDRESSABLES

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;

namespace Fusion.Editor {

  public class NetworkPrefabSourceFactoryAddressables : INetworkPrefabSourceFactory {
    public const int DefaultOrder = 800;

    private ILookup<string, AddressableAssetEntry> _lookup = default;

    private ILookup<string, AddressableAssetEntry> Lookup {
      get {
        if (_lookup == null) {
          _lookup = CreateAddressablesLookup();
          EditorApplication.delayCall += () => _lookup = null;
        }
        return _lookup;
      }
    }


    int INetworkPrefabSourceFactory.Order => DefaultOrder;
    Type INetworkPrefabSourceFactory.SourceType => typeof(NetworkPrefabSourceAddressables);

    NetworkPrefabSource.EntryBase INetworkPrefabSourceFactory.TryCreateEntry(NetworkObject prefab) {
      if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out var guid, out long _)) {
        var addressableEntry = Lookup[guid].SingleOrDefault();
        if (addressableEntry != null) {
          if (addressableEntry.IsSubAsset) {
            throw new InvalidOperationException("Sub assets not supported");
          }

          return new NetworkPrefabSourceAddressables.Entry() {
            Address = new AssetReferenceGameObject(addressableEntry.guid)
          };
        }
      }

      return null;
    }

    bool INetworkPrefabSourceFactory.TryResolve(NetworkPrefabSource.EntryBase entry, out NetworkObject prefab) {
      if (entry is NetworkPrefabSourceAddressables.Entry addr) {
        var go = addr.Address.editorAsset;
        if (go) {
          prefab = go.GetComponent<NetworkObject>();
        } else {
          prefab = null;
        }
        return true;
      }
      prefab = null;
      return false;
    }

    private static ILookup<string, AddressableAssetEntry> CreateAddressablesLookup() {
      var assetList = new List<AddressableAssetEntry>();
      var assetsSettings = AddressableAssetSettingsDefaultObject.Settings;
      if (assetsSettings != null) {
        foreach (var settingsGroup in assetsSettings.groups) {
          if (settingsGroup.ReadOnly)
            continue;
          settingsGroup.GatherAllAssets(assetList, true, true, true);
        }
      }

      return assetList.Where(x => !string.IsNullOrEmpty(x.guid)).ToLookup(x => x.guid);
    }
  }
}

#endif