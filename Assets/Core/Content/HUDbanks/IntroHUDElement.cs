using System;
using Cysharp.Threading.Tasks;
using TMPro;

namespace rwby
{
    public class IntroHUDElement : HUDElement
    {
        public TextMeshProUGUI readyText;
        public TextMeshProUGUI fightText;

        public override void InitializeElement(BaseHUD parentHUD)
        {
            base.InitializeElement(parentHUD);
            _ = DoIntro();
        }

        public async UniTask DoIntro()
        {
            readyText.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(2.0f));
            readyText.gameObject.SetActive(false);
            fightText.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            fightText.gameObject.SetActive(false);
        }
    }
}