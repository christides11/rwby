using UnityEngine;

namespace rwby
{
    [System.Serializable]
    public struct LocalPlayerData
    {
        public PlayerControllerType controllerType;
        public Rewired.Player rewiredPlayer;
        public GameObject camera;
    }
}