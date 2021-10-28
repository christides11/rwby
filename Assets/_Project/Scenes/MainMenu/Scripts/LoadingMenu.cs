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
        [SerializeField] private TextMeshProUGUI loadingTextMesh;


        public void OpenMenu(string loadingText)
        {
            loadingTextMesh.text = loadingText;
            gameObject.SetActive(true);
        }

        public void CloseMenu()
        {
            gameObject.SetActive(false);
            loadingTextMesh.text = "";
        }
    }
}