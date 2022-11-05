using Fusion;
using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct TeamDefinition : INetworkStruct
    {
        public bool editable;
        public bool removable;
        public int minimumPlayers;
        public int maximumPlayers;
        public int minCharactersPerPlayer;
        public int maxCharactersPerPlayer;
        public bool friendlyFire;
    }
}