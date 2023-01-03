using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using rwby.ui;
using UnityEngine;

namespace rwby.core.versus
{
    public class GamemodeVersusLobbyUI : MonoBehaviour, IGamemodeLobbyUI
    {
        public GamemodeVersus gamemode;
        
        public void ClearGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false)
        {
            settingsMenu.ClearOption("Map");
            settingsMenu.ClearOption("points");
            settingsMenu.ClearOption("timelimit");
        }
        
        public void AddGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false)
        {
            ModGUIDContentReference mapRef = local ? gamemode.localMap : gamemode.Map;

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

            int pointsToWin = local ? gamemode.localPointsRequired : gamemode.PointsRequired;
            if (settingsMenu.idContentDictionary.ContainsKey("points"))
            {
                ((ContentButtonIntValue)settingsMenu.idContentDictionary["points"]).intValueText.text = pointsToWin.ToString();
            }
            else
            {
                var pointsValueOption = settingsMenu.AddIntValueOption("points", "Points to Win", pointsToWin);
                pointsValueOption.addButton.onSubmit.AddListener(() => { ModifyPoints(1); });
                pointsValueOption.subtractButton.onSubmit.AddListener(() => { ModifyPoints(-1); });
            }

            int timeLimitMinutes = gamemode.Object != null ? gamemode.TimeLimitMinutes : gamemode.localTimeLimitMinutes;
            if (settingsMenu.idContentDictionary.ContainsKey("timelimit"))
            {
                ((ContentButtonIntValue)settingsMenu.idContentDictionary["timelimit"]).intValueText.text = timeLimitMinutes.ToString();
            }
            else
            {
                var timeLimitOption = settingsMenu.AddIntValueOption("timelimit", "Time Limit (Minutes)", timeLimitMinutes);
                timeLimitOption.addButton.onSubmit.AddListener(() => { ModifyTimeLimit(1); });
                timeLimitOption.subtractButton.onSubmit.AddListener(() => { ModifyTimeLimit(-1); });
            }
        }

        private void ModifyTimeLimit(int p0)
        {
            if (gamemode.Object != null)
            {
                gamemode.TimeLimitMinutes += p0;
                gamemode.WhenGamemodeSettingsChanged();
            }
            else
            {
                gamemode.localTimeLimitMinutes += p0;
                gamemode.WhenGamemodeSettingsChanged(local:true);
            }
        }

        private void ModifyPoints(int p0)
        {
            if (gamemode.Object != null)
            {
                gamemode.PointsRequired += p0;
                gamemode.WhenGamemodeSettingsChanged();
            }
            else
            {
                gamemode.localPointsRequired += p0;
                gamemode.WhenGamemodeSettingsChanged(local:true);
            }
        }

        private async UniTask OpenMapSelection(int player, bool local = false)
        {
            await ContentSelect.singleton.OpenMenu(player, (int)ContentType.Map,(a, b) =>
            {
                ContentSelect.singleton.CloseMenu(player);
                _ = SelectMap(b, local);
            });
        }

        private async UniTask SelectMap(ModGUIDContentReference mapReference, bool local)
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