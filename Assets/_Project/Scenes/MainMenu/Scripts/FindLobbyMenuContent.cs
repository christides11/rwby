using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rwby.ui.mainmenu
{
    public class FindLobbyMenuContent : MonoBehaviour
    {
        public rwby.ui.Selectable selectable;
        public Image password;
        public TextMeshProUGUI serverName;
        public TextMeshProUGUI gamemode;
        public TextMeshProUGUI map;
        public TextMeshProUGUI playerCount;
        public TextMeshProUGUI playerLimit;
        public TextMeshProUGUI region;
    }
}