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
[ScriptHelp(BackColor = EditorHeaderBackColor.Steel)]
public class NetworkDebugStart : Fusion.Behaviour {

  /// <summary>
  /// Selection for how <see cref="NetworkDebugStart"/> will behave at startup.
  /// </summary>
  public enum StartModes {
    UserInterface,
    Automatic,
    Manual
  }

  /// <summary>
  /// The current stage of connection or shutdown.
  /// </summary>
  public enum Stage {
    Disconnected,
    StartingUp,
    UnloadOriginalScene,
    ConnectingServer,
    ConnectingClients,
    AllConnected,
  }

  /// <summary>
  /// Supply a Prefab or a scene object which has the <see cref="NetworkRunner"/> component on it, 
  /// as well as any runner dependent components which implement <see cref="INetworkRunnerCallbacks"/>, 
  /// such as <see cref="NetworkEvents"/> or your own custom INetworkInput implementations.
  /// </summary>
  [WarnIf(nameof(RunnerPrefab), 0, "No " + nameof(RunnerPrefab) + " supplied. Will search for a " + nameof(NetworkRunner) + " in the scene at startup.")]
  [InlineHelp]
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
  [InlineHelp]
  public StartModes StartMode = StartModes.UserInterface;

  /// <summary>
  /// When <see cref="StartMode"/> is set to <see cref="StartModes.Automatic"/>, this option selects if the <see cref="NetworkRunner"/> 
  /// will be started as a dedicated server, or as a host (which is a server with a local player).
  /// </summary>
  [UnityEngine.Serialization.FormerlySerializedAs("Server")]
  [DrawIf(nameof(StartMode), (long)StartModes.Automatic, DrawIfHideType.Hide)]
  [InlineHelp]
  public GameMode AutoStartAs = GameMode.Shared;

  /// <summary>
  /// <see cref="NetworkDebugStartGUI"/> will not render GUI elements while <see cref="CurrentStage"/> == <see cref="Stage.AllConnected"/>.
  /// </summary>
  [DrawIf(nameof(StartMode), (long)StartModes.UserInterface, DrawIfHideType.Hide)]
  [InlineHelp]
  public bool AutoHideGUI = true;

  /// <summary>
  /// The number of client <see cref="NetworkRunner"/> instances that will be created if running in Mulit-Peer Mode. 
  /// When using the Select start mode, this number will be the default value for the additional clients option box.
  /// </summary>
  [DrawIf(nameof(_showAutoClients), true, DrawIfHideType.Hide)]
  [InlineHelp]
  public int AutoClients = 1;

  bool _usingMultiPeerMode => NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
  bool _showAutoClients => StartMode != StartModes.Manual && _usingMultiPeerMode && AutoStartAs != GameMode.Single;

  /// <summary>
  /// The port that server/host <see cref="NetworkRunner"/> will use.
  /// </summary>
  [InlineHelp]
  public ushort ServerPort = 27015;

  /// <summary>
  /// The default room name to use when connecting to photon cloud.
  /// </summary>
  [InlineHelp]
  public string DefaultRoomName = ""; // empty/null means Random Room Name

  /// <summary>
  /// Will automatically enable <see cref="FusionStats"/> once peers have finished connecting.
  /// </summary>
  [InlineHelp]
  public bool AlwaysShowStats = false;


  [NonSerialized]
  NetworkRunner _server;

  /// <summary>
  /// The Scene that will be loaded after network shutdown completes (all peers have disconnected). 
  /// If this field is null or invalid, will be set to the current scene when <see cref="NetworkDebugStart"/> runs Awake().
  /// </summary>
  [ScenePath]
  [InlineHelp]
  public string InitialScenePath;
  static string _initialScenePath;

  /// <summary>
  /// Indicates which step of the startup process <see cref="NetworkDebugStart"/> is currently in.
  /// </summary>
  public Stage CurrentStage { get; internal set; }

#if UNITY_EDITOR
  protected virtual void Reset() {
    if (TryGetComponent<NetworkDebugStartGUI>(out var ndsg) == false) {
      ndsg = gameObject.AddComponent<NetworkDebugStartGUI>();
    }
  }

#endif

  protected virtual void Start() {

    if (_initialScenePath == null) {
      if (String.IsNullOrEmpty(InitialScenePath)) {
        var currentScene = SceneManager.GetActiveScene();
        if (currentScene.IsValid()) {
          _initialScenePath = currentScene.path;
        } else {
          // Last fallback is the first entry in the build settings
          _initialScenePath = SceneManager.GetSceneByBuildIndex(0).path;
        }
        InitialScenePath = _initialScenePath;
      } else {
        _initialScenePath = InitialScenePath;
      }
    }

    var config = NetworkProjectConfig.Global;
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
        StartCoroutine(StartWithClients(AutoStartAs, sceneRef, isMultiPeer ? AutoClients : (AutoStartAs == GameMode.Client ? 1 : 0)));
      }
    } else {
      if (TryGetComponent<NetworkDebugStartGUI>(out var _) == false) {
        gameObject.AddComponent<NetworkDebugStartGUI>();
      }
    }
  }

  protected bool TryGetSceneRef(out SceneRef sceneRef) {
    var scenePath = SceneManager.GetActiveScene().path;
    var config = NetworkProjectConfig.Global;

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
  /// Start a single player instance.
  /// </summary>
  [BehaviourButtonAction(nameof(StartSinglePlayer), true, false)]
  public virtual void StartSinglePlayer() {
    if (TryGetSceneRef(out var sceneRef)) {
      StartCoroutine(StartWithClients(GameMode.Single, sceneRef, 0));
    }
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
    StartCoroutine(StartWithClients(GameMode.Client, default, 1));
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

  [BehaviourButtonAction("Shutdown", true, false)]
  public void Shutdown() {
    ShutdownAll();
  }

  /// <summary>
  /// Start a Fusion server instance, and the indicated number of client instances. 
  /// InstanceMode must be set to Multi-Peer mode, as this requires multiple <see cref="NetworkRunner"/> instances.
  /// </summary>
  public virtual void StartServerPlusClients(int clientCount) {
    if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
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
    if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
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
    if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
      if (TryGetSceneRef(out var sceneRef)) {
        StartCoroutine(StartWithClients(GameMode.Client, sceneRef, clientCount));
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
    if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
      if (TryGetSceneRef(out var sceneRef)) {
        StartCoroutine(StartWithClients(GameMode.Shared, sceneRef, clientCount));
      }
    } else {
      Debug.LogWarning($"Unable to start multiple {nameof(NetworkRunner)}s in Unique Instance mode.");
    }
  }

  public void ShutdownAll() {

    var runners = NetworkRunner.GetInstancesEnumerator();
    while (runners.MoveNext()) {
      var runner = runners.Current;
      if (runner != null && runner.IsRunning) {
        runner.Shutdown();
      }
    }

    SceneManager.LoadSceneAsync(_initialScenePath);
    // Destroy our DontDestroyOnLoad objects to finish the reset
    Destroy(RunnerPrefab.gameObject);
    Destroy(gameObject);
    CurrentStage = Stage.Disconnected;
  }


  protected IEnumerator StartWithClients(GameMode serverMode, SceneRef sceneRef, int clientCount) {
    // Avoid double clicks or disallow multiple startup calls.
    if (CurrentStage != Stage.Disconnected) {
      yield break;
    }

    bool includesServerStart = serverMode != GameMode.Shared && serverMode != GameMode.Client;

    if (!includesServerStart && clientCount == 0) {
      Debug.LogError($"{nameof(GameMode)} is set to {serverMode}, and {nameof(clientCount)} is set to zero. Starting no network runners.");
      yield break;
    }

    CurrentStage = Stage.StartingUp;

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

    // Single-peer can't start more than one peer. Validate clientCount to make sure it complies with current PeerMode.
    var config = NetworkProjectConfig.Global;
    if (config.PeerMode != NetworkProjectConfig.PeerModes.Multiple) {
      int maxClientsAllowed = includesServerStart ? 0 : 1;
      if (clientCount > maxClientsAllowed) {
        Debug.LogWarning($"Instance mode must be set to {nameof(NetworkProjectConfig.PeerModes.Multiple)} to perform a debug start multiple peers. Restricting client count to {maxClientsAllowed}.");
        clientCount = maxClientsAllowed;
      }
    }

    // If NDS is starting more than 1 shared client, they need to use the same Session Name, otherwise, they will end up on different Rooms
    // as Fusion creates a Random Session Name when no name is passed on the args
    if (serverMode == GameMode.Shared && clientCount > 1 && config.PeerMode == NetworkProjectConfig.PeerModes.Multiple) {
      DefaultRoomName = string.IsNullOrEmpty(DefaultRoomName) == false ? DefaultRoomName : Guid.NewGuid().ToString();
    }

    if (gameObject.transform.parent) {
      Debug.LogWarning($"{nameof(NetworkDebugStart)} can't be a child game object, un-parenting.");
      gameObject.transform.parent = null;
    }

    DontDestroyOnLoad(gameObject);

    // start scene
    var scene = config.PeerMode == NetworkProjectConfig.PeerModes.Multiple ? currentScene : default(Scene?);

    // start server, just take address from it
    if (includesServerStart) {
      _server = Instantiate(RunnerPrefab);
      _server.name = serverMode.ToString();
      _server.ProvideInput = (serverMode == GameMode.Host || serverMode == GameMode.Single) && clientCount == 0;

      InitializeNetworkRunner(_server, serverMode, NetAddress.Any(ServerPort), sceneRef, (runner) => {
#if FUSION_DEV
        var name = _server.name; // closures do not capture values, need a local var to save it
        Debug.Log($"Server NetworkRunner '{name}' started.");
#endif
        // this action is called after InitializeNetworkRunner for the server has completed startup
        StartCoroutine(StartClients(clientCount, scene, serverMode, sceneRef));
      });
    } else {
      StartCoroutine(StartClients(clientCount, scene, serverMode, sceneRef));
    }
  }

  protected IEnumerator StartClients(int clientCount, Scene? unload, GameMode serverMode, SceneRef sceneRef = default) {

    // If a server was started and there is a scene to unload - do so before clients are added (to avoid multiple AudioListener, etc warnings)
    if (unload.HasValue && serverMode != GameMode.Shared && serverMode != GameMode.Client) {
      CurrentStage = Stage.UnloadOriginalScene;
      yield return SceneManager.UnloadSceneAsync(unload.Value);
      unload = null;
    }

    var clientTasks = new List<Task>();

    CurrentStage = Stage.ConnectingClients;

    for (int i = 0; i < clientCount; ++i) {
      var client = Instantiate(RunnerPrefab);
      client.name = "Client#" + i;
      client.ProvideInput = i == 0;

      // if server mode is shared, then game client mode is shared also, otherwise its client
      var mode = serverMode == GameMode.Shared ? GameMode.Shared : GameMode.Client;

#if FUSION_DEV
      var clientTask = InitializeNetworkRunner(client, mode, NetAddress.Any(), sceneRef, (runner) => {
        var name = client.name; // closures do not capture values, need a local var to save it
        Debug.Log($"Client NetworkRunner '{name}' started.");
      });
#else
      var clientTask = InitializeNetworkRunner(client, mode, NetAddress.Any(), sceneRef, null);
#endif

      clientTasks.Add(clientTask);
    }

    bool done;
    do {
      done = true;

      // yield until all tasks report as completed
      foreach (var task in clientTasks) {
        done &= task.IsCompleted;

        if (task.IsFaulted) {
          Debug.LogWarning(task.Exception);
        }
      }
      yield return null;

    } while (done == false);

    CurrentStage = Stage.AllConnected;

    // Add stats last, so that event systems that may be getting created are already in place.
    if (AlwaysShowStats) {
      FusionStats.Create();
    }

    // unload original scene if needed and has not been done yet.
    if (unload.HasValue) {
      yield return SceneManager.UnloadSceneAsync(unload.Value);
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
      var clicked = Fusion.Editor.BehaviourEditorUtils.DrawWarnButton(new GUIContent("Add Scene To Settings", "Will add current scene to both Unity Build Settings and Fusion scene lists."), MessageType.Warning);
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
