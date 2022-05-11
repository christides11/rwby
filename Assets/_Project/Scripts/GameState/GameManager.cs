using System;
using Cysharp.Threading.Tasks;
using Fusion;
using Rewired.UI.ControlMapper;
using Rewired;
using rwby.ui.mainmenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace rwby
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager singleton;

        [SerializeField] private ModLoader modLoader;
        public ContentManager contentManager;
        public ControlMapper cMapper;
        public LocalPlayerManager localPlayerManager;
        public ControllerAssignmentMenu controllerAssignmentMenu;
        public LoadingMenu loadingMenu;
        public ProfilesManager profilesManager;
        public NetworkManager networkManager;

        public Settings settings;
        public ContentGUID internalModGUID = new ContentGUID(8, "");

        public async UniTask Initialize()
        {
            singleton = this;
            await modLoader.Initialize();
            contentManager.Initialize();
            localPlayerManager.Initialize();
            profilesManager.Initialize();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                QualitySettings.vSyncCount = 0;
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                QualitySettings.vSyncCount = 1;
            }

            /*
            if (Input.GetKeyDown(KeyCode.F4))
            {
                var player = ReInput.players.GetPlayer(0);
                
                // Disable all Rule Sets
                foreach(var ruleSet in player.controllers.maps.layoutManager.ruleSets) {
                    ruleSet.enabled = false;
                }
                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "k&m_default").enabled = true;
                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "js_default").enabled = true;
                player.controllers.maps.layoutManager.Apply();
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                var player = ReInput.players.GetPlayer(0);
                
                // Disable all Rule Sets
                foreach(var ruleSet in player.controllers.maps.layoutManager.ruleSets) {
                    ruleSet.enabled = false;
                }

                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "k&m_keyboard").enabled = true;
                player.controllers.maps.layoutManager.ruleSets.Find(item => item.tag == "js_default").enabled = true;
                player.controllers.maps.layoutManager.Apply();
            }*/
        }

        public SessionManagerGamemode gamemodeSessionHandlerPrefab;
        public virtual async UniTask<int> HostGamemodeSession(string lobbyName, int playerCount, bool privateLobby)
        {
            int sessionHandlerID = networkManager.CreateSessionHandler();
            StartGameResult result = await networkManager.sessions[sessionHandlerID].HostSession(lobbyName, playerCount, privateLobby);
            if (result.Ok == false)
            {
                Debug.Log($"Failed to host gamemode session: {result.ShutdownReason}");
                networkManager.DestroySessionHandler(sessionHandlerID);
                return -1;
            }
            
            SessionManagerGamemode go = networkManager.sessions[sessionHandlerID]._runner.Spawn(gamemodeSessionHandlerPrefab);
            return sessionHandlerID;
        }

        public virtual async UniTask<Scene> LoadScene(CustomSceneRef sceneReference, LoadSceneParameters parameters)
        {
            if (sceneReference.mapReference.modGUID == internalModGUID)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(sceneReference.sceneIdentifier);
                var s = SceneManager.LoadSceneAsync(sceneReference.sceneIdentifier, parameters);

                bool alreadyHandled = false;
                Scene sceneRef = default;

                // if there's a better way to get scene struct more reliably I'm dying to know
                UnityAction<Scene, LoadSceneMode> sceneLoadedHandler = (scene, _) =>
                {
                    if (scene.path == scenePath)
                    {
                        Assert.Check(!alreadyHandled);
                        alreadyHandled = true;
                        sceneRef = scene;
                    }
                };
                SceneManager.sceneLoaded += sceneLoadedHandler;
                
                await s;
                SceneManager.sceneLoaded -= sceneLoadedHandler;
                return sceneRef;
            }
            
            IMapDefinition mapDefinition = contentManager.GetContentDefinition<IMapDefinition>(sceneReference.mapReference);

            var result = await mapDefinition.LoadScene(sceneReference.sceneIdentifier, parameters);
            return result;
        }

        public virtual string[] GetSceneNames(CustomSceneRef sceneReference){
            if (sceneReference.mapReference.modGUID == internalModGUID)
            {
                return new string[] { SceneManager.GetSceneByBuildIndex(sceneReference.sceneIdentifier).name };
            }

            IMapDefinition mapDefinition = contentManager.GetContentDefinition<IMapDefinition>(sceneReference.mapReference);

            return mapDefinition.GetSceneNames().ToArray();
        }

        public virtual string GetSceneName(CustomSceneRef sceneReference)
        {
            if (sceneReference.mapReference.modGUID == internalModGUID)
            {
                return SceneManager.GetSceneByBuildIndex(sceneReference.sceneIdentifier).name;
            }
            
            IMapDefinition mapDefinition = contentManager.GetContentDefinition<IMapDefinition>(sceneReference.mapReference);

            return mapDefinition.GetSceneNames()[sceneReference.sceneIdentifier];
        }
    }
}