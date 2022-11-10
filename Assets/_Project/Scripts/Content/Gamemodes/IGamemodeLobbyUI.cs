using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public interface IGamemodeLobbyUI
    {
        void AddGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false);
        void ClearGamemodeSettings(int player, LobbySettingsMenu settingsMenu, bool local = false);
    }
}