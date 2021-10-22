using System;
using UnityEngine;
using Fusion;
using UI = UnityEngine.UI;
using Stats = Fusion.Simulation.Statistics;

public class FusionGraph : MonoBehaviour {

  const int FONT_SIZE              = 12;
  const int FONT_SIZE_MIN          = 0;
  const int FONT_EXTRA_SIZE_MAX    = 14;
  const int FONT_TITLE_SIZE_MAX    = 22;
  const int FONT_AVRGE_SIZE_MAX    = 28;
  const int GRPH_TOP_PAD           = 36;
  const int GRPH_BTM_PAD           = 36;
  const int HIDE_XTRAS_WDTH        = 200;

  const int EXPAND_GRPH_THRESH = GRPH_BTM_PAD + GRPH_TOP_PAD + 40;
  const int COMPACT_THRESH     = GRPH_BTM_PAD + GRPH_TOP_PAD - 20;
  
  const float LABEL_HEIGHT   = 0.25f;
  const float ALPHA          = 0.925f;

  static Shader Shader {
    get => Resources.Load<Shader>("FusionGraphShader");
  }

  private bool IsPlaying => Application.isPlaying;

  [DrawIf(nameof(IsPlaying), false)]
  public Color GraphColor = new Color(0f, 153f / 255f, 1f, 1f);
  [DrawIf(nameof(IsPlaying), false)]
  public Color FontColor = new Color(1, 1, 1, .5f);
  [DrawIf(nameof(IsPlaying), false)]
  public Color PanelColor = new Color(.25f, .25f, .25f, .8f);

  [SerializeField]
  Font _font;

  Font Font {
    get {
      if (_font == null) {
        _font = Font.CreateDynamicFontFromOSFont("Arial", FONT_SIZE);
      }
      return _font;
    }
  }

  float   _min;
  float   _max;
  float[] _values;

  [SerializeField] public UI.Image GraphImg;
  [SerializeField] public UI.Text LabelTitle;
  [SerializeField] public UI.Text LabelMin;
  [SerializeField] public UI.Text LabelMax;
  [SerializeField] public UI.Text LabelAvg;
  [SerializeField] public UI.Text LabelLast;

  [SerializeField] public int   Fractions  = 0;
  [SerializeField] public float Height     = 50; 
  [SerializeField] public float Multiplier = 1;

  [SerializeField] [HideInInspector] RectTransform _rt;
  [SerializeField] [HideInInspector] RectTransform _titleRT;
  [SerializeField] [HideInInspector] RectTransform _graphRT;
  [SerializeField] [HideInInspector] RectTransform _avgRT;

  [SerializeField]
  [HideInInspector]
  Stats.FusionStats _statId;
  public Stats.FusionStats StatId {
    get => _statId;
    set {
      _statId = value;
      TryConnectToRunner();
    }
  }

  private NetworkRunner _runner;
  public NetworkRunner Runner {
    get => _runner;
    set {
      _runner = value;
      TryConnectToRunner();
    }
  }

  private Stats.StatsBuffer<float> _statsBuffer;
  public Stats.StatsBuffer<float> StatsBuffer {
    get {
      if (_statsBuffer == null) {
        TryConnectToRunner();
      }
      return _statsBuffer;
    }
  }

  bool TryConnectToRunner() {
    if (_runner && _runner.Simulation != null) {
      var statistics = _runner.Simulation.Stats;
      if (statistics != null) {
        _statsBuffer = statistics.GetStatBuffer(_statId);
        return true;
      }
      _statsBuffer = null;
      return false;
    }
    _statsBuffer = null;
    return false;
  }

  private void Awake() {
    _rt = GetComponent<RectTransform>();
  }

  public void Initialize(Stats.FusionStats statId, RectTransform root, Color goodColor, Color badColor, float multiplier = 1, int fractions = 2) {

    if (_rt == null) {
      _rt = GetComponent<RectTransform>();
    }

    Fractions = fractions;
    Multiplier = multiplier;
    StatId = statId;

    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.anchoredPosition3D = default;

    var background = CreateRectTransform("Background", root);
    background.anchorMin = new Vector2(0, 0);
    background.anchorMax = new Vector2(1, 1);
    background.anchoredPosition = default;
    background.sizeDelta = default;

    var backgroundImage = background.gameObject.AddComponent<UI.Image>();
    backgroundImage.color = new Color(0, 0, 0, ALPHA);

    _graphRT = CreateRectTransform("Graph", root);
    _graphRT.anchorMin = new Vector2(0, 0);
    _graphRT.anchorMax = new Vector2(1, 1);
    _graphRT.anchoredPosition = default;
    _graphRT.offsetMin = new Vector2(0, GRPH_BTM_PAD);
    _graphRT.offsetMax = new Vector2(0, -GRPH_TOP_PAD);

    GraphImg = _graphRT.gameObject.AddComponent<UI.Image>();
    GraphImg.material = new Material(Shader);
    GraphImg.material.SetColor("_GoodColor", goodColor);
    GraphImg.material.SetColor("_BadColor", badColor);

    _titleRT = CreateRectTransform("Title", root);
    _titleRT.anchoredPosition = new Vector2(0, 0);
    LabelTitle = AddText(_titleRT.gameObject, name, TextAnchor.UpperRight);
    LabelTitle.resizeTextMaxSize = FONT_TITLE_SIZE_MAX;

    // Top Left value
    var max = CreateRectTransform("Max", root);
    max.anchorMin = new Vector2(0.0f, 1 - LABEL_HEIGHT);
    max.anchorMax = new Vector2(0.3f, 1);
    //max.anchoredPosition = new Vector2(0, 0);
    max.offsetMin = new Vector2( 10,  0);
    max.offsetMax = new Vector2(-10, -10);
    LabelMax = AddText(max.gameObject, "0", TextAnchor.UpperLeft);
    LabelMax.horizontalOverflow = HorizontalWrapMode.Overflow;

    var min = CreateRectTransform("Min", root);
    min.anchorMin = new Vector2(0.0f, 0);
    min.anchorMax = new Vector2(0.3f, LABEL_HEIGHT);
    //min.anchoredPosition = new Vector2(0, 0);
    min.sizeDelta = default;
    min.offsetMin = new Vector2(10, 10);
    min.offsetMax = new Vector2(0,  0);
    LabelMin = AddText(min.gameObject, "0", TextAnchor.LowerLeft);
    LabelMin.horizontalOverflow = HorizontalWrapMode.Overflow;

    _avgRT = CreateRectTransform("Avg", root);
    _avgRT.anchoredPosition = new Vector2(0, 0);
    LabelAvg = AddText(_avgRT.gameObject, "0", TextAnchor.LowerCenter);
    LabelAvg.resizeTextMaxSize = FONT_AVRGE_SIZE_MAX;

    var last = CreateRectTransform("Last", root);
    last.anchorMin = new Vector2(0.6f, 0);
    last.anchorMax = new Vector2(1.0f, LABEL_HEIGHT);
    //last.anchoredPosition = new Vector2(0, 0);
    last.offsetMin = new Vector2( 10,  10);
    last.offsetMax = new Vector2(-10f, 0);
    LabelLast = AddText(last.gameObject, "0", TextAnchor.LowerRight);
    LabelLast.horizontalOverflow = HorizontalWrapMode.Overflow;

#if UNITY_EDITOR
    if (Application.isPlaying == false) {
      UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif

    CalculateLayout();
  }

  public void CalculateLayout() {

    if (_rt == null) {
      _rt = GetComponent<RectTransform>();
    }

    var height = _rt.rect.height;
    var width  = _rt.rect.width;
    bool expandGraph = height < EXPAND_GRPH_THRESH;

    if (expandGraph) {
      _graphRT.offsetMin = new Vector2(0, 0);
      _graphRT.offsetMax = new Vector2(0, 1);
    } else {
      _graphRT.offsetMin = new Vector2(0, GRPH_BTM_PAD);
      _graphRT.offsetMax = new Vector2(0, -GRPH_TOP_PAD);
    }

    bool drawCompact = height < COMPACT_THRESH;
    bool showExtras  = width  > HIDE_XTRAS_WDTH;

    _titleRT.anchorMin   = new Vector2(drawCompact ? 0.0f : (showExtras ? 0.5f : 0), drawCompact ? 0 : 0.5f);
    _titleRT.anchorMax   = new Vector2(drawCompact ? 0.5f : 1, 1);
    _titleRT.offsetMin   = drawCompact ? new Vector2( 10,  5) : new Vector2( 10,  10);
    _titleRT.offsetMax   = drawCompact ? new Vector2(-10, -5) : new Vector2(-10, -10);
    LabelTitle.alignment = drawCompact ? TextAnchor.MiddleLeft : (showExtras ? TextAnchor.UpperRight : TextAnchor.UpperCenter) ;
    
    _avgRT.anchorMin     = new Vector2(drawCompact ? 0.5f : 0 , 0);
    _avgRT.anchorMax     = new Vector2(1.0f, drawCompact ? 1 : 0.5f);
    _avgRT.offsetMin     = drawCompact ? new Vector2( 10,  5) : new Vector2( 10,  10);
    _avgRT.offsetMax     = drawCompact ? new Vector2(-10, -5) : new Vector2(-10, -10);
    LabelAvg.alignment   = drawCompact ? TextAnchor.MiddleRight : TextAnchor.LowerCenter;

    LabelMin.enabled = showExtras;
    LabelMax.enabled = showExtras;
    LabelLast.enabled = showExtras;
  }

  public void Clear() {
    if (_values != null && _values.Length > 0) {
      Array.Clear(_values, 0, _values.Length);
    }
  }

  /// <summary>
  /// Returns true if the graph rendered. False if the size was too small and the graph was hidden.
  /// </summary>
  /// <returns></returns>
  public void Refresh() {

    var data = StatsBuffer;
    if (data == null || data.Count < 1) {
      return;
    }

    if (_values == null || _values.Length < data.Capacity) {
      _values = new float[data.Capacity];
    }

    var min  = float.MaxValue;
    var max  = float.MinValue;
    var avg  = 0f;
    var last = 0f;

    for (int i = 0; i < data.Count; ++i) {
      var v = Multiplier * data[i];
      
      min = Math.Min(v, min);
      max = Math.Max(v, max);

      _values[i] = last = v;

      avg += v;
    }

    avg /= data.Count;
    
    if (min > 0) {
      min = 0;
    }

    if (max > _max) {
      _max = max;
    }

    if (min < _min) {
      _min = min;
    }
    
    {
      var r = _max - _min;

      for (int i = 0; i < data.Count; ++i) {
        _values[i] = Mathf.Clamp01((_values[i] - _min) / r);
      }
    }

    if (LabelMin)  { LabelMin.text  = Math.Round(min,  Fractions).ToString(); }
    if (LabelMax)  { LabelMax.text  = Math.Round(max,  Fractions).ToString(); }
    if (LabelAvg)  { LabelAvg.text  = Math.Round(avg,  Fractions).ToString(); }
    if (LabelLast) { LabelLast.text = Math.Round(last, Fractions).ToString(); }

    if (GraphImg.enabled) {
      GraphImg.material.SetFloatArray("_Data", _values);
      GraphImg.material.SetFloat("_Count", _values.Length);
      GraphImg.material.SetFloat("_Height", Height);
    }
    
    _min = Mathf.Lerp(_min, 0, Time.deltaTime);
    _max = Mathf.Lerp(_max, 1, Time.deltaTime);
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

  static void ExpandAnchor(RectTransform rt) {
    rt.anchorMax = new Vector2(1, 1);
    rt.anchorMin = new Vector2(0, 0);
    rt.pivot = new Vector2(0.5f, 0.5f);
    rt.sizeDelta = default;
    rt.anchoredPosition = default;
  }

  UI.Text AddText(GameObject go, string label, TextAnchor anchor) {
    var text       = go.AddComponent<UI.Text>();
    text.text      = label;
    text.color     = FontColor;
    text.font      = Font;
    text.alignment = anchor;
    text.fontSize  = FONT_SIZE;

    text.resizeTextMinSize    = FONT_SIZE_MIN;
    text.resizeTextMaxSize    = FONT_EXTRA_SIZE_MAX;
    text.resizeTextForBestFit = true;
    text.alignByGeometry      = true;
    return text;
  }
}