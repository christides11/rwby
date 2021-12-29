
//using UnityEngine;
//using Fusion;

//#if UNITY_EDITOR
//using UnityEditor;
//using Fusion.Editor;
//using Fusion.Assistants;
//#endif

////[ExecuteInEditMode]
//public class TelemetryPopup : Fusion.Behaviour {

//  //public bool UseGlobalScale;
//  //public bool UseGlobalDistance;

//  [SerializeField] FusionGraph[] _graphs;

//  [SerializeField]
//  [Range(0, 100f)]
//  private float _titleHeight = 60f;

//  [SerializeField]
//  //[HideInInspector]
//  [Range(0f, 100f)]
//  private float _scale = 5f;

//  [SerializeField]
//  //[HideInInspector]
//  [Range(10f, 2048f)]
//  private float _resolution = 1024;

//  [SerializeField]
//  [NormalizedRect]
//  Rect RegionRect = Rect.MinMaxRect(0.6f, 0.5f, 1f, 1f);

//  [SerializeField]
//  //[HideInInspector]
//  [Range(-10f, 10f)]
//  private float _distanceOffset = 0f;

//  private bool _layoutIsDirty;

//#if UNITY_EDITOR

//  private void OnValidate() {
//    _layoutIsDirty = true;
//    Update();
//  }

//  [MenuItem("GameObject/Fusion/Telemetry/Add Telemetry Popup", false, FusionAssistants.PRIORITY)]
//  private static void AddTelemetryAssist() {
//    var selected = Selection.activeGameObject;
//    if (selected == null)
//      return;

//    var bounds = selected.gameObject.CollectMyBounds(BoundsTools.BoundsType.Both, out int dummy);

//    var go = (GameObject)PrefabUtility.InstantiatePrefab(TelemetryPrefabs.TelemetryPopup, selected.transform);
//    var telePopup = go.GetComponent<TelemetryPopup>();
//    //telePopup.UseGlobalDistance = false;
//    var extents = bounds.extents;
//    telePopup._distanceOffset = System.Math.Max(extents.x, (System.Math.Max(extents.y, extents.z))) * 1.25f;
//  }

//#endif

//  //[SerializeField] GameObject    _canvas;
//  [SerializeField] RectTransform _canvasRT;
//  [SerializeField] LineRenderer  _line;
//  [SerializeField] RectTransform _rightPanelRT;
//  [SerializeField] RectTransform _titlePanelRT;
//  [SerializeField] RectTransform _layoutPanelRT;

//  float _previousScale;
//  float _previousResol;

//  // Camera find is expensive, so do it once per update for ALL implementations
//  static float   _lastCameraFindTime;
//  static Camera _currentCam;

//  Camera MainCamera {
//    set {
//      _currentCam = value;
//    }
//    get {

//      var time = Time.time;
//      // Only look for the camera once per Update.
//      if (time == _lastCameraFindTime)
//        return _currentCam;

//      _lastCameraFindTime = time;
//      var cam = Camera.main;
//      _currentCam = cam;
//      return cam;
//    }
//  }


////#if UNITY_EDITOR

//  [BehaviourButtonAction("Generate Popup")]
//  private void Initialize() {

//    if (_canvasRT == null) {
//      // Generate popup boilerplate here.
//      ApplyDistance();
//      ApplyScale();
//    }
//  }

//  public void Update() {

//    if (_canvasRT == null) {
//      return;
//    }

//    var cam = MainCamera;

//    // Scene cam not available yet or is shutting down... nothing to do.
//    if (cam == null)
//      return;

//    ApplyConnector(_line, _canvasRT);

//    // Adjust the canvas height to match the telemetry elements inside of it.
//    //SetCanvasHeightBasedOnElements(_canvasRT);

//    var armOffset = transform.position - cam.transform.position;

//    transform.LookAt(transform.position + armOffset, cam.transform.up);
//    //transform.LookAt(transform.position + armOffset, cam.transform.up);
//  }

//  private void LateUpdate() {
//    foreach (var graph in _graphs) {
//      if (graph != null) {
//        graph.Refresh();
//      }
//    }
//  }


//#if UNITY_EDITOR
//  private void OnDrawGizmos () {

//    if (_canvasRT) {
//      if (_layoutIsDirty) {
//        ApplyLayout();
//        ApplyDistance();
//        ApplyScale();


//      }
//      Update();
//    }
//  }
//#endif

//  private static Vector3[] worldCornersNonalloc = new Vector3[4];


//  public void ApplyLayout() {
//    _rightPanelRT.anchorMin = new Vector2(RegionRect.xMin, RegionRect.yMin);
//    _rightPanelRT.anchorMax = new Vector2(RegionRect.xMax, RegionRect.yMax);
//    _titlePanelRT.sizeDelta = new Vector2(0, _titleHeight);
//    _layoutPanelRT.offsetMax = new Vector2(0, -_titleHeight);
//    _layoutIsDirty = false;

//    Debug.LogWarning("Apply Layout to graphs");
//    if (_graphs != null && _graphs.Length > 0) {
//      for (int i = 0; i < _graphs.Length; ++i) {
//        var graph = _graphs[i];
//        if (graph == null) {
//          continue;
//        }
//        graph.CalculateLayout();
//      }
//    }
//  }


//  public void ApplyScale() {

//    //var scale = _scale;
//    //var resol = _resolution;

//    if (_scale == _previousScale && _resolution == _previousResol)
//      return;

//    _previousResol = _resolution;
//    _previousScale = _scale;

//    _canvasRT.sizeDelta = new Vector2(_resolution, _resolution);

//    var scale = _scale / _resolution;
//    var holdrot = _canvasRT.rotation;
//    _canvasRT.rotation = Quaternion.identity;
//    var lossy = transform.lossyScale;
//    _canvasRT.localScale = new Vector3(scale /*/ lossy.x*/, scale /*/ lossy.y*/, 1 /*/ lossy.z*/);
//    _canvasRT.rotation = holdrot;
//  }

//  public void ApplyDistance() {

//    //_canvasRT.position = transform.position + (transform.rotation * new Vector3(dist, dist, -dist));
//    _canvasRT.position = transform.position + _canvasRT.forward * -_distanceOffset;
//  }

//  public void ApplyConnector(LineRenderer line, RectTransform canvasRectTransform) {

//    _layoutPanelRT.GetWorldCorners(worldCornersNonalloc);
//    // make line connect center of object with corner of popup canvas
//    line.SetPosition(0, transform.position);
//    //line.SetPosition(1, worldCornersNonalloc[0]);
//    line.SetPosition(1, worldCornersNonalloc[0]);
//    //line.SetPosition(1, canvasRectTransform.position);
//  }
//}
