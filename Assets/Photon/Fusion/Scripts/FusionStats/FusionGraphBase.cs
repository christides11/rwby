using System;
using UnityEngine;
using Fusion;
using UI = UnityEngine.UI;
using Stats = Fusion.Simulation.Statistics;
using Fusion.StatsInternal;

[ScriptHelp(BackColor = EditorHeaderBackColor.Olive)]
public abstract class FusionGraphBase : Fusion.Behaviour, IFusionStatsView {

  protected const int PAD = FusionStatsUtilities.PAD;
  protected const int MRGN = FusionStatsUtilities.MARGIN;
  protected const int MAX_FONT_SIZE_WITH_GRAPH = 24;

  protected Stats.StatSourceInfo _dataSourceInfo;

  //[SerializeField] [HideInInspector] public int Places = 0;
  [SerializeField] [HideInInspector] protected UI.Text  LabelTitle;
  [SerializeField] [HideInInspector] protected UI.Image BackImage;

  [SerializeField]
  protected Stats.StatSourceTypes _statSourceType;
  public Stats.StatSourceTypes StateSourceType {
    get => _statSourceType;
    set {
      _statSourceType = value;
      TryConnect();
    }
  }

  [SerializeField]
  [CastEnum(nameof(CastToStatType))]
  protected int _statId;
  public int StatId {
    get => _statId;
    set {
      _statId = value;
      TryConnect();
    }
  }

  protected IStatsBuffer _statsBuffer;
  public IStatsBuffer StatsBuffer {
    get {
      if (_statsBuffer == null) {
        TryConnect();
      }
      return _statsBuffer;
    }
  }

  protected bool _isOverlay;
  public bool IsOverlay {
    set {
      if (_isOverlay != value) {
        _isOverlay = value;
        CalculateLayout();
        _layoutDirty = true;
      }
    }
    get {
      return _isOverlay;
    }
  }

  protected virtual Color BackColor {
    get {
      if (_statSourceType == Stats.StatSourceTypes.Simulation) {
        return _fusionStats.SimDataBackColor;
      }
      if (_statSourceType == Stats.StatSourceTypes.NetConnection) {
        return _fusionStats.NetDataBackColor;
      }
      return _fusionStats.ObjDataBackColor;
    }
  }

  protected Type CastToStatType => 
    (_statSourceType == Stats.StatSourceTypes.Simulation)    ? typeof(Stats.SimStats) : 
    (_statSourceType == Stats.StatSourceTypes.NetConnection) ? typeof(Stats.NetStats) : 
                                                               typeof(Stats.ObjStats);

  protected IFusionStats _fusionStats;
  protected bool _layoutDirty = true;

  protected Stats.StatsPer CurrentPer;

  protected Stats.StatSourceInfo StatSourceInfo;

  public virtual void Initialize() {

  }

  public virtual void CyclePer() {

    var flags = _dataSourceInfo.PerFlags;
    switch (CurrentPer) {
      case Stats.StatsPer.Individual:
        if ((flags & Stats.StatsPer.Tick) == Stats.StatsPer.Tick) {
          CurrentPer = Stats.StatsPer.Tick;
        } else if ((flags & Stats.StatsPer.Second) == Stats.StatsPer.Second) {
          CurrentPer = Stats.StatsPer.Second;
        }
        return;

      case Stats.StatsPer.Tick:
        if ((flags & Stats.StatsPer.Second) == Stats.StatsPer.Second) {
          CurrentPer = Stats.StatsPer.Second;
        } else if ((flags & Stats.StatsPer.Individual) == Stats.StatsPer.Individual) {
          CurrentPer = Stats.StatsPer.Individual;
        }
        return;

      case Stats.StatsPer.Second:
        if ((flags & Stats.StatsPer.Individual) == Stats.StatsPer.Individual) {
          CurrentPer = Stats.StatsPer.Individual;
        } else if ((flags & Stats.StatsPer.Tick) == Stats.StatsPer.Tick) {
          CurrentPer = Stats.StatsPer.Tick;
        }
        return;

      default: 
        return;
    }
  }

  public abstract void CalculateLayout();

  public abstract void Refresh();

  protected virtual bool TryConnect() {

    if (gameObject.activeInHierarchy == false) {
      return false;
    }

    if (_fusionStats == null) {
      _fusionStats = GetComponentInParent<IFusionStats>();
    }

    // Any data connection requires a runner for the statistics source.
    var runner = _fusionStats?.Runner;

    var statistics = runner?.Simulation?.Stats;
    //Stats.StatSourceInfo info;

    StatSourceInfo = Stats.GetDescription(_statSourceType, _statId);

    switch (_statSourceType) {
      case Stats.StatSourceTypes.Simulation: {
          _statsBuffer = statistics?.GetStatBuffer((Stats.SimStats)_statId);
          break;
        }
      case Stats.StatSourceTypes.NetworkObject: {
          if (_statId >= Stats.OBJ_STAT_TYPE_COUNT) {
            StatId = 0;
          }
          if (_fusionStats.Object == null) {
            _statsBuffer = null;
            break;
          }

          _statsBuffer = statistics?.GetObjectBuffer(_fusionStats.Object.Id, (Stats.ObjStats)_statId, true);
          break;
        }
      case Stats.StatSourceTypes.NetConnection: {

          //StatSourceInfo = Stats.GetDescription((Stats.NetStats)_statId);
          if (runner == null) {
            _statsBuffer = null;
            break;
          }
          _statsBuffer = statistics?.GetStatBuffer((Stats.NetStats)_statId, runner);

          break;
        }
      default: {
          _statsBuffer = null;
          break;
        }
    }
    if (BackImage) {
      BackImage.color = BackColor;
    }

    // Update the labels, regardless if a connection can be made.
    if (LabelTitle) {
      CheckIfValidIncurrentMode(runner);
      ApplyTitleText();
    }

    _dataSourceInfo = StatSourceInfo;

    CurrentPer = StatSourceInfo.PerDefault;

    return (_statsBuffer != null);
  }

  protected void ApplyTitleText() {
    var info = StatSourceInfo;
    if (info.InvalidReason != null) {
      LabelTitle.text = info.InvalidReason;
      BackImage.gameObject.SetActive(false);
      LabelTitle.color = _fusionStats.FontColor * new Color(1, 1, 1, 0.2f);
    } else {
      var titleRT = LabelTitle.rectTransform;
      if (titleRT.rect.width < 100) {
        LabelTitle.text = info.ShortName ?? info.LongName;
      } else {
        LabelTitle.text = info.LongName;
      }
      BackImage.gameObject.SetActive(true);
    }
  }

  protected void CheckIfValidIncurrentMode(NetworkRunner runner) {
    if (runner == false) {
      return;
    }

    //var info = StatSourceInfo;
    var flags = StatSourceInfo.Flags;

    if ((flags & Stats.StatFlags.ValidForBuildType) == 0) {
      StatSourceInfo.InvalidReason = "DEBUG DLL ONLY";
      return;
    }

    var obj = _statSourceType == Stats.StatSourceTypes.NetworkObject ? _fusionStats?.Object : null;

    if (obj) {
      bool nonStateAuthOnly = (flags & Stats.StatFlags.ValidOnStateAuthority) == 0;
      if (nonStateAuthOnly && obj.HasStateAuthority) {
        StatSourceInfo.InvalidReason = "NON STATE AUTH ONLY";
        return;
      }
    }

    if (runner) {
      bool clientOnly = (flags & Stats.StatFlags.ValidOnServer) == 0;
      if (clientOnly && runner.IsClient == false) {
        StatSourceInfo.InvalidReason = "CLIENT ONLY";
        return;
      }
      bool ecOnly = (flags & Stats.StatFlags.ValidWithDeltaSnapshot) == 0;
      if (ecOnly && runner.Config.Simulation.ReplicationMode == SimulationConfig.StateReplicationModes.DeltaSnapshots) {
        StatSourceInfo.InvalidReason = "EC MODE ONLY";
        return;
      }
    }
  }
}
