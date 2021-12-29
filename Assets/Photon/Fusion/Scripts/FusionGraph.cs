using System;
using UnityEngine;
using Fusion;
using UI = UnityEngine.UI;
using Stats = Fusion.Simulation.Statistics;
using Fusion.StatsInternal;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FusionGraph : FusionGraphBase {

  public enum Layouts {
    Auto,
    Full,
    CenteredBasic,
    Compact
  }

  public enum ShowGraphOptions {
    Never,
    OverlayOnly,
    Always,
  }

  const int GRPH_TOP_PAD = 36;
  const int GRPH_BTM_PAD = 36;
  const int HIDE_XTRAS_WDTH = 200;
  const int INTERMITTENT_DATA_ARRAYSIZE = 128;

  const int EXPAND_GRPH_THRESH = GRPH_BTM_PAD + GRPH_TOP_PAD + 40;
  const int COMPACT_THRESH = GRPH_BTM_PAD + GRPH_TOP_PAD - 20;

  static Shader Shader {
    get => Resources.Load<Shader>("FusionGraphShader");
  }

  public float Padding = 5f;

  [SerializeField] public UI.Image GraphImg;
  [SerializeField] public UI.Text LabelMin;
  [SerializeField] public UI.Text LabelMax;
  [SerializeField] public UI.Text LabelAvg;
  [SerializeField] public UI.Text LabelLast;
  [SerializeField] public UI.Text LabelPer;

  [SerializeField] [HideInInspector] UI.Dropdown _viewDropdown;
  [SerializeField] [HideInInspector] UI.Button   _avgBttn;

  [SerializeField] public float  Height = 50;

  [SerializeField] [HideInInspector] RectTransform _rt;

  [SerializeField]
  Layouts _layout;
  public Layouts Layout {
    get => _layout;
    set {
      _layout = value;
      CalculateLayout();
    }
  }

  [SerializeField]
  ShowGraphOptions _showGraph = ShowGraphOptions.OverlayOnly;
  public ShowGraphOptions ShowGraph {
    get => _showGraph;
    set {
      _showGraph = value;
      CalculateLayout();
      _layoutDirty = true;
    }
  }

  [SerializeField]
  bool _alwaysExpandGraph;
  public bool AlwaysExpandGraph {
    get => _alwaysExpandGraph;
    set {
      _alwaysExpandGraph = value;
      CalculateLayout();
      _layoutDirty = true;
    }
  }

  float _min;
  float _max;
  float[] _values;
  float[] _intensity;
  float[] _histogram;

#if UNITY_EDITOR
  private void OnValidate() {
    if (Application.isPlaying == false) {
      //This is here so when changes are made that affect graph names/colors they get applied immediately.
      TryConnect();
    }

#if UNITY_EDITOR
    if (Selection.activeGameObject == gameObject) {
      UnityEditor.EditorApplication.delayCall += CalculateLayout;
    }
#endif
    _layoutDirty = true;
  }
#endif

  List<int> DropdownLookup = new List<int>();

  protected override bool TryConnect() {
    if (base.TryConnect()) {

      var flags = _statsBuffer.VisualizationFlags;

      DropdownLookup.Clear();
      _viewDropdown.ClearOptions();
      for (int i = 0; i < 16; ++i) {
        if (((int)flags & (1 << i)) != 0) {
          DropdownLookup.Add(1 << i);
          _viewDropdown.options.Add(new UI.Dropdown.OptionData(FusionStatsUtilities.CachedTelemetryNames[i + 1]));
          if ((1 << i & (int)_statsBuffer.DefaultVisualization) != 0) {
            _viewDropdown.value = i - 1;
          }
        }
      }
      SetPerText();
      return true;
    }
    return false;
  }

  [InlineHelp]
  FusionGraphVisualization _graphVisualization;
  public FusionGraphVisualization GraphVisualization {
    set {
      _graphVisualization = value;
      Reset();
    }
  }

  private void Reset() {
    _values = null;
    _histogram = null;
    _intensity = null;
    _min = 0;
    _max = 0;

    ResetGraphShader();
  }

  private void Awake() {
    _rt = GetComponent<RectTransform>();
  }

  public void Clear() {
    if (_values != null && _values.Length > 0) {
      Array.Clear(_values, 0, _values.Length);
      Array.Clear(_histogram, 0, _histogram.Length);
      Array.Clear(_intensity, 0, _intensity.Length);
      _min = 0;
      _max = 0;
      _histoHighestUsedBucketIndex = 0;
      _histoHighestValue = 0;
      _histoAvg = 0;
      _histoAvgSampleCount = 0;
    }
  }

  public override void Initialize() {

    _viewDropdown?.onValueChanged.AddListener(OnDropdownChanged);
    _avgBttn?.onClick.AddListener(CyclePer);
  }

  public void OnDropdownChanged(int value) {
    GraphVisualization = (FusionGraphVisualization)DropdownLookup[value];
    SetPerText();
  }

  void ResetGraphShader() {
    if (GraphImg) {
      GraphImg.material = new Material(Shader);
      GraphImg.material.SetColor("_GoodColor", _fusionStats.GraphColorGood);
      GraphImg.material.SetColor("_BadColor", _fusionStats.GraphColorBad);
      GraphImg.material.SetInt("EndCap", 0);
      GraphImg.material.SetInt("AnimateInOut", 0);
    }
  }

  public override void CyclePer() {
    if (_graphVisualization != FusionGraphVisualization.CountHistogram && _graphVisualization != FusionGraphVisualization.ValueHistogram) {
      base.CyclePer();
      SetPerText();
    }
  }

  void SetPerText() {
    // TODO: Temporary - here to avoid breaking existing implementations. Can be removed. Added Dec 15 2021
    if (LabelPer == null) {
      var prt = LabelAvg.rectTransform.parent.CreateRectTransform("Per")
      .SetAnchors(0.3f, 0.7f, 0.0f, 0.125f)
      .SetOffsets(MRGN, -MRGN, MRGN, 0.0f);
      LabelPer = prt.AddText("per sample", TextAnchor.LowerCenter, _fusionStats.FontColor);
    }

    LabelPer.text =
      // Histograms only show avg per sample
      (_graphVisualization == FusionGraphVisualization.ValueHistogram | _graphVisualization == FusionGraphVisualization.CountHistogram) ? "avg per Sample" :
      CurrentPer == Stats.StatsPer.Second ? "avg per Second" :
      CurrentPer == Stats.StatsPer.Tick   ? "avg per Tick" :
                                            "avg per Sample";
  }
  /// <summary>
  /// Returns true if the graph rendered. False if the size was too small and the graph was hidden.
  /// </summary>
  /// <returns></returns>
  public override void Refresh() {

    if (_layoutDirty) {
      CalculateLayout();
    }

    var statsBuffer = StatsBuffer;
    if (statsBuffer == null || statsBuffer.Count < 1) {
      return;
    }

    var visualization = _graphVisualization == FusionGraphVisualization.Auto ? _statsBuffer.DefaultVisualization : _graphVisualization;

    // TODO: move initialization of this to TryConnect?
    if (_values == null) {
      int size =
        visualization == FusionGraphVisualization.ContinuousTick ? statsBuffer.Capacity :
        visualization == FusionGraphVisualization.ValueHistogram ? _histoBucketCount + 3 :
        INTERMITTENT_DATA_ARRAYSIZE;

      _values    = new float[size];
      _histogram = new float[size];
      _intensity = new float[size];
    }


    switch (visualization) {
      case FusionGraphVisualization.ContinuousTick: {
          UpdateContinuousTick(ref statsBuffer);
          break;
        }
      case FusionGraphVisualization.IntermittentTick: {
          UpdateIntermittentTick(ref statsBuffer);
          break;
        }
      case FusionGraphVisualization.IntermittentTime: {
          UpdateIntermittentTime(ref statsBuffer);
          break;
        }
      case FusionGraphVisualization.CountHistogram: {
          //UpdateIntermittentTime(data);
          break;
        }
      case FusionGraphVisualization.ValueHistogram: {
          UpdateTickValueHistogram(ref statsBuffer);
          break;
        }
    }
  }

  void UpdateContinuousTick(ref IStatsBuffer data) 
    {
    var min = float.MaxValue;
    var max = float.MinValue;
    var avg = 0f;
    var last = 0f;

    for (int i = 0; i < data.Count; ++i) {
      var v = (float)(_dataSourceInfo.Multiplier * data.GetSampleAtIndex(i).FloatValue);

      min = Math.Min(v, min);
      max = Math.Max(v, max);

      if (i >= _values.Length)
        Debug.LogWarning(name + " Out of range " + i + " " + _values.Length + " " + data.Count);
      _values[i] = last = v;

      avg += v;
    }

    avg /= data.Count;

    ApplyScaling(ref min, ref max);
    UpdateUiText(min, max, avg, last);
  }

  // Intermittent Tick pulls values from a very short buffer, so we collect those values and merge them into a larger cache.
  (int tick, float value)[] _cachedValues;
  int _lastCachedTick;

  void UpdateIntermittentTick(ref IStatsBuffer data) {

    if (_cachedValues == null) {
      _cachedValues = new (int, float)[INTERMITTENT_DATA_ARRAYSIZE];
    }

    int latestServerStateTick = _fusionStats.Runner.Simulation.LatestServerState.Tick;

    var min = float.MaxValue;
    var max = float.MinValue;
    var sum = 0f;
    var last = 0f;

    var oldestAllowedBufferedTick = latestServerStateTick - INTERMITTENT_DATA_ARRAYSIZE + 1;

    var tailIndex = latestServerStateTick % INTERMITTENT_DATA_ARRAYSIZE;
    var headIndex = (tailIndex + 1) % INTERMITTENT_DATA_ARRAYSIZE;

    int gapcheck = _lastCachedTick;
    // Copy all data from the buffer into our larger intermediate cached buffer
    for (int i = 0; i < data.Count; ++i) {
      var sample = data.GetSampleAtIndex(i);
      var sampleTick = sample.TickValue;
      
      // sample on buffer is older than the range we are displaying.
      if (sampleTick < oldestAllowedBufferedTick) {
        gapcheck = sampleTick;
        continue;
      }

      // sample on the buffer has already been merged into cached buffer.
      if (sampleTick <= _lastCachedTick) {
        gapcheck = sampleTick;
        continue;
      }

      // Fill any gaps in the buffer data 
      var gap = sampleTick - gapcheck;
      for (int g = gapcheck + 1; g < sampleTick; ++g) {
        _cachedValues[g % INTERMITTENT_DATA_ARRAYSIZE] = (g, 0);
      }

      _lastCachedTick = sampleTick;
      _cachedValues[sampleTick % INTERMITTENT_DATA_ARRAYSIZE] = (sampleTick, (float)(_dataSourceInfo.Multiplier * sample.FloatValue));

      gapcheck = sampleTick;
    }

    // Loop through once to determine scaling
    for (int i = 0; i < INTERMITTENT_DATA_ARRAYSIZE; ++i) {
      var sample = _cachedValues[(i + headIndex) % INTERMITTENT_DATA_ARRAYSIZE];
      var v = sample.value;
      // Any outdated values are ticks that had no data, set them to zero.
      if (sample.tick < oldestAllowedBufferedTick) {
        sample.tick = oldestAllowedBufferedTick + i;
        sample.value = v = 0;
      }

      min = Math.Min(v, min);
      max = Math.Max(v, max);

      _values[i] = last = v;

      sum += v;
    }

    var avg = GetIntermittentAverageInfo(ref data, sum);
    ApplyScaling(ref min, ref max);
    UpdateUiText(min, max, avg, last);

  }

  void UpdateIntermittentTime(ref IStatsBuffer data) {
    var min = float.MaxValue;
    var max = float.MinValue;
    var sum = 0f;
    var last = 0f;

    for (int i = 0; i < data.Count; ++i) {
      var v = (float)(_dataSourceInfo.Multiplier * data.GetSampleAtIndex(i).FloatValue);

      min = Math.Min(v, min);
      max = Math.Max(v, max);

      _values[i] = last = v;

      sum += v;
    }
    var avg = GetIntermittentAverageInfo(ref data, sum);

    ApplyScaling(ref min, ref max);
    UpdateUiText(min, max, avg, last);
  }


  void ApplyScaling(ref float min, ref float max) {

    if (min > 0) {
      min = 0;
    }

    if (max > _max) {
      _max = max;
    }

    if (min < _min) {
      _min = min;
    }

    var r = _max - _min;

    for (int i = 0, len = _values.Length; i < len; ++i) {
      _values[i] = Mathf.Clamp01((_values[i] - _min) / r);
    }
  }

  void UpdateUiText(float min, float max, float avg, float last) {

    var decimals = _dataSourceInfo.Decimals;
    // TODO: At some point this label null checks should be removed
    if (LabelMin) { LabelMin.text = Math.Round(min, decimals).ToString(); }
    if (LabelMax) { LabelMax.text = Math.Round(max, decimals).ToString(); }
    if (LabelAvg) { LabelAvg.text = Math.Round(avg, decimals).ToString(); }
    if (LabelLast) { LabelLast.text = Math.Round(last, decimals).ToString(); }

    if (GraphImg && GraphImg.enabled) {
      GraphImg.material.SetFloatArray("_Data", _values);
      GraphImg.material.SetFloat("_Count", _values.Length);
      GraphImg.material.SetFloat("_Height", Height);
    }

    _min = Mathf.Lerp(_min, 0, Time.deltaTime);
    _max = Mathf.Lerp(_max, 1, Time.deltaTime);
  }


  float GetIntermittentAverageInfo(ref IStatsBuffer data, float sum) {

    switch (CurrentPer) {
      case Stats.StatsPer.Second: {
          var oldestTimeRecord = data.GetSampleAtIndex(0).TimeValue;
          var currentTime = (float)_fusionStats.Runner.Simulation.LatestServerState.Time;
          var avg = sum / (currentTime - oldestTimeRecord);
          return avg;
        }

      case Stats.StatsPer.Tick: {
          var oldestTickRecord = data.GetSampleAtIndex(0).TickValue;
          var currentTick = (float)_fusionStats.Runner.Simulation.LatestServerState.Tick;
          var avg = sum / (currentTick - oldestTickRecord);
          return avg;
        }

      default: {
          var avg = sum / _values.Length; // data.Count;
          return avg;
        }
    }
  }

  int    _histoBucketCount    = 1024 - 4;
  int    _histoBucketMaxValue = 1020;
  int    _histoHighestUsedBucketIndex;
  int    _histoAvgSampleCount;
  double _histoStep;
  double _histoHighestValue;
  double _histoAvg;

  void UpdateTickValueHistogram(ref IStatsBuffer data) {

    // Determine histogram bucketsizes if they haven't yet been determined.
    if (_histoStep == 0) {
      _histoStep = (double)_histoBucketCount / _histoBucketMaxValue;
    }

    int latestServerStateTick = _fusionStats.Runner.Simulation.LatestServerState.Tick;
    int mostCurrentBufferTick = data.GetSampleAtIndex(data.Count - 1).TickValue;

    // count non-existent ticks as zero values
    if (mostCurrentBufferTick < latestServerStateTick) {
      int countbackto = Math.Max(mostCurrentBufferTick, _lastCachedTick);
      int newZeroCount = latestServerStateTick - countbackto;
      float zerocountTotal = _histogram[0] + newZeroCount;
      _histogram[0] = zerocountTotal;

      if (zerocountTotal > _max) {
        _max = zerocountTotal;
      }
    }

    double multiplier = _dataSourceInfo.Multiplier;
    // Read data in stat buffer backwards until we reach a tick already recorded
    for (int i = data.Count - 1; i >= 0; --i) {
      var v = (float)(multiplier * data.GetSampleAtIndex(i).FloatValue);

      var sample = data.GetSampleAtIndex(i);
      var tick = sample.TickValue;

      if (tick <= _lastCachedTick) {
        break;
      }

      var val = sample.FloatValue * multiplier;

      int bucketIndex;
      if (val == 0) {
        bucketIndex = 0;
      }
      else if (val == _histoBucketMaxValue) {
        bucketIndex = _histoBucketCount;

      } 
      else if (val > _histoBucketMaxValue) {
        bucketIndex = _histoBucketCount + 1;
      }      
      else {
        bucketIndex = (int)(val * _histoStep) + 1;
      }

      _histoAvg = (_histoAvg * _histoAvgSampleCount + val) / (++_histoAvgSampleCount);

      var newval = _histogram[bucketIndex] + 1;
      
      if (newval > _max) {
        _max = newval;
      }
      _histogram[bucketIndex] = newval;


      if (val > _histoHighestValue) {
        _histoHighestValue = val;
        _histoHighestUsedBucketIndex = bucketIndex;
      }
    }

    int medianIndex = 0;
    float mostValues = 0;
    {
      var r = (_max - _min) * 1.1f;

      // Loop again to apply scaling
      for (int i = 0, cnt = _histogram.Length; i < cnt; ++i) {
        var value = _histogram[i];
        _intensity[i] = 0;
        if (i != 0 && value > mostValues) {
          mostValues = value;
          medianIndex = i;
        }
        _values[i] = Mathf.Clamp01((_histogram[i] - _min) / r);
      }
    }

    _intensity[medianIndex] = 1f;

    _lastCachedTick = latestServerStateTick;

    if (GraphImg && GraphImg.enabled) {
      GraphImg.material.SetFloatArray("_Data", _values);
      GraphImg.material.SetFloatArray("_Intensity", _intensity);
      GraphImg.material.SetFloat("_Count", _histoHighestUsedBucketIndex);
      GraphImg.material.SetFloat("_Height", Height);
    }

    _min = 0;

    var decimals = _dataSourceInfo.Decimals;
    LabelMax.text  = $"<color=yellow>{Math.Ceiling((medianIndex + 1) * _histoStep)}</color>";
    LabelAvg.text  = Math.Round(_histoAvg, decimals).ToString();
    LabelMin.text  = Math.Floor(_min).ToString();
    LabelLast.text = Math.Round(_histoHighestValue - 2, decimals).ToString();
  }

  /// <summary>
  /// Creates a new GameObject with <see cref="FusionGraph"/> and attaches it to the specified parent.
  /// </summary>
  public static FusionGraph Create(IFusionStats iFusionStats, Stats.StatSourceTypes statSourceType, int statId, RectTransform parentRT) {
    
    var statInfo = Stats.GetDescription(statSourceType, statId);

    var rootRT = parentRT.CreateRectTransform(statInfo.LongName);
    var graph = rootRT.gameObject.AddComponent<FusionGraph>();
    graph._fusionStats = iFusionStats;
    graph.Generate(statSourceType, (int)statId, rootRT);

    return graph;
  }

  /// <summary>
  /// Generates the Graph UI for this <see cref="FusionGraph"/>.
  /// </summary>
  public void Generate(Stats.StatSourceTypes type, int statId, RectTransform root) {

    _statSourceType = type;

    if (_rt == null) {
      _rt = GetComponent<RectTransform>();
    }
    _statId = statId;

    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.anchoredPosition3D = default;

    var background = root.CreateRectTransform("Background")
      .ExpandAnchor();

    BackImage = background.gameObject.AddComponent<UI.Image>();
    BackImage.color = BackColor;

    var graphRT = background.CreateRectTransform("Graph")
      .SetAnchors(0.0f, 1.0f, 0.2f, 0.8f)
      .SetOffsets(0.0f, 0.0f, 0.0f, 0.0f);

    GraphImg = graphRT.gameObject.AddComponent<UI.Image>();
    ResetGraphShader();

    var fontColor = _fusionStats.FontColor;
    var fontColorDim = _fusionStats.FontColor * new Color(1, 1, 1, 0.5f);

    var titleRT = root.CreateRectTransform("Title")
      .ExpandAnchor()
      .SetOffsets(PAD, -PAD, 0.0f, -2.0f);
    titleRT.anchoredPosition = new Vector2(0, 0);

    LabelTitle = titleRT.AddText(name, TextAnchor.UpperRight, fontColor);
    LabelTitle.resizeTextMaxSize = MAX_FONT_SIZE_WITH_GRAPH;

    // Top Left value
    var maxRT = root.CreateRectTransform("Max")
      .SetAnchors(0.0f, 0.3f, 0.85f, 1.0f)
      .SetOffsets(MRGN, 0.0f, 0.0f, -2.0f);
    LabelMax = maxRT.AddText("-", TextAnchor.UpperLeft, fontColorDim);

    // Bottom Left value
    var minRT = root.CreateRectTransform("Min")
      .SetAnchors(0.0f, 0.3f, 0.0f, 0.15f)
      .SetOffsets(MRGN, 0.0f, 0.0f, -2.0f);
    LabelMin = minRT.AddText("-", TextAnchor.LowerLeft, fontColorDim);

    // Main Center value
    var avgRT = root.CreateRectTransform("Avg")
      .SetOffsets(0.0f, 0.0f,  0.0f, 0.0f);
    avgRT.anchoredPosition = new Vector2(0, 0);
    LabelAvg = avgRT.AddText("-", TextAnchor.LowerCenter, fontColor);
    _avgBttn = avgRT.gameObject.AddComponent<UI.Button>();

    // Main Center value
    var perRT = root.CreateRectTransform("Per")
      .SetAnchors(0.3f, 0.7f, 0.0f, 0.125f)
      .SetOffsets(MRGN, -MRGN, MRGN, 0.0f);
    LabelPer = perRT.AddText("avg per Sample", TextAnchor.LowerCenter, fontColor);

    // Bottom Right value
    var _lstRT = root.CreateRectTransform("Last")
      .SetAnchors(0.7f, 1.0f, 0.0f,  0.15f)
      .SetOffsets(PAD, -PAD,  0.0f, -2.0f);
    LabelLast = _lstRT.AddText("-", TextAnchor.LowerRight, fontColorDim);

    _viewDropdown = titleRT.CreateDropdown(PAD, fontColor);

    _layoutDirty = true;
#if UNITY_EDITOR
    EditorUtility.SetDirty(this);
#endif

  }

  [BehaviourButtonAction("Update Layout")]
  public override void CalculateLayout() {
    // This Try/Catch is here to prevent errors resulting from a delayCall to this method when entering play mode.
    try {
      if (gameObject == null) {
        return;
      }
    } catch {
      return;
    }

    if (gameObject.activeInHierarchy == false) {
      return;
    }

    _layoutDirty = false;

    if (_rt == null) {
      _rt = GetComponent<RectTransform>();
    }

    if (_statsBuffer == null) {
      TryConnect();
    }
    ApplyTitleText();

    bool graphIsValid = _dataSourceInfo.InvalidReason == null;

    LabelMin.gameObject.SetActive(graphIsValid);
    LabelMax.gameObject.SetActive(graphIsValid);
    LabelAvg.gameObject.SetActive(graphIsValid);
    LabelPer.gameObject.SetActive(graphIsValid);

    if (!graphIsValid) {
      LabelTitle.rectTransform.ExpandAnchor(PAD);
      LabelTitle.alignment = TextAnchor.MiddleCenter;
      return;
    }

    bool isOverlayCanvas = _fusionStats != null && _fusionStats.CanvasType == FusionStats.StatCanvasTypes.Overlay;

    bool showGraph = ShowGraph == ShowGraphOptions.Always || (ShowGraph == ShowGraphOptions.OverlayOnly && isOverlayCanvas);

    var height = _rt.rect.height;
    var width  = _rt.rect.width;
    bool expandGraph = _alwaysExpandGraph || !showGraph || (_layout != Layouts.Full && height < EXPAND_GRPH_THRESH);
    bool isSuperShort = height < MRGN * 3;

    var graphRT = GraphImg.rectTransform;
    if (graphRT) {
      graphRT.gameObject.SetActive(showGraph);
      
      if (expandGraph) {
        graphRT.SetAnchors(0, 1, 0, 1);
      } else {
        graphRT.SetAnchors(0, 1, .25f, .8f);
      }
    }

    Layouts layout;
    if (_layout != Layouts.Auto) {
      layout = _layout;
    } else {
      if (height < COMPACT_THRESH) {
        layout = Layouts.Compact;
      } else {
        if (width < HIDE_XTRAS_WDTH) {
          layout = Layouts.CenteredBasic;
        } else {
          layout = Layouts.Full;
        }
      }
    }

    bool showExtras = layout == Layouts.Full /*|| (layout == Layouts.Compact && width > HIDE_XTRAS_WDTH)*/;

    var titleRT = LabelTitle.rectTransform;
    var avgRT = LabelAvg.rectTransform;

    // TODO: Temporary - here to avoid breaking existing implementations. Can be removed. Added Dec 15 2021
    if (LabelPer == null) {
      var prt = avgRT.parent.CreateRectTransform("Per")
      .SetAnchors(0.3f, 0.7f, 0.0f, 0.125f)
      .SetOffsets(MRGN, -MRGN, MRGN, 0.0f);
      LabelPer = prt.AddText("per sample", TextAnchor.LowerCenter, _fusionStats.FontColor);
    }

    var perRT = LabelPer.rectTransform;

    switch (layout) {
      case Layouts.Full: {
          titleRT.anchorMin = new Vector2(showExtras ? 0.3f : 0.0f, expandGraph ? 0.5f : 0.8f);
          titleRT.anchorMax = new Vector2(1.0f, 1.0f);
          titleRT.offsetMin = new Vector2(MRGN, MRGN);
          titleRT.offsetMax = new Vector2(-MRGN, -MRGN);
          LabelTitle.alignment = showExtras ? TextAnchor.UpperRight : TextAnchor.UpperCenter;

          avgRT.anchorMin = new Vector2(showExtras ? 0.3f : 0.0f, expandGraph ? 0.15f : 0.10f);
          avgRT.anchorMax = new Vector2(showExtras ? 0.7f : 1.0f, expandGraph ? 0.50f : 0.25f);
          avgRT.SetOffsets(0.0f, 0.0f, 0.0f, 0.0f);
          LabelAvg.alignment = TextAnchor.LowerCenter;

          perRT.SetAnchors(0.3f, 0.7f, 0.0f, expandGraph ? 0.2f : 0.1f);

          break;
        }
      case Layouts.CenteredBasic: {
          titleRT.anchorMin = new Vector2(showExtras ? 0.3f : 0.0f, 0.5f);
          titleRT.anchorMax = new Vector2(showExtras ? 0.7f : 1.0f, 1.0f);
          titleRT.offsetMin = new Vector2(showExtras ? 0.0f :  MRGN,  MRGN);
          titleRT.offsetMax = new Vector2(showExtras ? 0.0f : -MRGN, -MRGN);
          LabelTitle.alignment = TextAnchor.UpperCenter;

          avgRT.anchorMin = new Vector2(0.0f, expandGraph ? 0.15f : 0.10f);
          avgRT.anchorMax = new Vector2(1.0f, expandGraph ? 0.50f : 0.25f);
          avgRT.SetOffsets(MRGN, -MRGN, 0.0f, 0.0f);

          perRT.SetAnchors(0.0f, 1.0f, 0.0f, expandGraph ? 0.2f : 0.1f);

          LabelAvg.alignment = TextAnchor.LowerCenter;
          break;
        }
      case Layouts.Compact: {
          titleRT.anchorMin = new Vector2(0.0f, 0.0f);
          titleRT.anchorMax = new Vector2(0.5f, 1.0f);
          if (isSuperShort) {
            titleRT.SetOffsets(MRGN,     0, 0, 0);
            avgRT  .SetOffsets(MRGN, -MRGN, 0, 0);
          } else {
            titleRT.SetOffsets(MRGN, 0,  MRGN, -MRGN);
            avgRT  .SetOffsets(0, -MRGN, MRGN, -MRGN);
          }
          LabelTitle.alignment = TextAnchor.MiddleLeft;

          avgRT.SetAnchors(0.5f, 1.0f, 0.0f, 1.0f);

          LabelAvg.alignment = TextAnchor.MiddleRight;
          break;
        }
    }

    LabelMin.enabled = showExtras;
    LabelMax.enabled = showExtras;
    LabelLast.enabled = showExtras;
  }

}