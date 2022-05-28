using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace rwby
{
    // TODO: Better way 
    public abstract class CustomNetworkSceneManagerBase : Fusion.Behaviour, INetworkSceneObjectProvider
    {
        public NetworkRunner Runner { get; private set; } = null;
        public FusionLauncher SessionHandler { get; private set; } = null;

        private static WeakReference<CustomNetworkSceneManagerBase> s_currentlyLoading = new WeakReference<CustomNetworkSceneManagerBase>(null);
        
        private IEnumerator _runningCoroutine;
        public bool ShowHierarchyWindowOverlay = true;
        private Dictionary<Guid, NetworkObject> _sceneObjects = new Dictionary<Guid, NetworkObject>();
        private bool _currentSceneOutdated = false;

        [FormerlySerializedAs("currentLoadedScenes")] 
        public List<CustomSceneRef> localLoadedScenes = new List<CustomSceneRef>();
        public int _currentSceneChangeValue = -1;

        public byte loadPercentage = 0;
        
        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (ShowHierarchyWindowOverlay)
            {
                UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowOverlay;
            }
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowOverlay;
#endif
        }
        
        void INetworkSceneObjectProvider.Initialize(NetworkRunner runner)
        {
            Initialize(runner);
        }

        void INetworkSceneObjectProvider.Shutdown(NetworkRunner runner)
        {
            Shutdown(runner);
        }
        
        protected virtual void Initialize(NetworkRunner runner)
        {
            Assert.Check(!Runner);
            SessionHandler = GameManager.singleton.networkManager.GetSessionHandlerByRunner(runner);
            Runner = runner;
        }

        protected virtual void Shutdown(NetworkRunner runner)
        {
            Assert.Check(Runner == runner);
            Runner = null;
            SessionHandler = null;
            
            try
            {
                // ongoing loading, dispose
                if (_runningCoroutine != null)
                {
                    LogWarn($"There is an ongoing scene load, stopping and disposing coroutine.");
                    StopCoroutine(_runningCoroutine);
                    (_runningCoroutine as IDisposable)?.Dispose();
                }
            }
            finally
            {
                Runner = null;
                _runningCoroutine = null;
                localLoadedScenes.Clear();
                _currentSceneOutdated = false;
                _sceneObjects.Clear();
            }
        }

        protected virtual void LateUpdate()
        {
            // Not initialized yet.
            if (!Runner || !SessionHandler) return;
            
            // store the flag in case scene changes during the load; this supports scene toggling as well
            if (IsSceneListUpToDate() == false) _currentSceneOutdated = true;

            if (!_currentSceneOutdated || _runningCoroutine != null) return;

            if (s_currentlyLoading.TryGetTarget(out var target))
            {
                Assert.Check(target != this);
                if (!target)
                {
                    // orphaned loader?
                    s_currentlyLoading.SetTarget(null);
                }
                else
                {
                    LogTrace($"Waiting for {target} to finish loading");
                    return;
                }
            }
            
            var prevScenes = localLoadedScenes;
            localLoadedScenes = SessionHandler.GetCurrentScenes();
            _currentSceneChangeValue = Runner.CurrentScene;
            _currentSceneOutdated = false;
            
            LogTrace($"Scene transition {prevScenes.Count}->{localLoadedScenes.Count}");
            _runningCoroutine = UpdateScenesWrapper(prevScenes, localLoadedScenes);
            StartCoroutine(_runningCoroutine);
        }

        protected delegate void FinishedLoadingDelegate(IEnumerable<NetworkObject> sceneObjects);
        
        protected abstract IEnumerator SwitchScene(List<CustomSceneRef> oldScenes, List<CustomSceneRef> newScenes, FinishedLoadingDelegate finished);
        
        private IEnumerator UpdateScenesWrapper(List<CustomSceneRef> oldScenes, List<CustomSceneRef> newScenes)
        {
            loadPercentage = 0;
            if (Runner.IsServer)
            {
                foreach (PlayerRef playerRef in Runner.ActivePlayers)
                {
                    NetworkObject po = Runner.GetPlayerObject(playerRef);
                    if (!po) continue;
                    po.GetBehaviour<ClientManager>().mapLoadPercent = 0;
                }
            }else if (Runner.IsClient)
            {
                NetworkObject po = Runner.GetPlayerObject(Runner.LocalPlayer);
                if(po) po.GetBehaviour<ClientManager>().CLIENT_SetMapLoadPercentage(0);
            }
            bool finishCalled = false;
            Dictionary<Guid, NetworkObject> sceneObjects = new Dictionary<Guid, NetworkObject>();
            Exception error = null;
            FinishedLoadingDelegate callback = (objects) => {
                finishCalled = true;
                foreach (var obj in objects)
                {
                    sceneObjects.Add(obj.NetworkGuid, obj);
                }
            };
            
            try
            {
                Assert.Check(!s_currentlyLoading.TryGetTarget(out _));
                s_currentlyLoading.SetTarget(this);
                Runner.InvokeSceneLoadStart();
                var coro = SwitchScene(oldScenes, newScenes, callback);

                for (bool next = true; next;)
                {
                    try
                    {
                        next = coro.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                        break;
                    }

                    if (next)
                    {
                        yield return coro.Current;
                    }
                }
            }
            finally
            {
                Assert.Check(s_currentlyLoading.TryGetTarget(out var target) && target == this);
                s_currentlyLoading.SetTarget(null);

                LogTrace($"Corutine finished for scene loading.");
                _runningCoroutine = null;
            }

            if (error != null)
            {
                LogError($"Failed to switch scenes: {error}");
            }
            else if (!finishCalled)
            {
                LogError($"Failed to switch scenes: SwitchScene implementation did not invoke finished delegate");
            }
            else
            {
                _sceneObjects = sceneObjects;
                Runner.RegisterUniqueObjects(_sceneObjects.Values);
                Runner.InvokeSceneLoadDone();
                if (Runner.LocalPlayer.IsValid)
                {
                    NetworkObject no = Runner.GetPlayerObject(Runner.LocalPlayer);
                    if(no) no.GetBehaviour<ClientManager>().CLIENT_SetMapLoadPercentage(100);
                }
                loadPercentage = 100;
            }
        }

        private bool IsSceneListUpToDate()
        {
            if (Runner.CurrentScene == _currentSceneChangeValue) return true;
            if (SessionHandler.GetCurrentScenes() == localLoadedScenes) return true;
            _currentSceneChangeValue = Runner.CurrentScene;
            return false;
        }

        // TODO: ?
        public bool IsReady(NetworkRunner runner)
        {
            Assert.Check(Runner == runner);
            if (_runningCoroutine != null)
            {
                return false;
            }
            if (_currentSceneOutdated)
            {
                return false;
            }

            /*
            if (LobbyManager.singleton != null)
            {
                if(LobbyManager.singleton.currentLoadedScenes.ToList() != currentLoadedScenes) return false;
            }*/
            return true;
        }

        public bool TryResolveSceneObject(NetworkRunner runner, Guid sceneObjectGuid, out NetworkObject instance)
        {
            Assert.Check(Runner == runner);
            return _sceneObjects.TryGetValue(sceneObjectGuid, out instance);
        }

        public List<NetworkObject> FindNetworkObjects(Scene scene, bool disable = true, bool addVisibilityNodes = false)
        {
            var networkObjects = new List<NetworkObject>();
            var gameObjects = scene.GetRootGameObjects();
            var result = new List<NetworkObject>();

            // get all root gameobjects and move them to this runners scene
            foreach (var go in gameObjects)
            {
                networkObjects.Clear();
                go.GetComponentsInChildren(networkObjects);

                foreach (var sceneObject in networkObjects)
                {
                    if (sceneObject.Flags.IsSceneObject() && sceneObject.gameObject.activeInHierarchy)
                    {
                        Assert.Check(sceneObject.NetworkGuid.IsValid);
                        result.Add(sceneObject);
                    }
                }

                if (addVisibilityNodes)
                {
                    // register all render related components on this gameobject with the runner, for use with IsVisible
                    RunnerVisibilityNode.AddVisibilityNodes(go, Runner);
                }
            }

            if (disable)
            {
                foreach (var sceneObject in result)
                {
                    if (sceneObject.gameObject.activeInHierarchy)
                    {
                        sceneObject.gameObject.SetActive(false);
                    }
                }
            }

            return result;
        }
        
#if UNITY_EDITOR
        private static Lazy<GUIStyle> s_hierarchyOverlayLabelStyle = new Lazy<GUIStyle>(() => {
            var result = new GUIStyle(UnityEditor.EditorStyles.miniBoldLabel);
            result.alignment = TextAnchor.MiddleRight;
            result.padding.right += 20;
            result.padding.bottom += 2;
            return result;
        });

        private void HierarchyWindowOverlay(int instanceId, Rect position)
        {
            if (!Runner)
            {
                return;
            }

            if (!Runner.MultiplePeerUnityScene.IsValid())
            {
                return;
            }

            if (Runner.MultiplePeerUnityScene.GetHashCode() != instanceId)
            {
                return;
            }

            UnityEditor.EditorGUI.LabelField(position, Runner.name, s_hierarchyOverlayLabelStyle.Value);
        }
#endif
        
        [System.Diagnostics.Conditional("FUSION_NETWORK_SCENE_MANAGER_TRACE")]
        protected void LogTrace(string msg)
        {
            Log.Debug($"[NetworkSceneManager] {(this != null ? this.name : "<destroyed>")}: {msg}");
        }

        protected void LogError(string msg)
        {
            Log.Error($"[NetworkSceneManager] {(this != null ? this.name : "<destroyed>")}: {msg}");
        }

        protected void LogWarn(string msg)
        {
            Log.Warn($"[NetworkSceneManager] {(this != null ? this.name : "<destroyed>")}: {msg}");
        }
    }
}