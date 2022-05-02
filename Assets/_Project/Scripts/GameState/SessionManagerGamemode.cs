using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class SessionManagerGamemode : SessionManagerBase
    {
        public delegate void SessionGamemodeAction(SessionManagerGamemode sessionManager);

        public event SessionGamemodeAction OnLobbySettingsChanged;
        public event SessionGamemodeAction OnCurrentGamemodeChanged;
        public event SessionGamemodeAction OnGamemodeSettingsChanged;
        
        [Networked(OnChanged = nameof(OnChangedGamemodeSettings))] public SessionGamemodeSettings GamemodeSettings { get; set; }
        [Networked(OnChanged = nameof(OnChangedCurrentGameMode))] public GameModeBase CurrentGameMode { get; set; }
        
        protected static void OnChangedGamemodeSettings(Changed<SessionManagerGamemode> changed)
        {
            changed.Behaviour.OnLobbySettingsChanged?.Invoke(changed.Behaviour);
        }
        
        protected static void OnChangedCurrentGameMode(Changed<SessionManagerGamemode> changed)
        {
            changed.Behaviour.OnCurrentGamemodeChanged?.Invoke(changed.Behaviour);
        }

        public override void Spawned()
        {
            base.Spawned();
            GamemodeSettings = new SessionGamemodeSettings(){ maxPlayersPerClient = 4, teams = 1 };
        }
    }
}