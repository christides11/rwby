//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Fusion.Assistants;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

////[ExecuteInEditMode]
//public class TelemetryDevelopmentCanvas : MonoBehaviour {

//  public void Awake() {
//    if (Application.isPlaying)
//      Destroy(this);
//  }

//  private static TelemetryDevelopmentCanvas _instance;
//  public static TelemetryDevelopmentCanvas Instance {

//    get {
//      if (_instance != null) {
//        return _instance;
//      }

//      var found = Object.FindObjectsOfType<TelemetryDevelopmentCanvas>(true);
//      for (int i = 0; i < found.Length; ++i) {
//        var devcanvas = found[i];
//        if (i == found.Length - 1) {
//          _instance = devcanvas;
//          return devcanvas;
//        } else {
//          // Destroy extra found dev canvases.
//          Object.Destroy(devcanvas);
//        }
//      }
//      _instance = (PrefabUtility.InstantiatePrefab(TelemetryPrefabs.TelemetryCanvas) as GameObject).AddComponent<TelemetryDevelopmentCanvas>();
//      return _instance;
//    }
//  }

//  private static RectTransform _rectTransform;
//  public static RectTransform RectTransform {
//    get {
//      if (_rectTransform)
//        return _rectTransform;

//      _rectTransform = Instance.GetComponent<RectTransform>();
//      return _rectTransform;
//    }
//  }

//  private static LineRenderer _lineRenderer;
//  public static LineRenderer LineRenderer {
//    get {
//      if (_lineRenderer)
//        return _lineRenderer;

//      _lineRenderer = Instance.GetComponent<LineRenderer>();
//      return _lineRenderer;
//    }
//  }

//#if UNITY_EDITOR

//  [UnityEditor.Callbacks.DidReloadScripts]
//  public static void OnCompile() {
//    Selection.selectionChanged -= SelectionChanged;
//    Selection.selectionChanged += SelectionChanged;
//  }

//  //[InitializeOnLoadMethod]
//  //public static void OnLoad() {
//  //  Selection.selectionChanged -= SelectionChanged;
//  //  Selection.selectionChanged += SelectionChanged;
//  //}

//  ////[InitializeOnLoadMethod]
//  //void OnEnable() {
//  //  Debug.LogWarning("Register");
//  //  Selection.selectionChanged -= SelectionChanged;
//  //  Selection.selectionChanged += SelectionChanged;
//  //}

//  //void OnDisable() {
//  //  Selection.selectionChanged -= SelectionChanged;
//  //}

//  public static void SelectionChanged() {
//    var selected = Selection.activeGameObject;
//    if (selected != null && selected.TryGetComponent<TelemetryPopup>(out var found)) {
//    } else {
//      Instance.gameObject.SetActive(false);
//    }
//  }


//#endif

//}
