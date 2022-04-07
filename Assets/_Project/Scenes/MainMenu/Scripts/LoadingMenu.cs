using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

namespace rwby.menus
{
    public class LoadingMenu : MonoBehaviour
    {
        public Dictionary<int, GameObject> loadingMenus = new Dictionary<int, GameObject>();

        public GameObject instancePrefab;

        public bool OpenMenu(int player, string loadingText)
        {
            if (loadingMenus.ContainsKey(player)) return false;
            loadingMenus.Add(player, GameObject.Instantiate(instancePrefab, transform, false));
            loadingMenus[player].GetComponentInChildren<TextMeshProUGUI>().text = loadingText;
            loadingMenus[player].SetActive(true);
            return true;
        }

        public bool CloseMenu(int player)
        {
            if (loadingMenus.ContainsKey(player) == false) return false;
            Destroy(loadingMenus[player]);
            loadingMenus.Remove(player);
            return true;
        }
    }
}