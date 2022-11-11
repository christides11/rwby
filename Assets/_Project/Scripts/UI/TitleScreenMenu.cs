using System;
using Cysharp.Threading.Tasks;
using Rewired;
using UnityEngine;
using UnityEngine.Serialization;

namespace rwby.ui.mainmenu
{
    public class TitleScreenMenu : MainMenuMenu
    {
        [FormerlySerializedAs("modeSelectMenu")] public MainMenu mainMenu;

        [ActionIdProperty(typeof(Action))]
        public int[] validActions;

        public ModObjectItemReference menuSound;

        public Animation animation;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            animation.enabled = true;
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if (forceClose)
            {
                gameObject.SetActive(false);
                animation.enabled = false;
            }
            return true;
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < validActions.Length; i++)
            {
                if (ReInput.players.SystemPlayer.GetButton(validActions[i]))
                {
                    NextMenu();
                    return;
                }
            }

            if (Input.anyKey)
            {
                NextMenu();
                return;
            }
        }

        private bool transitioning = false;
        private void NextMenu()
        {
            if (transitioning) return;
            transitioning = true;
            GameManager.singleton.soundManager.Play(menuSound, 1.0f, 1.0f, transform.position);
            _ = PlayExitAnimation();
        }

        public float exitAnimBlack = 0.10f;
        public float exitAnimEnd = 0.10f;
        private async UniTask PlayExitAnimation()
        {
            animation.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(exitAnimBlack));
            currentHandler.Forward((int)MainMenuType.MAIN_MENU);
            await UniTask.Delay(TimeSpan.FromSeconds(exitAnimEnd));
            transitioning = false;
            gameObject.SetActive(false);
        }
    }
}