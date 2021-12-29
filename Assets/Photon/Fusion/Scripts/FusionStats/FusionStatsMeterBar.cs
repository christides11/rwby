using Fusion;
using UnityEngine;
using UnityEngine.UI;
using Stats = Fusion.Simulation.Statistics;
using Fusion.StatsInternal;

public class FusionStatsMeterBar : FusionGraphBase
{
  public float WarningThreshold;
  public float ErrorThreshold;

  //public Text TitleLabel;
  public Text ValueLabel;
  public Image Mask;
  public Image Bar;

  public Color GoodColor  = new Color(0.0f, 0.5f, 0.0f, 1.0f);
  public Color WarnColor  = new Color(0.5f, 0.5f, 0.0f, 1.0f);
  public Color ErrorColor = new Color(0.5f, 0.0f, 0.0f, 1.0f);

  public string CurrentLabel;
  public double  _currentRawValue;
  public double  _currentDisplayValue;
  public double  _currentBarValue;
  public Color  CurrentColor;

  public float DecayTime = 0.25f;
  public float HoldPeakTime = 0.1f;

  protected override Color BackColor => base.BackColor * new Color(.5f, .5f, .5f, 1);

  public void OnValidate() {

    if (Application.isPlaying == false) {
      TryConnect();
      _layoutDirty = true;
    }
  }

  public override void Initialize() {
    base.Initialize();

    // Prefabs lose editor generated sprites - recreate as needed.
    if (BackImage.sprite == null) {

      BackImage.sprite = FusionStatsUtilities.MeterSprite;
      Bar.sprite = BackImage.sprite;
      Mask.sprite = BackImage.sprite;
    }
  }

  double _lastImportedSampleTickTime;
  double _max;
  double _total;
  float  _lastPeakSetTime;

  public override void Refresh() {
    if (_layoutDirty) {
      CalculateLayout();
    }

    var statsBuffer = StatsBuffer;
    if (statsBuffer == null || statsBuffer.Count < 1) {
      return;
    }

    // Awkward temp RPC handling
    if (statsBuffer.DefaultVisualization == FusionGraphVisualization.CountHistogram) {

      if (statsBuffer.Count > 0) {
    
        int highestRpcsFoundForTick = 0;
        float newestSampleTick = statsBuffer.GetSampleAtIndex(statsBuffer.Count - 1).TickValue;
        var tick = newestSampleTick;
        // Only look back at ticks we have not yet already looked at on previous updates.
        if (newestSampleTick > _lastImportedSampleTickTime) {
          int tickRpcCount = 0;
          for (int i = statsBuffer.Count - 1; i >= 0; i--) {
            var sampletick = statsBuffer.GetSampleAtIndex(i).TickValue;

            if (sampletick > _lastImportedSampleTickTime) {
              // If we are now looking at samples from a different tick that previous for loop, reset to get count for this tick now.
              if (sampletick != tick) {
                tick = sampletick;
                // Capture the RPC count for the last recorded tick if it is the new high.
                if (tickRpcCount > highestRpcsFoundForTick) {
                  highestRpcsFoundForTick = tickRpcCount;
                }
                tickRpcCount = 0;
              }
              tickRpcCount++;
              _total++;
            } else {
              break;
            }
          }
          _lastImportedSampleTickTime = newestSampleTick;
        }

        SetValue(highestRpcsFoundForTick);
      }
      return;
    }

    if (statsBuffer.Count > 0) {
      var value = statsBuffer.GetSampleAtIndex(statsBuffer.Count - 1);
      if (value.TickValue == _fusionStats.Runner.Simulation.LatestServerState.Tick) {
        SetValue(value.FloatValue);
      } else {
        SetValue(0);
      }
    }
  }

  public void LateUpdate() {

    if (DecayTime <= 0) {
      return;
    }

    if (_currentBarValue <= 0) {
      return;
    }

    if (Time.time < _lastPeakSetTime + HoldPeakTime) {
      return;
    }

    double decayedVal = System.Math.Max(_currentBarValue - Time.deltaTime / DecayTime * _max, 0);
    SetBar(decayedVal);

  }

  public void SetValue(double rawvalue) {

    var info = _dataSourceInfo;

    double multiplied = rawvalue * info.Multiplier;

    if (multiplied > _max) {
      _max = multiplied;
    }

    double clampedValue = System.Math.Max(System.Math.Min(multiplied, _max), 0);
    var roundedValue = System.Math.Round(clampedValue, _dataSourceInfo.Decimals);
    var newDisplayValue = _total > 0 ? _total : roundedValue;

    if (clampedValue >= _currentBarValue) {
      _lastPeakSetTime = Time.time;
    }

    if (newDisplayValue != _currentDisplayValue) {
      ValueLabel.text = _total > 0 ? _total.ToString() : clampedValue.ToString();
      _currentDisplayValue = newDisplayValue;
    }

    // Only set values greater than the current shown value when using decay.
    if (DecayTime >= 0 && clampedValue <= _currentBarValue) {
      return;
    }

    if (clampedValue != _currentBarValue) {
      SetBar(clampedValue);
    }

  }

  void SetBar(double value) {

    Mask.fillAmount = (float)(value / _max);
    _currentBarValue = value;

    if (value >= ErrorThreshold) {
      if (CurrentColor != ErrorColor) {
        Bar.color = ErrorColor;
        CurrentColor = ErrorColor;
      }
    } else if (value >= WarningThreshold) {
      if (CurrentColor != WarnColor) {
        Bar.color = WarnColor;
        CurrentColor = WarnColor;
      }
    } else {
      if (CurrentColor != GoodColor) {
        CurrentColor = GoodColor;
        Bar.color = GoodColor;
      }
    }
  }

  public override void CalculateLayout() {

    _layoutDirty = false;

    // Special padding handling because Arial vertically sits below center.
    var pad = LabelTitle.transform.parent.GetComponent<RectTransform>().rect.height * .2f;
    LabelTitle.rectTransform.offsetMax = new Vector2(0, -pad);
    LabelTitle.rectTransform.offsetMin = new Vector2(PAD, pad * 1.2f);

    ValueLabel.rectTransform.offsetMax = new Vector2(-PAD, -pad);
    ValueLabel.rectTransform.offsetMin = new Vector2(0, pad * 1.2f);

    ApplyTitleText();
  }

  // unfinished
  public static FusionStatsMeterBar Create(
    RectTransform parent, 
    IFusionStats iFusionStats,
    Stats.StatSourceTypes statSourceType, 
    int statId, 
    float warnThreshold,
    float alertThreshold
    ) {

    var info = Stats.GetDescription(statSourceType, statId);
    var barRT = parent.CreateRectTransform(info.LongName, true);
    var bar   = barRT.gameObject.AddComponent<FusionStatsMeterBar>();
    bar._dataSourceInfo = info;
    bar._fusionStats = iFusionStats;
    //bar.Multiplier   = info.multiplier;
    //bar.PerFlags = info.per;
    bar.WarningThreshold = warnThreshold;
    bar.ErrorThreshold = alertThreshold;
    bar._statSourceType = statSourceType;
    bar._statId = statId;
    bar.GenerateMeter();
    return bar;
  }

  public void GenerateMeter() {

    var info = Stats.GetDescription(_statSourceType, _statId);
    var backRT = transform.CreateRectTransform("Back", true);
    BackImage = backRT.gameObject.AddComponent<Image>();
    BackImage.sprite = FusionStatsUtilities.MeterSprite;
    BackImage.color = BackColor;
    BackImage.type = Image.Type.Tiled;
    var maskRT  = transform.CreateRectTransform("Mask", true);
    var barRT = maskRT.CreateRectTransform("Bar", true);
    Bar = barRT.gameObject.AddComponent<Image>();
    Bar.sprite = BackImage.sprite;
    Bar.color = GoodColor;
    Bar.type = Image.Type.Tiled;

    var texture = new Texture2D(2, 2);
        texture.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        texture.Apply();

    Mask = maskRT.gameObject.AddComponent<Image>();
    Mask.sprite = Sprite.Create(texture,new Rect(0,0,2,2), new Vector2(.5f,.5f));
    Mask.type = Image.Type.Filled;
    Mask.fillMethod = Image.FillMethod.Horizontal;
    Mask.fillAmount = 0;
    maskRT.gameObject.AddComponent<Mask>().showMaskGraphic = false;

    var titleRT = transform.CreateRectTransform("Label", true)
      .ExpandAnchor()
      .SetAnchors(0.0f, 0.5f, 0.0f,1.0f)
      .SetOffsets(6, -6, 6, -6);

    LabelTitle = titleRT.AddText(info.LongName, TextAnchor.MiddleLeft, _fusionStats.FontColor);
    LabelTitle.alignByGeometry = false;

    var valueRT = transform.CreateRectTransform("Value", true)
      .ExpandAnchor()
      .SetAnchors(0.5f, 1.0f, 0.0f, 1.0f)
      .SetOffsets(6, -6, 6, -6);
    
    ValueLabel = valueRT.AddText("0.0", TextAnchor.MiddleRight, _fusionStats.FontColor);
    ValueLabel.alignByGeometry = false;

  }

}
