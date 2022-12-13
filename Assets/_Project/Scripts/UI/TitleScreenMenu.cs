using System;
using Animancer;
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
        
        public AnimancerComponent animancer;
        public AnimationClip clip;
        
        public override void Open(MenuDirection direction, IMenuHandler menuHandler)
        {
            base.Open(direction, menuHandler);
            gameObject.SetActive(true);
            animancer.enabled = true;
        }

        public override bool TryClose(MenuDirection direction, bool forceClose = false)
        {
            if (forceClose)
            {
                gameObject.SetActive(false);
                animancer.enabled = false;
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

        public float exitAnimBlack = 0.37f;
        private async UniTask PlayExitAnimation()
        {
            transitioning = true;
            animancer.Play(clip);
            var state = animancer.States.GetOrCreate(clip);
            state.Events.EndEvent = new AnimancerEvent(1.0f, OnAnimEnd);
            await UniTask.WaitUntil(() => state.Time >= exitAnimBlack);
            currentHandler.Forward((int)MainMenuType.MAIN_MENU, false);
        }

        private void OnAnimEnd()
        {
            transitioning = false;
            animancer.enabled = false;
            gameObject.SetActive(false);
        }
    }
}