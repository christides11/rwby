using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace rwby.ui.mainmenu
{
    public class JoinLobbyMenu : MenuBase
    {
        public TextMeshProUGUI menuLabel;
        public TextMeshProUGUI menuDescription;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            menuLabel.text = "JOIN LOBBY";
            menuDescription.text = "";
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            gameObject.SetActive(false);
            return true;
        }
    }
}