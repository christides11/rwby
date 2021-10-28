using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UI = UnityEngine.UI;
using Fusion;
using Stats = Fusion.Simulation.Statistics;

/// <summary>
/// Creates and controls a Canvas with one or multiple telemetry graphs. Can be created as a scene object or prefab,
/// or be created at runtime using the <see cref="Create"/> methods.
/// </summary>
[ExecuteAlways]
public class FusionStats : Fusion.Behaviour {

  public enum DefaultLayouts {
    Custom,
    Left,
    Right,
  }


  // Lookup for all FusionStats associated with active runners.
  static Dictionary<NetworkRunner, List<FusionStats>> _statsForRunnerLookup = new Dictionary<NetworkRunner, List<FusionStats>>();

  // Record of active FusionStats, used to prevent more than one _guid version from existing (in the case of FusionStats existing in a scene that gets cloned in Multi-Peer).
  static Dictionary<string, FusionStats> _activeGuids = new Dictionary<string, FusionStats>();

  // Added to make calling by reflection cleaner internally.
  internal static FusionStats CreateInternal(NetworkRunner runner = null, DefaultLayouts layout = DefaultLayouts.Left, Stats.FusionStatFlags? statsMask = null) {
    return Create(runner, layout, statsMask);
  }

  /// <summary>
  /// Creates a new GameObject with a <see cref="FusionStats"/> component.
  /// To generate the UI when not playing, call <see cref="Initialize"/> on the returned <see cref="FusionStats"/> component.
  /// Initialize automatically runs on Awake().
  /// </summary>
  /// <param name="runner"></param>
  /// <param name="normalizedRect">If not null, uses the supplied Rect as the initial position.</param>
  /// <param name="statsMask">The stats to be enabled. If left null, default statistics will be used.</param>
  public static FusionStats Create(NetworkRunner runner, Rect? normalizedRect, Stats.FusionStatFlags? statsMask = null) {

    if (normalizedRect.HasValue) {
      var fusionStats = Create(runner, DefaultLayouts.Custom, statsMask.HasValue ? statsMask.Value : DefaultMask);
      fusionStats.Position = normalizedRect.Value;
      return fusionStats;
    }
    return Create(runner, DefaultLayouts.Left, statsMask);
  }

  /// <summary>
  /// Creates a new GameObject with a <see cref="FusionStats"/> component.
  /// To generate the UI when not playing, call <see cref="Initialize"/> on the returned <see cref="FusionStats"/> component.
  /// Initialize automatically runs on Awake().
  /// </summary>
  /// <param name="runner"></param>
  /// <param name="layout">Uses a predefined position.</param>
  /// <param name="statsMask">The stats to be enabled. If left null, default statistics will be used.</param>
  /// <returns></returns>
  public static FusionStats Create(NetworkRunner runner = null, DefaultLayouts layout = DefaultLayouts.Left, Stats.FusionStatFlags? statsMask = null) {
    if (statsMask.HasValue == false) {
      statsMask = DefaultMask;
    }

    var fusionStats = new GameObject($"{nameof(FusionStats)} {(runner ? runner.name : "null")}").AddComponent<FusionStats>();
    fusionStats.ActiveRunner = runner;
    fusionStats._includedStats = statsMask.Value;
    fusionStats.ApplyDefaultLayout(layout);

    if (runner != null) {
      fusionStats.AutoDestroy = true;
    }
    return fusionStats;
  }

  /// <summary>
  /// Defines the stat types that should only be shown if explicitly asked for.
  /// </summary>
#if FUSION_DEV
  public static Stats.FusionStatFlags DefaultMask => (Stats.FusionStatFlags)(-1);
#else
  public static Stats.FusionStatFlags DefaultMask => 
      ~(
      Stats.FusionStatFlags.InterpDiff |
      Stats.FusionStatFlags.InterpUncertainty |
      Stats.FusionStatFlags.InterpMultiplier |
      Stats.FusionStatFlags.InputOffsetTarget |
      Stats.FusionStatFlags.InputOffsetDeviation |
      Stats.FusionStatFlags.InputReceiveDeltaDeviation
      );
#endif

  private double _currentDrawTime;
  private double _delayDrawUntil;

  const int   SCREEN_SCALE_W     = 1080;
  const int   SCREEN_SCALE_H     = 1080;
  const int   FONT_SIZE          = 12;
  const int   FONT_SIZE_BUTTON   = 20;
  const int   FONT_SIZE_MIN      = 4;
  const int   FONT_SIZE_MAX      = 32;
  const int   BTTN_FONT_SIZE_MAX = 40;
  const float TEXT_MARGIN        = 0.25f;
  const float TITLE_HEIGHT       = 20f;
  const float ALPHA              = 0.925f;
  const int   MARGIN             = 6;

  const string PLAY_TEXT = "PLAY";
  const string PAUS_TEXT = "PAUSE";
  const string SHOW_TEXT = "SHOW";
  const string HIDE_TEXT = "HIDE";
  const string CLER_TEXT = "CLEAR";
  const string CLSE_TEXT = "CLOSE";

  const string PLAY_ICON = "\u25ba";
  const string PAUS_ICON = "II";
  const string HIDE_ICON = "\u25bc";
  const string SHOW_ICON = "\u25b2";
  const string CLER_ICON = "\u1d13";
  const string CLSE_ICON = "x";

  // Used by DrawIfAttribute to determine inspector visibility of fields are runtime.
  bool ShowColorControls => !Application.isPlaying && ModifyColors;
  bool IsPlaying => Application.isPlaying;

  /// <summary>
  /// Interval (in seconds) between Graph redraws. Lower values reduce CPU overhead, draw calls and garbage collection. 
  /// </summary>
  [InlineHelp]
  [Unit(Units.Seconds, 1f, 0f, DecimalPlaces = 2)]
  public float RedrawInterval = .1f;

  /// <summary>
  /// Height of button region at top of the stats panel. Values less than or equal to 0 hide the buttons, and reduce the header size.
  /// </summary>
  [InlineHelp]
  [SerializeField]
  //[Unit(Units.Units, -TITLE_HEIGHT, 200)]
  [Range(-TITLE_HEIGHT, 80)]
  int _buttonHeight = 60;
  public int ButtonHeight {
    get => _buttonHeight;
    set {
      _buttonHeight = value;
      _layoutDirty = true;
    }
  }

  /// <summary>
  /// The Rect that defines the position of the entire states overlay on the player. Sizes are normalized percentages.(ranges of 0f-1f).
  /// </summary>
  [InlineHelp]
  [SerializeField]
  [NormalizedRect]
  Rect _position = new Rect(0.0f, 0.0f, 0.3f, 1.0f);
  public Rect Position {
    get => _position;
    set {
      _position = value;
      _layoutDirty = true;
    }
  }

  /// <summary>
  /// Initializes a <see cref="FusionGraph"/> for all available stats, even if not initially included. 
  /// If disabled, graphs added after initialization will be added to the bottom of the interface stack.
  /// </summary>
  [Header("Data")]
  [InlineHelp]
  public bool InitializeAllGraphs;

  /// <summary>
  /// When <see cref="_activeRunner"/> is null, will continuously attempt to find and connect to an active <see cref="NetworkRunner"/> which matches these indicated modes.
  /// </summary>
  [InlineHelp]
  [VersaMask]
  public SimulationModes ConnectTo = SimulationModes.Host | SimulationModes.Server | SimulationModes.Client;

  /// <summary>
  /// Selects which stats should be displayed.
  /// </summary>
  [InlineHelp]
  [SerializeField]
  [VersaMask]
  Stats.FusionStatFlags _includedStats;
  public Stats.FusionStatFlags IncludedStats {
    get => _includedStats;
    set {
      _includedStats = value;
      _activeDirty = true;
    }
  }

  [Header("Life-Cycle")]

  /// <summary>
  /// Automatically destroys this <see cref="FusionStats"/> GameObject if the associated runner is null or inactive.
  /// Otherwise attempts will continuously be made to find an new active runner which is running in <see cref="SimulationModes"/> specified by <see cref="ConnectTo"/>, and connect to that.
  /// </summary>
  [InlineHelp]
  [SerializeField]
  public bool AutoDestroy;

  /// <summary>
  /// Only one instance with the <see cref="Guid"/> can exist. Will destroy any clones on Awake.
  /// </summary>
  [InlineHelp]
  [SerializeField]
  public bool EnforceSingle = true;

  /// <summary>
  /// Identifier used to enforce single instances. Only one instance at a given time.
  /// </summary>
  [InlineHelp]
  [DrawIf(nameof(EnforceSingle), true)]
  [SerializeField]
  public string Guid;

  [Header("Customization")]
  /// <summary>
  /// Shows/hides controls in the inspector for defining element colors.
  /// </summary>
  [InlineHelp]
  [DrawIf(nameof(IsPlaying), false, DrawIfHideType.Hide)]
  public bool ModifyColors;

  /// <summary>
  /// The color used for the telemetry graph data.
  /// </summary>
  [DrawIf(nameof(ShowColorControls), true, DrawIfHideType.Hide)]
  public Color GraphColor = new Color(0f, 153f / 255f, 1f, 1f);

  [DrawIf(nameof(ShowColorControls), true, DrawIfHideType.Hide)]
  public Color FontColor = new Color(1, 1, 1, .5f);

  [DrawIf(nameof(ShowColorControls), true, DrawIfHideType.Hide)]
  public Color PanelColor = new Color(.25f, .25f, .25f, .8f);

  bool _hidden;
  bool _paused;
  bool _layoutDirty;
  bool _activeDirty;

  Font _font;

  [SerializeField] [HideInInspector] FusionGraph[] _graphs;

  [SerializeField] [HideInInspector] UI.Text _pauseIcon;
  [SerializeField] [HideInInspector] UI.Text _pauseLabel;
  [SerializeField] [HideInInspector] UI.Text _toggleIcon;
  [SerializeField] [HideInInspector] UI.Text _toggleLabel;
  [SerializeField] [HideInInspector] UI.Text _titleText;

  [SerializeField] [HideInInspector] Canvas        _canvas;
  [SerializeField] [HideInInspector] RectTransform _canvasRT;
  [SerializeField] [HideInInspector] RectTransform _rootPanelRT;
  [SerializeField] [HideInInspector] RectTransform _headerRT;
  [SerializeField] [HideInInspector] RectTransform _statsRT;
  [SerializeField] [HideInInspector] RectTransform _statsLayoutRT;
  [SerializeField] [HideInInspector] RectTransform _titleRT;

  [SerializeField] [HideInInspector] UI.Button _clearButton;
  [SerializeField] [HideInInspector] UI.Button _toggleButton;
  [SerializeField] [HideInInspector] UI.Button _pauseButton;
  [SerializeField] [HideInInspector] UI.Button _closeButton;

  NetworkRunner _activeRunner;
  public NetworkRunner ActiveRunner {
    get {
      // If the runner is null or inactive, reapply to ActiveRunner setter in case the state of the runner has changed and needs to be disconnected.
      if (_activeRunner == null || _activeRunner.IsShutdown) {
        ActiveRunner = null;
      }
      return _activeRunner;
    }
    set {
      if (_activeRunner == value) {
        return;
      }
      // Keep track of which runners have active stats windows - needed so pause/unpause can affect all (since pause affects other panels)
      DisassociateWithRunner(_activeRunner);
      _activeRunner = value;
      AssociateWithRunner(value);

      var label = value ? value.name : "Disconnected";
      if (_titleText) {
        _titleText.text = label;
      }
      name = $"{nameof(FusionStats)} {label}";

      if (_graphs != null) {
        for (int i = 0; i < _graphs.Length; ++i) {
          var graph = _graphs[i];
          if (graph != null) {
            graph.Runner = _activeRunner;
          }
        }
      }
    }
  }

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

  void OnValidate() {

    if (EnforceSingle && Guid == "") {
      Guid = System.Guid.NewGuid().ToString().Substring(0, 13);
    }
    _activeDirty = true;
    _layoutDirty = true;

    if (Application.isPlaying) {
      ReapplyEnabled();
    } else {
    // Editor only code to force a delayed layout refresh.
      LazyEditorRepaint(0, false);
    }
  }

  // Changes to enabled graphs and layout in OnValidate need to be deferred to apply correctly.
  void LazyEditorRepaint(int extraWaitTicks, bool hideCanvas) {
#if UNITY_EDITOR
    if (hideCanvas && _canvas) {
      _canvas.enabled = false;
      _canvasHidden = true;
    }
    _layoutDirty = true;
    ReapplyEnabled();
    UnityEditor.EditorApplication.delayCall += DelayedLayoutRefresh;
    _waitTicks = extraWaitTicks;
#endif
  }
#if UNITY_EDITOR
  int  _waitTicks;
  bool _canvasHidden;
  void DelayedLayoutRefresh() {

    if (_waitTicks > 0) {
      UnityEditor.EditorApplication.delayCall += DelayedLayoutRefresh;
      _waitTicks--;
      return;
    }
   
    if (_canvasHidden && _canvas) {
      _canvas.enabled = true;
    }
    
    CalculateLayout();
  }
#endif

  void Reset() {
    Guid = System.Guid.NewGuid().ToString().Substring(0, 13);
    _includedStats = DefaultMask;
  }

  void Awake() {

    if (Application.isPlaying == false) {
      if (_canvas) {
        // Hide canvas for rebuild, Unity makes this ugly.
        LazyEditorRepaint(0, true);
      }
      return;
    }

    if (Guid == "") {
      Guid = System.Guid.NewGuid().ToString().Substring(0, 13);
    }

    if (EnforceSingle && Guid != null) {
      if (_activeGuids.ContainsKey(Guid)) {
        Destroy(this.gameObject);
        return;
      }
      _activeGuids.Add(Guid, this);
    }

    DontDestroyOnLoad(gameObject);
  }

  void Start() {
    if (Application.isPlaying) {
      Initialize();
      _activeDirty = true;
      _layoutDirty = true;
    }
  }

  void OnDestroy() {
    // Try to unregister this Stats in case it hasn't already.
    DisassociateWithRunner(_activeRunner);

    // If this is the current enforce single instance of this GUID, remove it from the record.
    if (Guid != null) {
      if (_activeGuids.TryGetValue(Guid, out var stats)) {
        if (stats == this) {
          _activeGuids.Remove(Guid);
        }
      }
    }
  }

  [BehaviourButtonAction("Destroy Graphs", conditionMember: nameof(_canvasRT), ConditionFlags = BehaviourActionAttribute.ActionFlags.ShowAtNotRuntime)]
  void DestroyGraphs() {
    if (_canvasRT) {
      DestroyImmediate(_canvasRT.gameObject);
    }
    _canvasRT = null;
  }

  bool _graphsAreMissing => _canvasRT == null;
  [BehaviourButtonAction("Generate Graphs", conditionMember: nameof(_graphsAreMissing), ConditionFlags = BehaviourActionAttribute.ActionFlags.ShowAtNotRuntime)]
  void Initialize() {

    // Only add an event system if no active event systems exist.
    if (Application.isPlaying && FindObjectOfType<EventSystem>() == null) {
      var eventSystemGO = new GameObject("Event System");
      eventSystemGO.AddComponent<EventSystem>();
      eventSystemGO.AddComponent<StandaloneInputModule>();
      if (Application.isPlaying) {
        DontDestroyOnLoad(eventSystemGO);
      }
    }

    // Already existed before runtime. (Scene object)
    if (_canvasRT) {
      // Listener connections are not retained with serialization and always need to be connected at startup.
      _clearButton .onClick.AddListener(Clear);
      _toggleButton.onClick.AddListener(Toggle);
      _pauseButton .onClick.AddListener(Pause);
      _closeButton .onClick.AddListener(Close);
      // Run Unity first frame layout failure hack.
      StartCoroutine(DelayedDirty());
      return;
    }

    var rootRectTr = gameObject.GetComponent<Transform>();
    _canvasRT      = CreateRectTransform("Stats Canvas", rootRectTr);
    _canvas        = _canvasRT.gameObject.AddComponent<Canvas>();
    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

    var scaler = _canvasRT.gameObject.AddComponent<UI.CanvasScaler>();
    scaler.uiScaleMode         = UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(SCREEN_SCALE_W, SCREEN_SCALE_H);
    scaler.matchWidthOrHeight  = .4f;

    _canvasRT.gameObject.AddComponent<UI.GraphicRaycaster>();

    _rootPanelRT = CreateRectTransform("Root Panel", _canvasRT);

    _headerRT = CreateRectTransform("Header", _rootPanelRT);
    _headerRT.gameObject.AddComponent<UI.Image>().color = PanelColor;

    _titleRT = CreateRectTransform("Runner Title", _headerRT);
    _titleRT.anchorMin = new Vector2(0, 0);
    _titleRT.anchorMax = new Vector2(1, 1);
    _titleRT.offsetMin = new Vector2(0, 0);
    _titleRT.offsetMax = new Vector2(-MARGIN, -MARGIN);
    _titleText = AddText(_titleRT.gameObject, _activeRunner ? _activeRunner.name : "Disconnected", TextAnchor.UpperCenter);

    // Buttons
    var buttonsRT = CreateRectTransform("Buttons", _headerRT);
    buttonsRT.anchorMax = new Vector2(1f, 1f);
    buttonsRT.anchorMin = new Vector2(0, 0);
    buttonsRT.pivot     = new Vector2(0.5f, 0);
    buttonsRT.sizeDelta = new Vector2(-(MARGIN * 2), -(TITLE_HEIGHT + (MARGIN * 2)));
    buttonsRT.anchoredPosition3D = default;
    var buttonsGrid = buttonsRT.gameObject.AddComponent<UI.HorizontalLayoutGroup>();
    buttonsGrid.childControlHeight = true;
    buttonsGrid.childControlWidth = true;
    buttonsGrid.spacing = MARGIN;
    MakeButton(buttonsRT, ref _toggleButton, HIDE_ICON, HIDE_TEXT, out _toggleIcon, out _toggleLabel, Toggle);
    MakeButton(buttonsRT, ref _pauseButton,  PAUS_ICON, PAUS_TEXT, out _pauseIcon,  out _pauseLabel,  Pause);
    MakeButton(buttonsRT, ref _clearButton,  CLER_ICON, CLER_TEXT, out _,           out _,            Clear);
    MakeButton(buttonsRT, ref _closeButton,  CLSE_ICON, CLSE_TEXT, out _,           out _,            Close);

    // Stats stack
    _statsRT = CreateRectTransform("Stats Panel", _rootPanelRT);
    _statsRT.gameObject.AddComponent<UI.Image>().color = PanelColor;

    _statsLayoutRT = CreateRectTransform("Layout", _statsRT);
    _statsLayoutRT.anchorMax = new Vector2(1, 1);
    _statsLayoutRT.anchorMin = new Vector2(0, 0);
    _statsLayoutRT.pivot = new Vector2(0.5f, 0.5f);
    _statsLayoutRT.sizeDelta = new Vector2(-(MARGIN * 2), -(MARGIN * 2));
    _statsLayoutRT.anchoredPosition3D = default;

    var gridLayout = _statsLayoutRT.gameObject.AddComponent<UI.VerticalLayoutGroup>();
    gridLayout.childControlHeight = true;
    gridLayout.childControlWidth = true;
    gridLayout.spacing = MARGIN;

    CalculateLayout();

    _graphs = new FusionGraph[Stats.STAT_TYPE_COUNT];
    for (int i = 0; i < Stats.STAT_TYPE_COUNT; ++i) {
      if (InitializeAllGraphs == false) {
        var statFlag = (Stats.FusionStatFlags)(1 << i);
        if ((statFlag & _includedStats) == 0) {
          continue;
        }
      }

      CreateGraph((Stats.FusionStats)i, _statsLayoutRT, GraphColor, GraphColor);
    }
    // Hide canvas for a tick if spawned at runtime. Unity makes some ugliness on the first update.
    if (Application.isPlaying) {
      _canvas.enabled = false;
      _activeDirty = true;
      _layoutDirty = true;
      StartCoroutine(ShowCanvas(_canvas));
    } else {
      LazyEditorRepaint(1, true);
    }
  }


  // Hacks for Unity startup ugliness with runtime created FusionStats on the first update.
  IEnumerator ShowCanvas(Canvas canvas) {
    yield return null;
    _activeDirty = true;
    _layoutDirty = true;
    yield return null;
    canvas.enabled = true;
  }

  // Hacks for Unity startup ugliness. Forces scene FusionStats to run layout on second update.
  IEnumerator DelayedDirty() {
    yield return null;
    _activeDirty = true;
    _layoutDirty = true;
  }

  void AssociateWithRunner(NetworkRunner runner) {
    if (runner != null) {
      if (_statsForRunnerLookup.TryGetValue(runner, out var runnerStats) == false) {
        _statsForRunnerLookup.Add(runner, new List<FusionStats>() { this });
      } else {
        runnerStats.Add(this);
      }
    }
  }

  void DisassociateWithRunner(NetworkRunner runner) {
    if (runner != null && _statsForRunnerLookup.TryGetValue(runner, out var oldrunnerstats)) {
      if (oldrunnerstats.Contains(this)) {
        oldrunnerstats.Remove(this);
      }
    }
  }

  void Pause() {
    if (_activeRunner && _activeRunner.Simulation != null) {
      _paused = !_paused;

      _activeRunner.Simulation.Stats.Pause(_paused);
      // Pause for all FusionStats tied to this runner.
      if (_statsForRunnerLookup.TryGetValue(_activeRunner, out var stats)) {
        var icon  = _paused ? PLAY_ICON : PAUS_ICON;
        var label = _paused ? PLAY_TEXT : PAUS_TEXT;
        foreach (var stat in stats) {
          if (stat) {
            stat._pauseIcon.text  = icon;
            stat._pauseLabel.text = label;
          }
        }
      }
    }
  }

  void Toggle() {
    _hidden = !_hidden;

    _toggleIcon.text  = _hidden ? SHOW_ICON : HIDE_ICON;
    _toggleLabel.text = _hidden ? SHOW_TEXT : HIDE_TEXT;

    _statsRT.gameObject.SetActive(!_hidden);

    for (int i = 0; i < _graphs.Length; ++i) {
      var graph = _graphs[i];
      if (graph) {
        _graphs[i].gameObject.SetActive(!_hidden && (1 << i & (int)_includedStats) != 0);
      }
    }
  }

  void Clear() {
    if (_activeRunner && _activeRunner.Simulation != null) {
      _activeRunner.Simulation.Stats.Clear();
    }

    for (int i = 0; i < _graphs.Length; ++i) {
      var graph = _graphs[i];
      if (graph) {
        _graphs[i].Clear();
      }
    }
  }

  void Close() {
    Destroy(this.gameObject);
  }

#if UNITY_EDITOR
  // Having this here makes dragging rect feel much more responsive, than relying on OnValidate only for repaints.
  private void OnDrawGizmosSelected() {
    if (Application.isPlaying == false) {
      if (_layoutDirty) {
        CalculateLayout();
      }
    }
  }
#endif

  void LateUpdate() {

    // Use of the ActiveRunner getter here is intentional - this forces a test of the existing Runner having gone null or inactive.
    var runner = ActiveRunner;
    bool runnerIsNull = runner == null;

    if (AutoDestroy && runnerIsNull) {
      Destroy(this.gameObject);
      return;
    }

    if (_activeDirty) {
      ReapplyEnabled();
    }

    if (_layoutDirty) {
      CalculateLayout();
    }

    // If runner is null, see if there is a runner we can connect to.
    if (runnerIsNull) {
      if (FindActiveRunner(ConnectTo) == null) {
        return;
      }
    }

    // Cap redraw rate - rate of 0 = disabled.
    if (RedrawInterval > 0) {
      var currentime = Time.timeAsDouble;
      if (currentime > _delayDrawUntil) {
        _currentDrawTime = currentime;
        while (_delayDrawUntil <= currentime) {
          _delayDrawUntil += RedrawInterval;
        }
      }

      if (currentime != _currentDrawTime) {
        return;
      }
    }

    foreach (var graph in _graphs) {
      if (graph != null) {
        graph.Refresh();
      }
    }
  }

  static void ExpandAnchor(RectTransform rt) {
    rt.anchorMax        = new Vector2(1, 1);
    rt.anchorMin        = new Vector2(0, 0);
    rt.pivot            = new Vector2(0.5f, 0.5f);
    rt.sizeDelta        = default;
    rt.anchoredPosition = default;
  }

  UI.Image MakeButton(RectTransform parent, ref UI.Button button, string label, Sprite sprite, UnityAction action) {
    var rt = CreateRectTransform(label, parent);
    button = rt.gameObject.AddComponent<UI.Button>();
    //var back = rt.gameObject.AddComponent<UI.Image>();

    //back.color = new Color(1f, 1f, 1f, ALPHA);

    var iconRt = CreateRectTransform("Icon", rt, true);
    var image = iconRt.gameObject.AddComponent<UI.Image>();
    image.sprite = sprite; // Resources.Load<Sprite>("Icons/FusionStatsShowIcon");
    //image.color = new Color(1, 1, 1, .5f);
    image.preserveAspect = true;
    iconRt.anchorMin = new Vector2(0.2f, 0.2f);
    iconRt.anchorMax = new Vector2(0.8f, 0.8f);
    iconRt.sizeDelta = default;

    button.image = image;

    UI.ColorBlock colors    = button.colors;
    colors.normalColor      = new Color(.0f, .0f, .0f, ALPHA);
    colors.pressedColor     = new Color(.5f, .5f, .5f, ALPHA);
    colors.highlightedColor = new Color(.3f, .3f, .3f, ALPHA);
    colors.selectedColor    = new Color(.0f, .0f, .0f, ALPHA);
    button.colors           = colors;

    button.onClick.AddListener(action);
    return image;
  }


  void MakeButton(RectTransform parent, ref UI.Button button, string iconText, string labelText, out UI.Text icon, out UI.Text text, UnityAction action) {
    var rt = CreateRectTransform(labelText, parent);
    button = rt.gameObject.AddComponent<UI.Button>();

    var iconRt = CreateRectTransform("Icon", rt, true);
    iconRt.anchorMin = new Vector2(0, 0.4f);
    iconRt.anchorMax = new Vector2(1, 1.0f);
    iconRt.offsetMin = new Vector2(0, 0);
    iconRt.offsetMax = new Vector2(0, -4);

    icon = iconRt.gameObject.AddComponent<UI.Text>();
    button.targetGraphic   = icon;
    icon.font              = Font;
    icon.text              = iconText;
    icon.alignment         = TextAnchor.MiddleCenter;
    icon.fontStyle         = FontStyle.Bold;
    icon.fontSize          = BTTN_FONT_SIZE_MAX;
    icon.resizeTextMinSize = 0;
    icon.resizeTextMaxSize = BTTN_FONT_SIZE_MAX;
    icon.alignByGeometry = true;
    icon.resizeTextForBestFit = true;

    var textRt = CreateRectTransform("Label", rt, true);
    textRt.anchorMin = new Vector2(0, 0.0f);
    textRt.anchorMax = new Vector2(1, 0.4f);
    textRt.offsetMin = new Vector2( 6, 10);
    textRt.offsetMax = new Vector2(-6, 0);

    text                      = textRt.gameObject.AddComponent<UI.Text>();
    text.color                = Color.black;
    text.font                 = Font;
    text.text                 = labelText;
    text.alignment            = TextAnchor.MiddleCenter;
    text.fontStyle            = FontStyle.Bold;
    text.fontSize             = 0;
    text.resizeTextMinSize    = FONT_SIZE_MIN;
    text.resizeTextMaxSize    = BTTN_FONT_SIZE_MAX;
    text.resizeTextForBestFit = true;

    UI.ColorBlock colors    = button.colors;
    colors.normalColor      = new Color(.0f, .0f, .0f, ALPHA);
    colors.pressedColor     = new Color(.5f, .5f, .5f, ALPHA);
    colors.highlightedColor = new Color(.3f, .3f, .3f, ALPHA);
    colors.selectedColor    = new Color(.0f, .0f, .0f, ALPHA);
    button.colors = colors;

    button.onClick.AddListener(action);
  }

  static RectTransform CreateRectTransform(string name, Transform parent, bool expand = false) {
    var go = new GameObject(name);
    var rt = go.AddComponent<RectTransform>();
    rt.SetParent(parent);
    rt.localScale = new Vector3(1, 1, 1);

    if (expand) {
      ExpandAnchor(rt);
    }
    return rt;
  }

  UI.Text AddText(GameObject go, string label, TextAnchor anchor) {
    var text = go.AddComponent<UI.Text>();
    text.text      = label;
    text.color     = FontColor;
    text.font      = Font;
    text.alignment = anchor;
    text.fontSize  = FONT_SIZE;

    text.resizeTextMinSize    = FONT_SIZE_MIN;
    text.resizeTextMaxSize    = FONT_SIZE_MAX;
    text.resizeTextForBestFit = true;
    return text;
  }

  public FusionGraph CreateGraph(Stats.FusionStats statId, RectTransform parentRT, Color goodColor, Color badColor, float? multiplier = null, int? decimals = null) {

    var statInfo = Stats.GetDescription(statId);
    // If no explicit multiplier was given, use the default for this stat.
    if (multiplier.HasValue == false) {
      multiplier = statInfo.multiplier;
    } 

    var rootRT = CreateRectTransform(statInfo.longname, parentRT);
    var fg   = rootRT.gameObject.AddComponent<FusionGraph>();
    fg.Initialize(statId, rootRT, goodColor, badColor, multiplier.Value, decimals.HasValue ? decimals.Value : 2);

    _graphs[(int)statId] = fg;

    fg.Runner = _activeRunner;

    if (((int)_includedStats & (1 << (int)statId)) == 0) {
      fg.gameObject.SetActive(false);
    }

    return fg;
  }

  // returns true if a graph has been added.
  void ReapplyEnabled() {

    _activeDirty = false;

    if (_graphs == null || _graphs.Length < 0) {
      return;
    }

    // This is null if the children were deleted. Stop execution, or new Graphs will be created without a parent.
    if (_statsLayoutRT == null) {
      return;
    }

    for (int i = 0; i < _graphs.Length; ++i) {
      var graph = _graphs[i];
      if (graph == null) {
        graph = CreateGraph((Stats.FusionStats)i, _statsLayoutRT, GraphColor, GraphColor);
        _graphs[i] = graph;
      }
      if (_graphs[i] != null) {
        bool enabled = ((Stats.FusionStatFlags)(1 << (int)graph.StatId) & _includedStats) != 0;
        graph.gameObject.SetActive(enabled);
      }
    }
  
  }

  void CalculateLayout() {

    _layoutDirty = false;
    if (_rootPanelRT) {
      _rootPanelRT.anchorMax          = new Vector2(_position.xMax, _position.yMax);
      _rootPanelRT.anchorMin          = new Vector2(_position.xMin, _position.yMin);
      _rootPanelRT.sizeDelta          = new Vector2(0.0f, 0.0f);
      _rootPanelRT.pivot              = new Vector2(0.5f, 0.5f);
      _rootPanelRT.anchoredPosition3D = default;

      _headerRT.anchorMin             = new Vector2(0.0f, 1);
      _headerRT.anchorMax             = new Vector2(1.0f, 1);
      _headerRT.pivot                 = new Vector2(0.5f, 1);
      _headerRT.anchoredPosition3D    = default;
      _headerRT.sizeDelta             = new Vector2(0, TITLE_HEIGHT + _buttonHeight);

      // Cache this
      _titleRT.offsetMin = new Vector2(MARGIN, Math.Max(MARGIN, ButtonHeight));

      _statsRT.anchorMax              = new Vector2(1, 1);
      _statsRT.anchorMin              = new Vector2(0, 0);
      _statsRT.anchoredPosition3D     = default;
      _statsRT.pivot                  = new Vector2(0.5f, 0);
      _statsRT.sizeDelta              = new Vector2(0.0f, -(TITLE_HEIGHT + _buttonHeight));


      if (_graphs != null && _graphs.Length > 0) {
        for (int i = 0; i < _graphs.Length; ++i) {
          var graph = _graphs[i];
          if (graph == null) {
            continue;
          }
          graph.CalculateLayout();
        }
      }
    }
  }


  NetworkRunner FindActiveRunner(SimulationModes? mode = null) {

    var enumerator = NetworkRunner.GetInstancesEnumerator();
    while (enumerator.MoveNext()) {
      var found = enumerator.Current;
      if (found && found.IsRunning) {
        if (mode.HasValue && (mode.Value & found.Mode) == 0) {
          continue;
        }
        ActiveRunner = found;
        return found;
      }
    }

    ActiveRunner = null;
    return null;
  }

  void ApplyDefaultLayout(DefaultLayouts defaults) {
    switch (defaults) {
      case DefaultLayouts.Left: {
          Position = Rect.MinMaxRect(0.0f, 0.0f, 0.3f, 1.0f);
          break;
        }
      case DefaultLayouts.Right: {
          Position = Rect.MinMaxRect(0.7f, 0.0f, 1.0f, 1.0f);
          break;
        }
      default: {
          // Use existing serialized editor settings.
          break;
        }
    }
    _layoutDirty = true;
  }

}