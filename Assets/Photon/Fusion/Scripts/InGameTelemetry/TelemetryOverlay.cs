
//using UnityEngine;
//using Fusion;

//#if UNITY_EDITOR
//using UnityEditor;
//using Fusion.Editor;
//using Fusion.Assistants;
//#endif

////[ExecuteInEditMode]
//public class TelemetryOverlay : Fusion.NetworkBehaviour {

//  //public bool UseGlobalScale;
//  //public bool UseGlobalDistance;

//  [SerializeField]
//  [Range(0, 100f)]
//  private float _titleHeight = 60f;

//  [SerializeField]
//  //[HideInInspector]
//  [Range(0f, .1f)]
//  private float _scale = .1f;

//  [SerializeField]
//  //[HideInInspector]
//  [Unit(Units.Units, -10f, 10f)]
//  private float _distanceOffset = 1f;


//  Rect _regionRect;
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
//    var telePopup = go.GetComponent<TelemetryOverlay>();
//    //telePopup.UseGlobalDistance = false;
//    var extents = bounds.extents;
//  }

//#endif

//  [SerializeField] RectTransform _testRect;

//  [SerializeField] Canvas        _canvas;
//  [SerializeField] RectTransform _canvasRT;
//  [SerializeField] LineRenderer  _line;
//  [SerializeField] RectTransform _rightPanelRT;
//  [SerializeField] RectTransform _titlePanelRT;
//  [SerializeField] RectTransform _layoutPanelRT;

//  float _previousScale;

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
//    if (cam == null) {
//      return;
//    }

//    // Scene cam not available yet or is shutting down... nothing to do.
//    if (cam == null)
//      return;

//    //var dot = Vector3.Dot(cam.transform.forward, transform.position);
//    //if (dot > 0) {
//    //  _canvas.enabled = false;
//    //  //return;
//    //} else {
//    //  _canvas.enabled = true;
//    //}
//    var dist = Vector3.Distance(cam.transform.position, transform.position);
//    _canvas.planeDistance = dist - _distanceOffset;
//    //Debug.LogWarning(_canvas.planeDistance);
//    //Debug.LogWarning(_canvas.planeDistance);

//    var no = Object ? Object : transform.GetComponentInParent<NetworkObject>();
//    if (no == null) {
//      return;
//    }

//    const float HEIGHT = .1f;
//    const float WIDTH = .1f;

//    var screenpoint = cam.WorldToScreenPoint(no.transform.position);
//    _regionRect.Set(screenpoint.x / cam.pixelWidth, screenpoint.y / cam.pixelHeight, WIDTH, HEIGHT);
//    ApplyLayout();
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
//    _rightPanelRT.anchorMin = new Vector2(_regionRect.xMin, _regionRect.yMin);
//    _rightPanelRT.anchorMax = new Vector2(_regionRect.xMax, _regionRect.yMax);
//    _rightPanelRT.offsetMin = default;
//    _rightPanelRT.offsetMax = default;
//    _titlePanelRT.sizeDelta = new Vector2(0, _titleHeight);
//    _layoutPanelRT.offsetMax = new Vector2(0, -_titleHeight);
//    _layoutIsDirty = false;
//  }


//  public void ApplyScale() {

//    //var scale = _scale;

//    //if (scale == _previousScale)
//    //  return;

//    //var holdrot = _canvasRT.rotation;
//    //_canvasRT.rotation = Quaternion.identity;
//    //var lossy = transform.lossyScale;
//    //_canvasRT.localScale = new Vector3(scale / lossy.x, scale/ lossy.y, 1 / lossy.z);
//    //_canvasRT.rotation = holdrot;
//  }

//  public void ApplyDistance() {

//    ////_canvasRT.position = transform.position + (transform.rotation * new Vector3(dist, dist, -dist));
//    //_canvasRT.position = transform.position + _canvasRT.forward * -_distanceOffset;
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
