using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;
using UI = UnityEngine.UI;

/// <summary>
/// Fusion Telemetry component.
/// </summary>
public class FusionStats : Fusion.Behaviour {
  static FusionStats _instance;

  public static FusionStats Instance {
    get {
      if (_instance) {
        return _instance;
      }

      _instance = FindObjectOfType<FusionStats>();

      if (_instance) {
        return _instance;
      }

      return Create();
    }
  }

  public static FusionStats Create() {
    if (_instance) {
      return _instance;
    }

    return _instance = (new GameObject(nameof(FusionStats))).AddComponent<FusionStats>();
  }

  public static void SetActiveRunner(NetworkRunner runner) {
    if (runner && runner.IsRunning) {
      var instance = Instance;
      if (instance) {
        instance._active = runner;
      }
    }
  }

  const int SCREEN_SCALE_W = 1920;
  const int SCREEN_SCALE_H = 1080;

  const int   FONT_SIZE        = 12;
  const int   FONT_SIZE_BUTTON = 18;
  const float TEXT_MARGIN      = 0.25f;

  const float ALPHA  = 0.925f;
  const int   MARGIN = 10;

  public Color GraphColor = new Color(0f, 153f/255f, 1f, 1f);
  public int   Width      = 400;
  public int   Height     = 50;

  bool _hidden;
  bool _paused;

  Font              _font;
  RectTransform     _grid;
  List<FusionGraph> _graphs;

  UI.Text _pauseText;
  UI.Text _toggleText;

  NetworkRunner _active;

  Shader Shader {
    get => Resources.Load<Shader>("FusionGraphShader");
  }

  Font Font {
    get {
      if (_font == null) {
        _font = Font.CreateDynamicFontFromOSFont("Arial", FONT_SIZE);
      }

      return _font;
    }
  }

  (UI.Button, UI.Text) MakeButton(RectTransform parent, string label) {
    var rt     = CreateRectTransform(label, parent);
    var button = rt.gameObject.AddComponent<UI.Button>();
    var image  = rt.gameObject.AddComponent<UI.Image>();
    image.color = new Color(0, 0, 0, ALPHA);

    var textRt = CreateRectTransform("Text", rt, true);
    var text   = textRt.gameObject.AddComponent<UI.Text>();
    text.font      = Font;
    text.text      = label;
    text.alignment = TextAnchor.MiddleCenter;
    text.fontSize  = FONT_SIZE_BUTTON;

    return (button, text);
  }

  void Awake() {
    if (FindObjectsOfType<FusionStats>().Length > 1) {
      enabled = false;
      Destroy(gameObject);
    } else {
      _instance = this;
      DontDestroyOnLoad(gameObject);
    }
  }

  void Start() {
    if (!enabled) {
      return;
    }

    if (!GetComponent<Canvas>()) {
      var canvas = gameObject.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;

      var scaler = gameObject.AddComponent<UI.CanvasScaler>();
      scaler.uiScaleMode = UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(SCREEN_SCALE_W, SCREEN_SCALE_H);

      gameObject.AddComponent<UI.GraphicRaycaster>();

      // Only add an event system if no active event systems exist.
      if (FindObjectOfType<EventSystem>() == null) {
        gameObject.AddComponent<EventSystem>();
        gameObject.AddComponent<StandaloneInputModule>();
      }

      _grid = CreateRectTransform("Grid", gameObject.GetComponent<RectTransform>());
      _grid.anchorMax = new Vector2(1, 1);
      _grid.anchorMin = new Vector2(0, 0);
      _grid.pivot = new Vector2(0.5f, 0.5f);
      _grid.sizeDelta = new Vector2(-(MARGIN * 2), -(MARGIN * 2));
      _grid.anchoredPosition3D = default;

      var gridLayout = _grid.gameObject.AddComponent<UI.GridLayoutGroup>();
      gridLayout.constraint = UI.GridLayoutGroup.Constraint.FixedColumnCount;
      gridLayout.constraintCount = 1;
      gridLayout.startAxis = UI.GridLayoutGroup.Axis.Horizontal;
      gridLayout.startCorner = UI.GridLayoutGroup.Corner.UpperLeft;
      gridLayout.spacing = new Vector2(MARGIN, MARGIN);
      gridLayout.cellSize = new Vector2(Width, Height);

      var buttonsCell = CreateRectTransform("Buttons", _grid);
      var buttonsGrid = buttonsCell.gameObject.AddComponent<UI.GridLayoutGroup>();
      buttonsGrid.constraint = UI.GridLayoutGroup.Constraint.FixedRowCount;
      buttonsGrid.constraintCount = 1;
      buttonsGrid.startAxis = UI.GridLayoutGroup.Axis.Horizontal;
      buttonsGrid.startCorner = UI.GridLayoutGroup.Corner.UpperLeft;
      buttonsGrid.spacing = new Vector2(MARGIN, MARGIN);
      buttonsGrid.cellSize = new Vector2(100, Height);

      var (clearButton, _) = MakeButton(buttonsCell, "Clear");
      clearButton.onClick.AddListener(Clear);

      var (toggle, toggleText) = MakeButton(buttonsCell, "Hide");
      toggle.onClick.AddListener(Toggle);
      _toggleText = toggleText;

      var (pause, pauseText) = MakeButton(buttonsCell, "Pause");
      pause.onClick.AddListener(Pause);
      _pauseText = pauseText;
    }

    _graphs = new List<FusionGraph>();

    CreateGraph("FrameTime (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.FrameTime);

    CreateGraph("Ping (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.RoundTripTime);

    // snapshot
    
    CreateGraph("Snapshot Size (bytes)", goodColor: GraphColor, badColor: GraphColor).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.PacketSize);

    CreateGraph("Snapshot Delta (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InterpStateDelta);
    
    // interp
    
    CreateGraph("Interpolation Timescale (%)", goodColor: GraphColor, badColor: GraphColor, multiplier: 100, fractions: 1).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InterpTimescale);

    CreateGraph("Interpolation Offset (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InterpOffset);

#if FUSION_DEV
    CreateGraph("Interpolation Diff (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InterpDiff);
#endif
   
#if FUSION_DEV
    CreateGraph("Interpolation Uncertainty (%)", goodColor: GraphColor, badColor: GraphColor, multiplier: 100, fractions: 2).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InterpUncertainty);

    CreateGraph("Interpolation Multiplier", goodColor: GraphColor, badColor: GraphColor, multiplier: 1, fractions: 2).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InterpMultiplier);
#endif
    
    // prediction
    
    CreateGraph("Prediction Timescale (%)", goodColor: GraphColor, badColor: GraphColor, multiplier: 100, fractions: 1).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.SimulationTimeScale);

#if FUSION_DEV
    CreateGraph("Prediction Offset Target (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InputOffsetTarget);
#endif
    
    CreateGraph("Prediction Offset (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InputOffset);

#if FUSION_DEV
    CreateGraph("Prediction Offset Deviation (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InputOffsetDeviation);
#endif
    
    CreateGraph("Prediction Delta (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InputReceiveDelta);

#if FUSION_DEV
    CreateGraph("Prediction Delta Deviation (ms)", goodColor: GraphColor, badColor: GraphColor, multiplier: 1000).DataSource =
      StateUpdateCallback(r => r.Simulation.Stats.InputReceiveDeltaDeviation);
#endif
    
  }

  void Pause() {
    _paused         = !_paused;
    _pauseText.text = _paused ? "Continue" : "Pause";

    var enumerator = NetworkRunner.GetInstancesEnumerator();

    while (enumerator.MoveNext()) {
      if (enumerator.Current && enumerator.Current.IsRunning) {
        enumerator.Current.Simulation.Stats.Pause(_paused);
      }
    }
  }

  void Toggle() {
    _hidden          = !_hidden;
    _toggleText.text = _hidden ? "Show" : "Hide";

    foreach (var graph in _graphs) {
      graph.gameObject.SetActive(_hidden == false);
    }
  }

  void LateUpdate() {
    foreach (var graph in _graphs) {
      graph.Refresh();
    }
  }

  Func<Simulation.Statistics.StatsBuffer<float>> StateUpdateCallback(Func<NetworkRunner, Simulation.Statistics.StatsBuffer<float>> callback) {
    return () => {
      if (_paused) {
        return null;
      }

      var runner = GetActiveRunner(SimulationModes.Client);
      if (runner) {
        return callback(runner);
      }

      return null;
    };
  }

  void Clear() {
    var enumerator = NetworkRunner.GetInstancesEnumerator();

    while (enumerator.MoveNext()) {
      if (enumerator.Current && enumerator.Current.IsRunning) {
        enumerator.Current.Simulation.Stats.Clear();
      }
    }

    for (int i = 0; i < _graphs.Count; ++i) {
      _graphs[i].Clear();
    }
  }

  NetworkRunner GetActiveRunner(SimulationModes? mode = null) {
    if (_active && _active.IsRunning) {
      return _active;
    }
    
    var enumerator = NetworkRunner.GetInstancesEnumerator();

    while (enumerator.MoveNext()) {
      if (enumerator.Current && enumerator.Current.IsRunning) {
        if (mode.HasValue && mode.Value != enumerator.Current.Mode) {
          continue;
        }

        return enumerator.Current;
      }
    }

    return null;
  }

  static void ExpandAnchor(RectTransform rt) {
    rt.anchorMax        = new Vector2(1, 1);
    rt.anchorMin        = new Vector2(0, 0);
    rt.pivot            = new Vector2(0.5f, 0.5f);
    rt.sizeDelta        = default;
    rt.anchoredPosition = default;
  }


  static RectTransform CreateRectTransform(string name, RectTransform parent, bool expand = false) {
    var go = new GameObject(name);
    var rt = go.AddComponent<RectTransform>();
    rt.SetParent(parent);

    if (expand) {
      ExpandAnchor(rt);
    }

    return rt;
  }

  UI.Text AddText(GameObject go, string label, TextAnchor anchor) {
    var text = go.AddComponent<UI.Text>();
    text.text      = label;
    text.color     = Color.white;
    text.font      = Font;
    text.alignment = anchor;
    text.fontSize  = FONT_SIZE;

    return text;
  }

  public FusionGraph CreateGraph(string name, Color goodColor, Color badColor, float multiplier = 1, int fractions = 2) {
    var root = CreateRectTransform(name, _grid);
    var fg   = root.gameObject.AddComponent<FusionGraph>();

    fg.Fractions  = fractions;
    fg.Multiplier = multiplier;

    root.anchorMin          = new Vector2(0.5f, 0.5f);
    root.anchorMax          = new Vector2(0.5f, 0.5f);
    root.sizeDelta          = new Vector2(Width + 10, Height + 10);
    root.anchoredPosition3D = default;

    var background = CreateRectTransform("Background", root);
    background.anchorMin        = new Vector2(0, 0);
    background.anchorMax        = new Vector2(1, 1);
    background.anchoredPosition = default;
    background.sizeDelta        = default;

    var backgroundImage = background.gameObject.AddComponent<UI.Image>();
    backgroundImage.color = new Color(0, 0, 0, ALPHA);

    var graph = CreateRectTransform("Graph", root);
    graph.anchorMin        = new Vector2(0, TEXT_MARGIN);
    graph.anchorMax        = new Vector2(1, 1f - TEXT_MARGIN);
    graph.anchoredPosition = default;
    graph.sizeDelta        = default;

    fg.Image          = graph.gameObject.AddComponent<UI.Image>();
    fg.Image.material = new Material(Shader);
    fg.Image.material.SetColor("_GoodColor", goodColor);
    fg.Image.material.SetColor("_BadColor", badColor);

    var title = CreateRectTransform("Title", root);
    title.anchorMin        = new Vector2(0, 1f - TEXT_MARGIN);
    title.anchorMax        = new Vector2(1, 1f);
    title.anchoredPosition = new Vector2(0, 0);
    title.sizeDelta        = default;
    AddText(title.gameObject, name, TextAnchor.UpperRight);

    var max = CreateRectTransform("Max", root);
    max.anchorMin        = new Vector2(0, 1f - TEXT_MARGIN);
    max.anchorMax        = new Vector2(1, 1f);
    max.anchoredPosition = new Vector2(0, 0);
    max.sizeDelta        = default;
    fg.LabelMax          = AddText(max.gameObject, "0", TextAnchor.UpperLeft);

    var min = CreateRectTransform("Min", root);
    min.anchorMin        = new Vector2(0, 0f);
    min.anchorMax        = new Vector2(1, TEXT_MARGIN);
    min.anchoredPosition = new Vector2(0, 0);
    min.sizeDelta        = default;
    fg.LabelMin          = AddText(min.gameObject, "0", TextAnchor.UpperLeft);


    var avg = CreateRectTransform("Last", root);
    avg.anchorMin        = new Vector2(0, 0f);
    avg.anchorMax        = new Vector2(1, TEXT_MARGIN);
    avg.anchoredPosition = new Vector2(0, 0);
    avg.sizeDelta        = default;
    fg.LabelAvg          = AddText(avg.gameObject, "0", TextAnchor.UpperCenter);

    var last = CreateRectTransform("Last", root);
    last.anchorMin        = new Vector2(0, 0f);
    last.anchorMax        = new Vector2(1, TEXT_MARGIN);
    last.anchoredPosition = new Vector2(0, 0);
    last.sizeDelta        = default;
    fg.LabelLast          = AddText(last.gameObject, "0", TextAnchor.UpperRight);


    _graphs.Add(fg);

    return fg;
  }
}