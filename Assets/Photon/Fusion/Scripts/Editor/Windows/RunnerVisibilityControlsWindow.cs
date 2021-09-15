namespace Fusion.Editor {
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using UnityEngine;
  using UnityEditor;

  public class RunnerVisibilityControlsWindow : EditorWindow {

    const float WIDE_SWITCH_WIDTH = 194;
    const double KEY_COOLDOWN = .2d;
    const double REFRESH_RATE = 1f;
    
    public static RunnerVisibilityControlsWindow Instance { get; private set; }
    public static GUIContent VisibilitySettingLabel;
    public static GUIContent InputSettingLabel;
    public static GUIStyle WarnStyle;
    public static GUIStyle EyeStyle;
    public static GUIStyle InputStyle;

    Vector2 _scrollPosition;
    double _lastRepaintTime;

    [MenuItem("Window/Fusion/Runner Visibility Controls")]
    [MenuItem("Fusion/Windows/Runner Visibility Controls")]
    public static void ShowWindow() {
      var window = GetWindow(typeof(RunnerVisibilityControlsWindow), false, "Runner Visibility Controls");
      window.minSize = new Vector2(76, 40);
      Instance = (RunnerVisibilityControlsWindow)window;
    }

    private void Awake() {
      Instance = this;
    }

    private void OnEnable() {
      Instance = this;
    }

    private void OnDestroy() {
      Instance = null;
    }

    private static void InitializeStyles() {

      var txtcolor = EditorStyles.label.normal.textColor;
      var dimcolor = new Color(txtcolor.r, txtcolor.g, txtcolor.b, txtcolor.a * .5f);

      WarnStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, margin = new RectOffset(8, 8, 8, 8) };

      EyeStyle = new GUIStyle(EditorStyles.toggle);
      EyeStyle.normal.background    = Resources.Load<Texture2D>("icons/visible-disabled-icon");
      EyeStyle.active.background    = Resources.Load<Texture2D>("icons/visible-disabled-icon");
      EyeStyle.hover.background     = Resources.Load<Texture2D>("icons/visible-disabled-icon");
      EyeStyle.onNormal.background  = Resources.Load<Texture2D>("icons/visible-enabled-icon");
      EyeStyle.onActive.background  = Resources.Load<Texture2D>("icons/visible-enabled-icon");
      EyeStyle.onHover.background = Resources.Load<Texture2D>("icons/visible-enabled-icon");
      EyeStyle.normal.textColor   = dimcolor;
      EyeStyle.active.textColor   = dimcolor;
      EyeStyle.hover.textColor    = dimcolor;
      EyeStyle.focused.textColor  = dimcolor;
      EyeStyle.padding = new RectOffset(19, 0, 0, 0);

      InputStyle = new GUIStyle(EyeStyle);
      InputStyle.normal.background    = Resources.Load<Texture2D>("icons/input-disabled-icon");
      InputStyle.active.background    = Resources.Load<Texture2D>("icons/input-disabled-icon");
      InputStyle.hover.background     = Resources.Load<Texture2D>("icons/input-disabled-icon");
      InputStyle.onNormal.background  = Resources.Load<Texture2D>("icons/input-enabled-icon");
      InputStyle.onHover.background   = Resources.Load<Texture2D>("icons/input-enabled-icon");
      InputStyle.onActive.background  = Resources.Load<Texture2D>("icons/input-enabled-icon");
    }

    private void Update() {
      // Force a repaint every x seconds in case runner count and runner settings have changed.
      if ((Time.realtimeSinceStartup - _lastRepaintTime) > REFRESH_RATE)
        Repaint();
    }

    private void OnGUI() {

      _lastRepaintTime = Time.realtimeSinceStartup;


      // When not playing, we use the config asset rather than runner configs to get the current mode settings.
      if (!Application.isPlaying) {

        var npc = NetworkProjectConfig.Global;

        if (npc.PeerMode != NetworkProjectConfig.PeerModes.Multiple) {
          BehaviourEditorUtils.DrawWarnBox("Runner Visibility Controls only apply to Multi-Peer mode.", MessageType.Info);
        }
        return;
      }

      if (EyeStyle == null) {
        InitializeStyles();
      }

      bool isWide = EditorGUIUtility.currentViewWidth > WIDE_SWITCH_WIDTH;
      _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

      if (!Application.isPlaying) {
        BehaviourEditorUtils.DrawWarnBox("No Runners Active.", MessageType.Info);
      } else {
        var enumerator = NetworkRunner.GetInstancesEnumerator();
        while (enumerator.MoveNext()) {
          var runner = enumerator.Current;

          // Only show active runners.
          if (!runner || !runner.IsRunning) {
            continue;
          }

          NetworkProjectConfig config = runner.Config;

          // Check for MultiPeer using the runner.config, in case developer changed that prior to starting runner. (may disagree with asset config)
          if (config.PeerMode != NetworkProjectConfig.PeerModes.Multiple) {
            BehaviourEditorUtils.DrawWarnBox("Runner Visibility Controls only apply to Multi-Peer mode.", MessageType.Info);
            break;
          }

          EditorGUILayout.BeginHorizontal();
          {
            string runnerName = isWide ?
              (runner.MultiPeerUnitySceneRoot ? runner.MultiPeerUnitySceneRoot.scene.name : "") :
              (runner.IsServer ? "S" : "C");

            if (VisibilitySettingLabel == null)
              VisibilitySettingLabel = new GUIContent(runnerName, "Toggles IsVisible for this Runner. [Shift + Click] will solo the selected runner.");
            else
              VisibilitySettingLabel.text = runnerName;

            bool newVisVal = GUI.Toggle(EditorGUILayout.GetControlRect(GUILayout.Width(isWide ? 76 : 30)), runner.IsVisible, VisibilitySettingLabel);
            if (newVisVal != runner.IsVisible)
              runner.IsVisible = newVisVal;

            if (InputSettingLabel == null)
              InputSettingLabel = new GUIContent("", $"Toggles ProvideInput for this runner. [Shift + Click] will solo for the selected runner.");
            
            InputSettingLabel.text = isWide ? "Provide Input" : "In";

            if (runner.Mode != SimulationModes.Server) {
              bool newInpVal = GUI.Toggle(EditorGUILayout.GetControlRect(GUILayout.Width(isWide ? 94 : 40)), runner.ProvideInput, InputSettingLabel);
              if (newInpVal != runner.ProvideInput) {
                runner.ProvideInput = newInpVal;
              }
            } else {
              GUI.Label(EditorGUILayout.GetControlRect(GUILayout.Width(isWide ? 94 : 40)), "");
            }

            if (GUI.Button(EditorGUILayout.GetControlRect(GUILayout.Width(50)), "Stats")) {
              // reflection hack
              Type.GetType("FusionStats, Assembly-CSharp").GetMethod("SetActiveRunner").Invoke(null, new object[] { runner });
            }
          }
          EditorGUILayout.EndHorizontal();
        }
      }
      EditorGUILayout.EndScrollView();
    }
  }
}
