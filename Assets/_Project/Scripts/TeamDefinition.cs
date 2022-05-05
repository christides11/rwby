using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct TeamDefinition
    {
        public string teamName;
        public Color color;
        public int minimumPlayers;
        public int maximumPlayers;
        public int minCharactersPerPlayer;
        public int maxCharactersPerPlayer;
        public bool friendlyFire;
    }
}