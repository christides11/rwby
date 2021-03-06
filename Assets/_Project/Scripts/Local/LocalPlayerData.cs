using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct LocalPlayerData
    {
        public bool isValid;
        public PlayerControllerType controllerType;
        public Rewired.Player rewiredPlayer;
        public Camera camera;
    }
}