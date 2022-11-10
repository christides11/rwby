using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using rwby.ui;
using TMPro;
using UnityEngine;

namespace rwby
{
    public class ContentSelectInstance : MonoBehaviour
    {
        public ContentSelect contentSelect;
        public Canvas canvas;
        public Transform contentTransform;
        public GameObject contentItem;
        public TweenSelectable leftButton;
        public TweenSelectable rightButton;

        public int contentType = 0;
        public int currentPage = 0;
        public int amtPerPage = 15;

        private int player = 0;
        private List<ModGUIDContentReference> pageContent;
        private UnityEngine.Events.UnityAction<int, ModGUIDContentReference> selectAction;
        
        
        public async UniTask<bool> Open(int player, int contentType, UnityEngine.Events.UnityAction<int, ModGUIDContentReference> selectAction)
        {
            this.player = player;
            this.contentType = contentType;
            this.selectAction = selectAction;
            gameObject.SetActive(true);
            await UniTask.WaitForEndOfFrame(this);
            
            pageContent = ContentManager.singleton.GetPaginatedContent(contentType, amtPerPage, currentPage);

            if (pageContent == null || pageContent.Count == 0)
            {
                return false;
            }

            _ = FillPage();
            return true;
        }

        public void Close()
        {
            pageContent = null;
            selectAction = null;
        }

        public async UniTask FillPage()
        {
            foreach (ModGUIDContentReference con in pageContent)
            {
                GameObject contentItemGameobject = GameObject.Instantiate(this.contentItem, contentTransform, false);
                ModGUIDContentReference contentReference = con;
                contentItemGameobject.GetComponent<Selectable>().onSubmit.AddListener(() => { selectAction.Invoke(player, contentReference); });
                contentItemGameobject.GetComponentInChildren<TextMeshProUGUI>().text = con.ToString();
            }
        }
        
        public void ChangePage(int modifier)
        {
            
        }
    }
}