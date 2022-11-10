using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby.core.versus
{
    public class GamemodeVersusTeardown : NetworkBehaviour, IGamemodeTeardown
    {
        public GamemodeVersus gamemode;

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
            gamemode.sessionManager.clientContentUnloaderService.TellClientsToUnload(gamemode.hudBankContentReference);
            gamemode.sessionManager.clientContentUnloaderService.TellClientsToUnload(gamemode.Map);

            var loadedContentList = gamemode.sessionManager.loadedContent;
            foreach (var contentItem in loadedContentList)
            {
                gamemode.sessionManager.clientContentUnloaderService.TellClientsToUnload(contentItem);
            }
            
            
            gamemode.sessionManager.currentLoadedScenes.Clear();
            gamemode.sessionManager.currentLoadedScenes.Add(new CustomSceneRef(new ContentGUID(8), 0, 1));
            Runner.SetActiveScene(Runner.CurrentScene+1);
            
            Resources.UnloadUnusedAssets();
        }
    }
}