#if UNITY_EDITOR

using UnityEditor;

namespace FusionPrototypingInternal {
  internal static class FusionPrototypingAssetUtils {
    public static T GetAsset<T>(this string Guid, ref T backing) where T : UnityEngine.Object {
      if (backing != null)
        return backing;

      var path = AssetDatabase.GUIDToAssetPath(Guid);
      if (path != null && path != "")
        backing = AssetDatabase.LoadAssetAtPath<T>(path);
      return backing;
    }
  }
}

#endif
