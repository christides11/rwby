#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion.Assistants {
  public static class TelemetryPrefabs {

    //private static TelemetryDevelopmentCanvas _devPopupCanvas;

    //public static TelemetryDevelopmentCanvas DevelopmentPopupCanvas {
    //  get {
    //    if (_devPopupCanvas != null)
    //      return _devPopupCanvas;

    //    var found = Object.FindObjectsOfType<TelemetryDevelopmentCanvas>(true);
    //    for (int i = 0;  i < found.Length; ++i){
    //      var devcanvas = found[i];
    //      if (i == found.Length - 1) {
    //        _devPopupCanvas = devcanvas;
    //        return devcanvas;
    //      } else {
    //        // Destroy extra found dev canvases.
    //        Object.Destroy(devcanvas);
    //      }
    //    }

    //    _devPopupCanvas = (PrefabUtility.InstantiatePrefab(TelemetryCanvas) as GameObject).AddComponent<TelemetryDevelopmentCanvas>();
    //    return _devPopupCanvas;
    //  }
    //}


    // Default Element
    private const string _defaultGuid = "b8ad69dae7d958645960cdee599803bb";
    private static GameObject _default;
    public static GameObject Default { get { return _defaultGuid.GetAssetFromGuid(ref _default); } }

    // Default complete popup prefab
    private const string _telemetryPopupGuid = "c3854702886c370448adaa8a2e2e47a5";
    private static GameObject _telemetryPopup;
    public static GameObject TelemetryPopup { get { return _telemetryPopupGuid.GetAssetFromGuid(ref _telemetryPopup); } }

    // Default telemetry canvas group
    private const string _telemetryCanvasGuid = "05ad9a61e553ee349a615ea24af6dce9";
    private static GameObject _telemetryCanvas;
    public static GameObject TelemetryCanvas { get { return _telemetryCanvasGuid.GetAssetFromGuid(ref _telemetryCanvas); } }


    private static T GetAssetFromGuid<T>(this string Guid, ref T backing) where T : UnityEngine.Object {
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