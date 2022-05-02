using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoadData
{
    public Scene scene;
    public AsyncOperation operation;
}

public static class AddressablesSceneLoader
{

    public static AsyncOperationHandle<SceneLoadData> LoadSceneAsync(object key, LoadSceneParameters parameters, bool allowSceneActivation = true, int priority = 0)
    {
        IResourceLocation location = GetLocation(key);
        AsyncOperationHandle dependencies = Addressables.DownloadDependenciesAsync(location.Dependencies, false);

        SceneLoadOperation operation = new SceneLoadOperation(Addressables.ResourceManager);
        operation.Init(location, parameters, allowSceneActivation, priority, dependencies);

        return Addressables.ResourceManager.StartOperation<SceneLoadData>(operation, dependencies);
    }

    public static AsyncOperationHandle UnloadSceneAsync(AsyncOperationHandle<SceneLoadData> sceneLoadHandle)
    {
        UnloadSceneOperation unloadOp = new UnloadSceneOperation();
        unloadOp.Init(sceneLoadHandle);
        return Addressables.ResourceManager.StartOperation(unloadOp, sceneLoadHandle);
    }

    private static IResourceLocation GetLocation(object key)
    {
        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            IList<IResourceLocation> locations = new List<IResourceLocation>();
            bool success = locator.Locate(key, typeof(SceneInstance), out locations);

            if (success)
            {
                return locations[0];
            }
        }

        return null;
    }

    private sealed class SceneLoadOperation : AsyncOperationBase<SceneLoadData>, IUpdateReceiver
    {

        private SceneLoadData _scene;
        private AsyncOperationHandle _dependencies;
        private IResourceLocation _location;
        private ResourceManager _resourceManager;
        private LoadSceneParameters _loadParameters;
        private bool _allowSceneActivation;
        private int _priority;

        public SceneLoadOperation(ResourceManager manager)
        {
            _resourceManager = manager;
        }

        public void Init(IResourceLocation location, LoadSceneParameters parameters, bool allowSceneActivation, int priority, AsyncOperationHandle dependencies)
        {
            _dependencies = dependencies;
            // Line found in the SceneProvider.cs source but Acquire() call is internal only?
            //if (_dependencies.IsValid())
            //    _dependencies.Acquire();

            _location = location;
            _loadParameters = parameters;
            _allowSceneActivation = allowSceneActivation;
            _priority = priority;
        }

        protected override void Destroy()
        {
            if (_dependencies.IsValid()) Addressables.Release(_dependencies);
            base.Destroy();
        }

        public override void GetDependencies(List<AsyncOperationHandle> deps)
        {
            if (_dependencies.IsValid()) deps.Add(_dependencies);
        }

        protected override string DebugName { get { return string.Format("Scene({0})", _location == null ? "Invalid" : _resourceManager.TransformInternalId(_location)); } }

        protected override void Execute()
        {
            var loadingFromBundle = false;
            if (_dependencies.IsValid())
            {
                IList list = _dependencies.Result as IList;
                foreach (var d in list)
                {
                    var abResource = d as IAssetBundleResource;
                    if (abResource != null && abResource.GetAssetBundle() != null)
                        loadingFromBundle = true;
                }
            }

            _scene = InternalLoadScene(_location, loadingFromBundle, _allowSceneActivation, _priority);
            ((IUpdateReceiver)this).Update(0.0f);
        }

        void IUpdateReceiver.Update(float unscaledDeltaTime)
        {
            if (_scene.operation.isDone || (!_allowSceneActivation && Mathf.Approximately(_scene.operation.progress, 0.9f)))
                Complete(_scene, true, null);
        }

        private SceneLoadData InternalLoadScene(IResourceLocation location, bool loadingFromBundle, bool allowSceneActivation, int priority)
        {
            string internalId = _resourceManager.TransformInternalId(location);
            AsyncOperation op = InternalLoad(internalId, loadingFromBundle, _loadParameters);
            op.allowSceneActivation = allowSceneActivation;
            op.priority = priority;

            return new SceneLoadData() { operation = op, scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1) };
        }

        private AsyncOperation InternalLoad(string path, bool loadingFromBundle, LoadSceneParameters parameters)
        {
#if !UNITY_EDITOR
            return SceneManager.LoadSceneAsync(path, parameters);
#else
            if (loadingFromBundle)
                return SceneManager.LoadSceneAsync(path, parameters);
            else
            {
                if (!path.ToLower().StartsWith("assets/") && !path.ToLower().StartsWith("packages/"))
                    path = "Assets/" + path;
                if (path.LastIndexOf(".unity") == -1)
                    path += ".unity";

                return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
#endif
        }
    }

    private sealed class UnloadSceneOperation : AsyncOperationBase<SceneLoadData>
    {

        private SceneLoadData _instance;
        private AsyncOperationHandle<SceneLoadData> _sceneLoadHandle;

        public void Init(AsyncOperationHandle<SceneLoadData> sceneLoadHandle)
        {
            // Line found in the SceneProvider.cs source but ReferenceCount is internal only?
            //if (sceneLoadHandle.ReferenceCount > 0)
            {
                _sceneLoadHandle = sceneLoadHandle;
                _instance = _sceneLoadHandle.Result;
            }
        }

        protected override void Execute()
        {
            if (_sceneLoadHandle.IsValid() && _instance.scene.isLoaded)
            {
                var unloadOp = SceneManager.UnloadSceneAsync(_instance.scene);
                if (unloadOp == null)
                    UnloadSceneCompleted(null);
                else
                    unloadOp.completed += UnloadSceneCompleted;
            }
            else
                UnloadSceneCompleted(null);
        }

        private void UnloadSceneCompleted(AsyncOperation obj)
        {
            if (_sceneLoadHandle.IsValid())
                Addressables.Release(_sceneLoadHandle);

            Complete(_instance, true, "");
        }

    }

}