using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Fusion {
  public partial class FusionUnityLogger : Fusion.ILogger {

    const string PreExceptionMessage = "## Code Threw Exception ##";
    const string ColorDarkSkin = "#63c5da";
    const string ColorLightSkin = "#000080";
    const string ColorRuntime = "#000080";
    
    static partial void InitializePartial(ref FusionUnityLogger logger);

    
    StringBuilder _builder = new StringBuilder();

    public bool UseGlobalPrefix { get; set;  }
    public bool UseColorTags { get; set;  }
    public string GlobalPrefixColor { get; set; }
    public Func<int, int> GenerateColor { get; set; }

    public void Log<T>(LogType logType, string prefix, ref T context, string message) where T : ILogBuilder {

      Debug.Assert(_builder.Length == 0);
      string fullMessage;

      try {
        if (logType == LogType.Debug) {
          _builder.Append("[DEBUG] ");
        } else if (logType == LogType.Trace) {
          _builder.Append("[TRACE] ");
        }

        if (UseGlobalPrefix) {
          if (UseColorTags) {
            _builder.Append("<color=");
            _builder.Append(GlobalPrefixColor);
            _builder.Append(">");
          }
          _builder.Append("[Fusion");

          if (!string.IsNullOrEmpty(prefix)) {
            _builder.Append("/");
            _builder.Append(prefix);
          }

          _builder.Append("]");

          if (UseColorTags) {
            _builder.Append("</color>");
          }
          _builder.Append(" ");
        } else {
          if (!string.IsNullOrEmpty(prefix)) {
            _builder.Append(prefix);
            _builder.Append(": ");
          }
        }

        var options = new LogOptions(UseColorTags, GenerateColor);
        context.BuildLogMessage(_builder, message, options);
        fullMessage = _builder.ToString();
      } finally {
        _builder.Clear();
      }

      var obj = context as UnityEngine.Object;

      switch (logType) {
        case LogType.Error:
          Debug.LogError(fullMessage, obj);
          break;
        case LogType.Warn:
          Debug.LogWarning(fullMessage, obj);
          break;
        default:
          Debug.Log(fullMessage, obj);
          break;
      }
    }

    public void LogException<T>(string prefix, ref T context, Exception ex) where T : ILogBuilder {
      Log(LogType.Error, string.Empty, ref context, PreExceptionMessage);
      if (context is UnityEngine.Object obj) {
        Debug.LogException(ex, obj);
      } else {
        Debug.LogException(ex);
      }
    }

    static int GetRandomColor(int seed, Color32 min, Color32 max) {
      var random = new NetworkRNG(seed);
      int r = random.RangeInclusive(min.r, max.r);
      int g = random.RangeInclusive(min.g, max.g);
      int b = random.RangeInclusive(min.b, max.b);

      r = Mathf.Clamp(r, 0, 255);
      g = Mathf.Clamp(g, 0, 255);
      b = Mathf.Clamp(b, 0, 255);

      int rgb = (r << 16) | (g << 8) | b;
      return rgb;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Initialize() {
      if (Fusion.Log.Initialized) {
        return;
      }

      var logger = new FusionUnityLogger() {
        UseColorTags = true,
        UseGlobalPrefix = true,
#if UNITY_EDITOR
        GlobalPrefixColor = UnityEditor.EditorGUIUtility.isProSkin ? ColorDarkSkin : ColorLightSkin,
#else
        GlobalPrefixColor = ColorRuntime,
#endif
      };

#if UNITY_EDITOR
      if (UnityEditor.EditorGUIUtility.isProSkin) {
        logger.GenerateColor = (seed) => GetRandomColor(seed, new Color32(158, 158, 158, 0), new Color32(255, 255, 255, 0));
      } else
#endif
      {
        logger.GenerateColor = (seed) => GetRandomColor(seed, new Color32(20, 20, 20, 0), new Color32(90, 90, 90, 0));
      }

      InitializePartial(ref logger);

      if (logger != null) {
        Fusion.Log.Init(logger);
      }
    }

  }
}
