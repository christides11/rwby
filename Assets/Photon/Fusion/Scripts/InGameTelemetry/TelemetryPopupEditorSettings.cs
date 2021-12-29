
//using UnityEditor;
//using UnityEngine;

//#if FUSION_DEV
//[CreateAssetMenu(fileName = nameof(TelemetryPopupEditorSettings), menuName = "Fusion/Telemetry Popup Editor Settings", order = 1)]
//#endif
//public class TelemetryPopupEditorSettings : ScriptableObject
//{

//  [SerializeField]
//  bool _showWhenNotRunning = true;
//  public bool ShowWhenNotRunning {
//    get {
//      return _showWhenNotRunning;
//    }
//    set {
//      if (_showWhenNotRunning == value)
//        return;

//      Undo.RecordObject(this, "Toggle Show Popups When Not Running");
//      _showWhenNotRunning = value;
//      EditorUtility.SetDirty(this);
//      UpdateAllShowWhenNotRunning();
//      //AssetDatabase.SaveAssets();
//    }
//  }

//  [Range(0f, .1f)]
//  [SerializeField]
//  private float _globalScale = 100f;
//  public float GlobalScale {
//    get { return _globalScale; }
//    set {
//      if (_globalScale == value)
//        return;
//      Debug.Log("SetGlobal scale " + value);
//      Undo.RecordObject(this, "Change Global Telemetry Popup Scale");
//      _globalScale = value;
//      EditorUtility.SetDirty(this);
//      //AssetDatabase.SaveAssets();
//      UpdateAllPopupScaling();
//    }
//  }

//  [Range(0f, 10f)]
//  [SerializeField]
//  private float _globalDistance = 2f;
//  public float GlobalDistance {
//    get { return _globalDistance; }
//    set {
//      if (value == _globalDistance)
//        return;

//      Undo.RecordObject(this, "Change Global Telemetry Popup Distance");
//      _globalDistance = value;
//      EditorUtility.SetDirty(this);
//      //AssetDatabase.SaveAssets();
//      UpdateAllPopupDistances();
//    }
//  }

//  //[InitializeOnLoadMethod]
//  private void UpdateAllShowWhenNotRunning() {
//    var popups = FindObjectsOfType<TelemetryPopup>(true);
//    foreach (var p in popups) {
//      p.SetChildrenEnabled(ShowWhenNotRunning);
//    }
//  }

//  private void UpdateAllPopupScaling() {
//    var popups = FindObjectsOfType<TelemetryPopup>(true);
//    foreach (var p in popups) {
//      if (p.UseGlobalScale) {
//        p.ApplyScale();
//      }
//    }
//  }

//  private void UpdateAllPopupDistances() {
//    var popups = FindObjectsOfType<TelemetryPopup>(true);
//    foreach (var p in popups) {
//      if (p.UseGlobalDistance) {
//        p.ApplyDistance();
//      }
//    }
//  }



//  static TelemetryPopupEditorSettings _instance;

//  // Standard singleton enforcement.
//  public static TelemetryPopupEditorSettings Instance {
//    get {
//      if (_instance) {
//        return _instance;
//      }
//      _instance = Resources.Load<TelemetryPopupEditorSettings>(nameof(TelemetryPopupEditorSettings));

//      return _instance;
//    }
//  }
//}

