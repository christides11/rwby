namespace Fusion.Editor {

  using System.IO;
  using UnityEditor;
  using UnityEditor.AssetImporters;
  using UnityEngine;

  [ScriptedImporter(1, ExtensionWithoutDot)]
  public class NetworkProjectConfigImporter : ScriptedImporter {

    public const string ExtensionWithoutDot = "fusion";
    public const string Extension = "." + ExtensionWithoutDot;
    public const string ExpectedAssetName = nameof(NetworkProjectConfig) + Extension;

    public string PrefabAssetsContainerPath = "Assets/Photon/Fusion/User/NetworkPrefabAssetContainer.asset";

    public override void OnImportAsset(AssetImportContext ctx) {
      NetworkProjectConfig.UnloadGlobal();
      NetworkProjectConfig config = LoadConfigFromFile(ctx.assetPath);

      var root = ScriptableObject.CreateInstance<NetworkProjectConfigAsset>();
      root.Config = config;
      ctx.AddObjectToAsset("root", root);
    }

    public static NetworkProjectConfig LoadConfigFromFile(string path) {
      var config = new NetworkProjectConfig();
      try {
        var text = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(text)) {
          throw new System.ArgumentException("Empty string");
        }
        EditorJsonUtility.FromJsonOverwrite(text, config);
      } catch (System.ArgumentException ex) {
        throw new System.ArgumentException($"Failed to parse {path}: {ex.Message}");
      }
      return config;
    }

    class Postprocessor : AssetPostprocessor {
      static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

        foreach (var path in deletedAssets) {
          if (path.EndsWith(Extension)) {
            NetworkProjectConfig.UnloadGlobal();
            break;
          }
        }
        foreach (var path in movedAssets) {
          if (path.EndsWith(Extension)) {
            NetworkProjectConfig.UnloadGlobal();
            break;
          }
        }
      }
    }
  }
}
