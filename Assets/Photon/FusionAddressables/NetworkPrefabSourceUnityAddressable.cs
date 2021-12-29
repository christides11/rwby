#if FUSION_USE_ADDRESSABLES

using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using Fusion.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif 

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Fusion {

  public class NetworkPrefabSourceUnityAddressable : NetworkPrefabSourceUnityBase {

    public AssetReferenceGameObject Address;

    public override string EditorSummary => $"[Address: {Address}]";

    public override void Load(in NetworkPrefabLoadContext context) {
      Debug.Assert(!Address.OperationHandle.IsValid());
      var op = Address.LoadAssetAsync();
      if (op.IsDone) {
        context.Loaded(op.Result);
      } else {
        if (context.HasFlag(NetworkPrefabLoadContext.FLAGS_PREFER_ASYNC)) {
          var c = context;
          op.Completed += (_op) => {
            c.Loaded(_op.Result);
          };
        } else {
          var result = op.WaitForCompletion();
          context.Loaded(result);
        }
      }
    }

    public override void Unload() {
      Address.ReleaseAsset();
    }
  }

#if UNITY_EDITOR
  public class NetworkPrefabAssetFactoryAddressable : INetworkPrefabSourceFactory {
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
    Type INetworkPrefabSourceFactory.SourceType => typeof(NetworkPrefabSourceUnityAddressable);

    NetworkPrefabSourceUnityBase INetworkPrefabSourceFactory.TryCreate(string assetPath) {
      var guid = AssetDatabase.AssetPathToGUID(assetPath);
      var addressableEntry = Lookup[guid].SingleOrDefault();
      if (addressableEntry != null) {
        if (addressableEntry.IsSubAsset) {
          throw new InvalidOperationException("Sub assets not supported");
        }

        var result = ScriptableObject.CreateInstance<NetworkPrefabSourceUnityAddressable>();
        result.Address = new AssetReferenceGameObject(addressableEntry.guid);
        return result;
      }
      return null;
    }

    UnityEngine.GameObject INetworkPrefabSourceFactory.EditorResolveSource(NetworkPrefabSourceUnityBase prefabAsset) { 
      return ((NetworkPrefabSourceUnityAddressable)prefabAsset).Address.editorAsset;
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
#endif
}

#endif