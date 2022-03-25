using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Rewired;
using Rewired.Integration.UnityUI;
using System;

namespace rwby
{
    public class LobbyMenuInstance : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lobbyName;
        [SerializeField] private Transform lobbyPlayerList;
        [SerializeField] private Transform localPlayerList;
        [SerializeField] private GameObject lobbyPlayerListItem;
        [SerializeField] private GameObject localPlayerListItem;
        [SerializeField] private GameObject localPlayerListAddPlayerItem;
        
        [SerializeField] public Transform gamemodeOptionsList;
    }
}