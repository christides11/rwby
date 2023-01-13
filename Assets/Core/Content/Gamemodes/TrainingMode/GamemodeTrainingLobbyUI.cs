using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using rwby.ui;
using UnityEngine;

namespace rwby.core.training
{
    public class GamemodeTrainingLobbyUI : MonoBehaviour, IGamemodeLobbyUI
    {
        public GamemodeTraining gamemode;
        
        public void AddGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false)
        {
            ModIDContentReference mapRef = local ? gamemode.localMap : gamemode.Map;

            IMapDefinition mapDefinition = ContentManager.singleton.GetContentDefinition<IMapDefinition>(mapRef);
            string mapName = mapDefinition != null ? mapDefinition.Name : "None";
            if (settingsMenu.idContentDictionary.ContainsKey("Map"))
            {
                ((ContentButtonStringValue)settingsMenu.idContentDictionary["Map"]).valueString.text = mapName;
            }
            else
            {
                settingsMenu.AddStringValueOption("Map", "Map", mapName).onSubmit.AddListener(async () => { await OpenMapSelection(player, local); });
            }
        }

        public void ClearGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false)
        {
            settingsMenu.ClearOption("Map");
        }

        private async UniTask OpenMapSelection(int player, bool local = false)
        {
            await ContentSelect.singleton.OpenMenu(player, (int)ContentType.Map,(a, b) =>
            {
                ContentSelect.singleton.CloseMenu(player);
                _ = SelectMap(b, local);
            });
        }

        private async UniTask SelectMap(ModIDContentReference mapReference, bool local)
        {
            if (local)
            {
                if (gamemode.localMap.IsValid() && gamemode.localMap != mapReference)
                {
                    ContentManager.singleton.UnloadContentDefinition(gamemode.localMap);
                }
                gamemode.localMap = mapReference;
                _ = await ContentManager.singleton.LoadContentDefinition(gamemode.localMap);
                gamemode.WhenGamemodeSettingsChanged(true);
            }
            else
            {
                if (gamemode.Map.IsValid() && gamemode.Map != mapReference)
                {
                    gamemode.sessionManager.clientContentUnloaderService.TellClientsToUnload(gamemode.Map);
                }
                gamemode.Map = mapReference;
                gamemode.WhenGamemodeSettingsChanged();
            }
        }
    }
}