using System;
using Fusion;
using Fusion.Sockets;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using Fusion.Editor;
#endif

/// <summary>
/// A Fusion prototyping class for starting up basic networking. Add this component to your startup scene, and supply a <see cref="RunnerPrefab"/>.
/// Can be set to automatically startup the network, display an in-game menu, or allow simplified start calls like <see cref="StartHost()"/>.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Fusion/Prototyping/Network Debug Start")]
public class NetworkDebugStart : Fusion.Behaviour {

#if UNITY_EDITOR
  public override EditorHeaderBackColor EditorHeaderBackColor => EditorHeaderBackColor.Steel;
#endif

  /// <summary>
  /// Enumeration of Server Modes. Server starts a server peer without a local player. Host starts a server peer with a local player.
  /// Shared indicates that no peer should be started, and the Photon cloud will act as the server.
  /// </summary>
  public enum ServerModes {
    Server,
    Host,
    Shared
  }

  /// <summary>
  /// Selection for how <see cref="NetworkDebugStart"/> will behave at startup.
  /// </summary>
  public enum StartModes {
    UserInterface,
    Automatic,
    Manual
  }

  /// <summary>
  /// Supply a Prefab or a scene object which has the <see cref="NetworkRunner"/> component on it, 
  /// as well as any runner dependent components which implement <see cref="INetworkRunnerCallbacks"/>, 
  /// such as <see cref="NetworkEvents"/> or your own custom INetworkInput implementations.
  /// </summary>
  [WarnIf(nameof(RunnerPrefab), 0, "No " + nameof(RunnerPrefab) + " supplied. Will search for a " + nameof(NetworkRunner) + " in the scene at startup.")]
  public NetworkRunner RunnerPrefab;

  /// <summary>
  /// Select how network startup will be triggered. Automatically, by in-game menu selection, or exclusively by script.
  /// </summary>
  [WarnIf(nameof(StartMode), (long)StartModes.Manual, "Start network by calling the methods " +
                                                         nameof(StartHost) + "(), " +
                                                         nameof(StartServer) + "(), " +
                                                         nameof(StartClient) + "(), " +
                                                         nameof(StartHostPlusClients) + "(), or " +
                                                         nameof(StartServerPlusClients) + "()"
  )]
  public StartModes StartMode = StartModes.UserInterface;

  /// <summary>
  /// When <see cref="StartMode"/> is set to <see cref="StartModes.Automatic"/>, this option selects if the <see cref="NetworkRunner"/> 
  /// will be started as a dedicated server, or as a host (which is a server with a local player).
  /// </summary>
  [DrawIf(nameof(StartMode), (long)StartModes.Automatic)]
  public ServerModes Server;

  /// <summary>
  /// The number of client <see cref="NetworkRunner"/> instances that will be created if running in Mulit-Peer Mode. 
  /// When using the Select start mode, this number will be the default value for the additional clients option box.
  /// </summary>
  [DrawIf(nameof(_showAutoClients), true, DrawIfHideType.ReadOnly)]
  public int AutoClients = 1;

  bool _usingMultiPeerMode => NetworkProjectConfigAsset.Instance.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
  bool _showAutoClients => StartMode != StartModes.Manual && _usingMultiPeerMode;

  /// <summary>
  /// The port that server/host <see cref="NetworkRunner"/> will use.
  /// </summary>
  public ushort ServerPort = 27015;

  /// <summary>
  /// The default room name to use when connecting to photon cloud.
  /// </summary>
  public string DefaultRoomName = ""; // empty/null means Random Room Name

  public bool AlwaysShowStats = false;

  [NonSerialized]
  NetworkRunner _server;

  [NonSerialized]
  bool _isStartingUp;

  GameMode? GetServerGameMode(ServerModes? serverModes) {
    if (serverModes == null) return null;

    switch (serverModes) {
      case ServerModes.Server: return GameMode.Server;
      case ServerModes.Host: return GameMode.Host;
      case ServerModes.Shared: return GameMode.Shared;
    }
    throw new InvalidOperationException();
  }

  protected virtual void Start() {

    var config = NetworkProjectConfigAsset.Instance.Config;
    var isMultiPeer = config.PeerMode == NetworkProjectConfig.PeerModes.Multiple;

    var existingrunner = FindObjectOfType<NetworkRunner>();

    if (existingrunner && existingrunner != RunnerPrefab) {
      if (existingrunner.State != NetworkRunner.States.Shutdown) {
        // disable
        enabled = false;

        // destroy this and GUI (if exists), and return
        var gui = GetComponent<NetworkDebugStartGUI>();
        if (gui) {
          Destroy(gui);
        }

        Destroy(this);
        return;
      } else {
        // If no RunnerPrefab is supplied, use the scene runner.
        if (RunnerPrefab == null) {
          RunnerPrefab = existingrunner;
        }
      }
    }

    if (StartMode == StartModes.Manual)
      return;

    //// Force this to select if auto not allowed.
    //if (StartMode == StartModes.Automatic && config.PeerMode != NetworkProjectConfig.PeerModes.Multiple && Server != ServerModes.Shared) {
    //  StartMode = StartModes.UserInterface;
    //}

    if (StartMode == StartModes.Automatic) {
      if (TryGetSceneRef(out var sceneRef)) {
        StartCoroutine(StartWithClients(GetServerGameMode(Server), sceneRef, isMultiPeer ? AutoClients : 0));
      }
    } else {
      if (TryGetComponent<NetworkDebugStartGUI>(out var _) == false) {
        gameObject.AddComponent<NetworkDebugStartGUI>();
      }
    }
  }

  protected bool TryGetSceneRef(out SceneRef sceneRef) {
    var scenePath = SceneManager.GetActiveScene().path;
    var config = NetworkProjectConfigAsset.Instance.Config;

    if (config.TryGetSceneRef(scenePath, out sceneRef) == false) {
      // Failed to find scene by full path, try with just name
      if (config.TryGetSceneRef(SceneManager.GetActiveScene().name, out sceneRef) == false) {
        Debug.LogError($"Could not find scene reference to scene {scenePath}, make sure it's added to {nameof(NetworkProjectConfig)}.");
        return false;
      }
    }
    return true;
  }

  /// <summary>
  /// Start a server instance.
  /// </summary>
  [BehaviourButtonAction(nameof(StartServer), true, false)]
  public virtual void StartServer() {
    if (TryGetSceneRef(out var sceneRef)) {
      StartCoroutine(StartWithClients(GameMode.Server, sceneRef, 0));
    }
  }

  /// <summary>
  /// Start a host instance. This is a server instance, with a local player.
  /// </summary>
  [BehaviourButtonAction(nameof(StartHost), true, false)]
  public virtual void StartHost() {
    if (TryGetSceneRef(out var sceneRef)) {
      StartCoroutine(StartWithClients(GameMode.Host, sceneRef, 0));
    }
  }

  /// <summary>
  /// Start a client instance.
  /// </summary>
  [BehaviourButtonAction(nameof(StartClient), true, false)]
  public virtual void StartClient() {
    StartCoroutine(StartWithClients(null, default, 1));
  }

  [BehaviourButtonAction(nameof(StartSharedClient), true, false)]
  public virtual void StartSharedClient() {
    if (TryGetSceneRef(out var sceneRef)) {
      StartCoroutine(StartWithClients(GameMode.Shared, sceneRef, 1));
    }
  }

  /// <summary>
  /// Start a Fusion server instance, and the number of client instances indicated by <see cref="AutoClients"/>. 
  /// InstanceMode must be set to Multi-Peer mode, as this requires multiple <see cref="NetworkRunner"/> instances.
  /// </summary>
  [BehaviourButtonAction(nameof(StartServerPlusClients), true, false, nameof(_usingMultiPeerMode))]
  public virtual void StartServerPlusClients() {
    StartServerPlusClients(AutoClients);
  }

  /// <summary>
  /// Start a Fusion host instance, and the number of client instances indicated by <see cref="AutoClients"/>. 
  /// InstanceMode must be set to Multi-Peer mode, as this requires multiple <see cref="NetworkRunner"/> instances.
  /// </summary>
  [BehaviourButtonAction(nameof(StartHostPlusClients), true, false, nameof(_usingMultiPeerMode))]
  public void StartHostPlusClients() {
    StartHostPlusClients(AutoClients);
  }

  /// <summary>
  /// Start a Fusion server instance, and the indicated number of client instances. 
  /// InstanceMode must be set to Multi-Peer mode, as this requires multiple <see cref="NetworkRunner"/> instances.
  /// </summary>
  public virtual void StartServerPlusClients(int clientCount) {
    if (NetworkProjectConfigAsset.Instance.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
      if (TryGetSceneRef(out var sceneRef)) {
        StartCoroutine(StartWithClients(GameMode.Server, sceneRef, clientCount));
      }
    } else {
      Debug.LogWarning($"Unable to start multiple {nameof(NetworkRunner)}s in Unique Instance mode.");
    }
  }

  /// <summary>
  /// Start a Fusion host instance (server with local player), and the indicated number of additional client instances. 
  /// InstanceMode must be set to Multi-Peer mode, as this requires multiple <see cref="NetworkRunner"/> instances.
  /// </summary>
  public void StartHostPlusClients(int clientCount) {
    if (NetworkProjectConfigAsset.Instance.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
      if (TryGetSceneRef(out var sceneRef)) {
        StartCoroutine(StartWithClients(GameMode.Host, sceneRef, clientCount));
      }
    } else {
      Debug.LogWarning($"Unable to start multiple {nameof(NetworkRunner)}s in Unique Instance mode.");
    }
  }

  /// <summary>
  /// Start a Fusion host instance (server with local player), and the indicated number of additional client instances. 
  /// InstanceMode must be set to Multi-Peer mode, as this requires multiple <see cref="NetworkRunner"/> instances.
  /// </summary>
  public void StartMultipleClients(int clientCount) {
    if (NetworkProjectConfigAsset.Instance.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
      if (TryGetSceneRef(out var sceneRef)) {
        StartCoroutine(StartWithClients(null, sceneRef, clientCount));
      }
    } else {
      Debug.LogWarning($"Unable to start multiple {nameof(NetworkRunner)}s in Unique Instance mode.");
    }
  }

  /// <summary>
  /// Start as Room on the Photon cloud, and connects as one or more clients.
  /// </summary>
  /// <param name="clientCount"></param>
  public void StartMultipleSharedClients(int clientCount) {
    if (NetworkProjectConfigAsset.Instance.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
      if (TryGetSceneRef(out var sceneRef)) {
        StartCoroutine(StartWithClients(GameMode.Shared, sceneRef, clientCount));
      }
    } else {
      Debug.LogWarning($"Unable to start multiple {nameof(NetworkRunner)}s in Unique Instance mode.");
    }
  }

  protected IEnumerator StartClients(int clientCount, Scene? unload, GameMode? serverMode, SceneRef sceneRef = default) {

    bool unloadSceneFirst = serverMode.HasValue && serverMode.Value != GameMode.Shared && unload.HasValue;
    // unload if exists, and we have a server type (server runner already has started, so we can delete the default scene)
    if (unloadSceneFirst) {
      yield return SceneManager.UnloadSceneAsync(unload.Value);
    }

    var clientTasks = new List<Task>();

    for (int i = 0; i < clientCount; ++i) {
      var client = Instantiate(RunnerPrefab);
      client.name = "Client#" + i;
      client.ProvideInput = i == 0;

      // if server mode is shared, then game client mode is shared also, otherwise its client
      var mode = serverMode == GameMode.Shared ? GameMode.Shared : GameMode.Client;

      var clientTask = InitializeNetworkRunner(client, mode, NetAddress.Any(), sceneRef, (runner) => {
        var name = client.name; // closures do not capture values, need a local var to save it
        Log.Debug($"Start {name} done");
      });

      clientTasks.Add(clientTask);
    }

    bool done;
    do {
      done = true;

      foreach (var task in clientTasks) {
        done &= task.IsCompleted;

        if (task.IsFaulted) {
          Log.DebugWarn(task.Exception);
        }
      }

      yield return null;

    } while (done == false);

    Destroy(RunnerPrefab.gameObject);
    // destroy this and GUI (if exists)

    // unload initial scene if we haven't already.
    if (!unloadSceneFirst && unload.HasValue) {
      yield return SceneManager.UnloadSceneAsync(unload.Value);
    }

    // Add stats last, so that event systems that may be getting created are already in place.
    if (AlwaysShowStats) {
      FusionStats.Create();
    }

    var gui = GetComponent<NetworkDebugStartGUI>();
    if (gui) {
      Destroy(gui);
    }

    Destroy(this);

  }

  protected IEnumerator StartWithClients(GameMode? serverMode, SceneRef sceneRef, int clientCount) {
    // Avoid double clicks or disallow multiple startup calls.
    if (_isStartingUp) {
      yield break;
    }

    if (serverMode.HasValue == false && clientCount == 0) {
      Debug.LogError($"Server Mode is null and Client Count is zero. Starting no network runners.");
      yield break;
    }



    _isStartingUp = true;

    var currentScene = SceneManager.GetActiveScene();

    // must have a runner
    if (!RunnerPrefab) {
      Debug.LogError($"{nameof(RunnerPrefab)} not set, can't perform debug start.");
      yield break;
    }

    // Clone the RunnerPrefab so we can safely delete the startup scene (the prefab might be part of it, rather than an asset).
    RunnerPrefab = Instantiate(RunnerPrefab);
    DontDestroyOnLoad(RunnerPrefab);
    RunnerPrefab.name = "Temporary Runner Prefab";

    var config = NetworkProjectConfigAsset.Instance.Config;
    if (clientCount > 1 || (clientCount > 0 && serverMode.HasValue && serverMode.Value != GameMode.Shared)) {
      if (config.PeerMode != NetworkProjectConfig.PeerModes.Multiple) {
        Debug.LogError($"Instance mode must be set to {nameof(NetworkProjectConfig.PeerModes.Multiple)} to perform a debug start with server + client(s)");
        yield break;
      }
    }

    if (gameObject.transform.parent) {
      Debug.LogWarning($"{nameof(NetworkDebugStart)} can't be a child game object, unparenting.");
      gameObject.transform.parent = null;
    }
    DontDestroyOnLoad(gameObject);

    // start scene
    var scene = config.PeerMode == NetworkProjectConfig.PeerModes.Multiple ? currentScene : default(Scene?);

    // start server, just take address from it
    if (serverMode.HasValue && serverMode.Value != GameMode.Shared) {
      _server = Instantiate(RunnerPrefab);
      _server.name = serverMode.Value.ToString();
      _server.ProvideInput = (serverMode.Value == GameMode.Host || serverMode.Value == GameMode.Single) && clientCount == 0;

      InitializeNetworkRunner(_server, serverMode.Value, NetAddress.Any(ServerPort), sceneRef, runner => {
        StartCoroutine(StartClients(clientCount, scene, serverMode, sceneRef));
      });
    } else {
      if (clientCount > 0) {
        StartCoroutine(StartClients(clientCount, scene, serverMode, sceneRef));
      } else {
        // Create stats last after all runners have been started (in this case just the host/server), so that any EventSystems from the scene are in place.
        if (AlwaysShowStats) {
          FusionStats.Create();
        }
        // Destroy our temporary prefab copy
        Destroy(RunnerPrefab.gameObject);
      }
    }
  }

  protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized) {
    return runner.StartGame(new StartGameArgs {
      GameMode = gameMode,
      Address = address,
      Scene = scene,
      SessionName = DefaultRoomName,
      Initialized = initialized
    });
  }

#if UNITY_EDITOR
  // Draws the button at the bottom of the inspector if scene currently is not added to Unity or Fusion scene lists.
  [BehaviourAction()]
  void DisplayAddToSceneButtonIfNeeded() {
    if (Application.isPlaying)
      return;
    var currentScene = SceneManager.GetActiveScene();
    if (currentScene.GetSceneIndexInBuildSettings() == -1 || currentScene.GetSceneIndexInFusionSettings() == -1) {
      GUILayout.Space(4);
      var clicked = Fusion.Editor.BehaviourEditorUtils.DrawWarnButton(new GUIContent("Add Scene To Settings", "Will add current scene to both Unity Build Settings and Fusion scene lists."), true);
      if (clicked) {
        if (currentScene.name == "") {
          UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        if (currentScene.name != "") {
          currentScene.AddSceneToBuildSettings();
        }

        currentScene.AddSceneToFusionConfig();
      }
    }
  }

#endif
}
