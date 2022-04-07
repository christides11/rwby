using UnityEngine;

namespace rwby.menus
{
    public class OnlineMenu : MonoBehaviour
    {
        [Header("Menus")] 
        public ModeSelectMenu modeSelectMenu;
        public FindLobbyMenu findLobbyMenu;
        public HostLobbyMenu hostLobbyMenu;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void BUTTON_FindLobby()
        {
            findLobbyMenu.OpenMenu();
            Close();
        }

        public void BUTTON_HostLobby()
        {
            hostLobbyMenu.OpenMenu();
            Close();
        }

        public void BUTTON_QuickJoin()
        {
            Close();
        }
        
        public void BUTTON_Back()
        {
            modeSelectMenu.Open();
            Close();
        }
    }
}