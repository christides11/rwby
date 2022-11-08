 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace rwby.ui
{
    public class PauseMenu : MonoBehaviour
    {
        public delegate void EmptyAction(int playerID);
        public static event EmptyAction onInstanceOpened;
        public static event EmptyAction OnInstanceClosed;

        public static PauseMenu singleton;

        public PauseMenuInstance instancePrefab;

        public Dictionary<int, PauseMenuInstance> pauseMenus = new Dictionary<int, PauseMenuInstance>();

        private void Awake()
        {
            singleton = this;
        }

        public bool OpenMenu(int playerID)
        {
            if (pauseMenus.ContainsKey(playerID)) return false;
            pauseMenus.Add(playerID, GameObject.Instantiate(instancePrefab, transform, false));
            pauseMenus[playerID].gameObject.SetActive(true);
            pauseMenus[playerID].Open();
            pauseMenus[playerID].playerID = playerID;
            onInstanceOpened?.Invoke(playerID);
            return true;
        }

        public bool CloseMenu(int playerID, bool forceClose = false)
        {
            if (!pauseMenus.ContainsKey(playerID)) return true;
            if (!pauseMenus[playerID].Close()) return false;
            Destroy(pauseMenus[playerID].gameObject);
            pauseMenus.Remove(playerID);
            OnInstanceClosed?.Invoke(playerID);
            return true;
        }

        public bool IsPlayerPaused(int player)
        {
            return pauseMenus.ContainsKey(player);
        }
    }
}