using Rewired;
using UnityEngine;

namespace rwby.menus
{
    public class TitleScreenMenu : MonoBehaviour
    {
        public ModeSelectMenu modeSelectMenu;

        private void Update()
        {
            if (ReInput.players.SystemPlayer.GetAnyButtonDown())
            {
                modeSelectMenu.Open();
                gameObject.SetActive(false);
            }
        }
    }
}