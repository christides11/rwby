using System;
using UnityEngine;
using Fusion;

/// <summary>
/// Companion component for <see cref="NetworkDebugStart"/>. Automatically added as needed for rendering in-game networking IMGUI.
/// </summary>
[RequireComponent(typeof(NetworkDebugStart))]
[AddComponentMenu("Fusion/Network Debug Start GUI")]
public class NetworkDebugStartGUI : Fusion.Behaviour {

#if UNITY_EDITOR
  public override EditorHeaderBackColor EditorHeaderBackColor => EditorHeaderBackColor.Steel;
#endif

  /// <summary>
  /// When enabled, the in-game user interface buttons can be activated with the keys H (Host), S (Server) and C (Client).
  /// </summary>
  public bool EnableHotkeys;

  NetworkDebugStart _networkDebugStart;
  string _clientCount;
  bool _isMultiplePeerMode;

#if UNITY_EDITOR

  protected virtual void Reset() {
    _networkDebugStart = EnsureNetworkDebugStartExists();
    _clientCount = _networkDebugStart.AutoClients.ToString();
  }

#endif

  protected virtual void OnValidate() {
    ValidateClientCount();
  }

  protected void ValidateClientCount() {
    if (_clientCount == null) {
      _clientCount = "1";
    } else {
      _clientCount = System.Text.RegularExpressions.Regex.Replace(_clientCount, "[^0-9]", "");
    }
  }
  protected int GetClientCount() {
    try {
      return Convert.ToInt32(_clientCount);
    } catch {
      return 0;
    }
  }

  protected virtual void Awake() {

    _networkDebugStart = EnsureNetworkDebugStartExists();
    _clientCount = _networkDebugStart.AutoClients.ToString();
    ValidateClientCount();
  }
  protected virtual void Start() {
    _isMultiplePeerMode = NetworkProjectConfigAsset.Instance.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
  }

  protected NetworkDebugStart EnsureNetworkDebugStartExists() {
    if (_networkDebugStart) {
      if (_networkDebugStart.gameObject == gameObject)
        return _networkDebugStart;
    }

    if (TryGetBehaviour<NetworkDebugStart>(out var found)) {
      _networkDebugStart = found;
      return found;
    }

    _networkDebugStart = AddBehaviour<NetworkDebugStart>();
    return _networkDebugStart;
  }

  private void Update() {

    if (EnableHotkeys) {
      if (Input.GetKeyDown(KeyCode.H)) {
        if (_isMultiplePeerMode) {
          StartHostWithClients(_networkDebugStart);
        } else {
          _networkDebugStart.StartHost();
        }
      }

      if (Input.GetKeyDown(KeyCode.S)) {
        if (_isMultiplePeerMode) {
          StartServerWithClients(_networkDebugStart);
        } else {
          _networkDebugStart.StartServer();
        }
      }

      // starting as client is only an option in shared.
      if (!_isMultiplePeerMode)
        if (Input.GetKeyDown(KeyCode.C))
          _networkDebugStart.StartClient();

      // starting as client is only an option in shared.
      if (!_isMultiplePeerMode)
        if (Input.GetKeyDown(KeyCode.P))
          _networkDebugStart.StartSharedClient();
    }
  }

  protected virtual void OnGUI() {

    var nds = EnsureNetworkDebugStartExists();
    if (nds.StartMode != NetworkDebugStart.StartModes.UserInterface) {
      return;
    }

    GUI.skin = FusionScalableIMGUI.GetScaledSkin(out var height, out var width, out var padding, out var margin);


    GUILayout.BeginArea(new Rect(margin, margin, width + /*margin * 2 +*/ padding * 2, Screen.height));
    GUILayout.BeginVertical(GUI.skin.window);

    nds.DefaultRoomName = GUILayout.TextField(nds.DefaultRoomName, 25, GUILayout.Width(width), GUILayout.Height(height));

    if (_isMultiplePeerMode == false) {
      if (GUILayout.Button(EnableHotkeys ? "Start Shared Client (P)" : "Start Shared Client", GUILayout.Width(width), GUILayout.Height(height))) {
        nds.StartSharedClient();
      }
      if (GUILayout.Button(EnableHotkeys ? "Start Server (S)" : "Start Server", GUILayout.Width(width), GUILayout.Height(height))) {
        nds.StartServer();
      }
      if (GUILayout.Button(EnableHotkeys ? "Start Host (H)" : "Start Host", GUILayout.Width(width), GUILayout.Height(height))) {
        nds.StartHost();
      }
      if (GUILayout.Button(EnableHotkeys ? "Start Client (C)" : "Start Client", GUILayout.Width(width), GUILayout.Height(height))) {
        nds.StartClient();
      }
    } else {
      if (GUILayout.Button(EnableHotkeys ? "Start Shared Clients (P)" : "Start Shared Clients", GUILayout.Width(width), GUILayout.Height(height))) {
        StartMultipleSharedClients(nds);
      }
      if (GUILayout.Button(EnableHotkeys ? "Start Server (S)" : "Start Server", GUILayout.Width(width), GUILayout.Height(height))) {
        StartServerWithClients(nds);
      }
      if (GUILayout.Button(EnableHotkeys ? "Start Host (H)" : "Start Host", GUILayout.Width(width), GUILayout.Height(height))) {
        StartHostWithClients(nds);
      }
      if (GUILayout.Button(EnableHotkeys ? "Start Clients (C)" : "Start Clients", GUILayout.Width(width), GUILayout.Height(height))) {
        StartMultipleClients(nds);
      }

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Client Count", GUILayout.Height(height));
        GUILayout.Label("", GUILayout.Width(4));
        string newcount = GUILayout.TextField(_clientCount, 10, GUILayout.Width(width * .2f), GUILayout.Height(height));
        if (_clientCount != newcount) {
          // Remove everything but numbers from our client count string.
          _clientCount = newcount;
          ValidateClientCount();
        }
      }
      GUILayout.EndHorizontal();
    }

    GUILayout.EndVertical();
    GUILayout.EndArea();
  }

  private void StartHostWithClients(NetworkDebugStart nds) {
    int count;
    try {
      count = Convert.ToInt32(_clientCount);
    } catch {
      count = 0;
    }
    nds.StartHostPlusClients(count);
  }

  private void StartServerWithClients(NetworkDebugStart nds) {
    int count;
    try {
      count = Convert.ToInt32(_clientCount);
    } catch {
      count = 0;
    }
    nds.StartServerPlusClients(count);
  }

  private void StartMultipleClients(NetworkDebugStart nds) {
    int count;
    try {
      count = Convert.ToInt32(_clientCount);
    } catch {
      count = 0;
    }
    nds.StartMultipleClients(count);
  }

  private void StartMultipleSharedClients(NetworkDebugStart nds) {
    int count;
    try {
      count = Convert.ToInt32(_clientCount);
    } catch {
      count = 0;
    }
    nds.StartMultipleSharedClients(count);
  }
}
