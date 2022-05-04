using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby
{
    [System.Serializable]
    public class LobbySettingsManager
    {
        [FormerlySerializedAs("lobbyManager")] public SessionManagerGamemode sessionManagerClassic;

        public LobbySettingsManager(SessionManagerGamemode sessionManagerClassic)
        {
            this.sessionManagerClassic = sessionManagerClassic;
        }
    }
}