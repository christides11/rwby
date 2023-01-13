using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby.core.training
{
    public class GamemodeTrainingTeardown : NetworkBehaviour, IGamemodeTeardown
    {
        public GamemodeTraining gamemode;

        public void Teardown()
        {
            MapHandler[] mapHandler = Runner.SimulationUnityScene.FindObjectsOfTypeInOrder<MapHandler>();
            if (mapHandler.Length > 0)
            {
                for (int i = 0; i < mapHandler.Length; i++)
                {
                    mapHandler[i].Teardown(gamemode);
                }
            }
            
            gamemode.startingPoints.Clear();
            gamemode.respawnPoints.Clear();
            gamemode.sessionManager.clientContentUnloaderService.TellClientsToUnload(gamemode.contentManager.ConvertStringToGUIDReference(gamemode.hudbankStringReference));
            gamemode.sessionManager.clientContentUnloaderService.TellClientsToUnload(gamemode.Map);

            var loadedContentList = gamemode.sessionManager.loadedContent;
            foreach (var contentItem in loadedContentList)
            {
                gamemode.sessionManager.clientContentUnloaderService.TellClientsToUnload(contentItem);
            }
            
            
            gamemode.sessionManager.currentLoadedScenes.Clear();
            foreach (var s in GameManager.singleton.networkManager.GetSessionHandlerByRunner(Runner).defaultSceneList)
            {
                gamemode.sessionManager.currentLoadedScenes.Add(s);
            }
            Runner.SetActiveScene(Runner.CurrentScene+1);
            
            Resources.UnloadUnusedAssets();
        }
    }
}