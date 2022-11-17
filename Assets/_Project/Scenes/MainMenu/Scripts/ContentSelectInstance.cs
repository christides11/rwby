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

        public List<string> tagsToFind = new List<string>();
        
        
        public async UniTask<bool> Open(int player, int contentType, UnityEngine.Events.UnityAction<int, ModGUIDContentReference> selectAction)
        {
            this.player = player;
            this.contentType = contentType;
            this.selectAction = selectAction;
            gameObject.SetActive(true);
            await UniTask.WaitForEndOfFrame(this);
            
            pageContent = await ContentManager.singleton.GetPaginatedContent(contentType, amtPerPage, currentPage, new HashSet<string>(tagsToFind));

            if (pageContent == null || pageContent.Count == 0)
            {
                return false;
            }

            _ = FillPage();
            return true;
        }

        public void Close()
        {
            UnloadCurrentPage();
            tagsToFind = new List<string>();
            pageContent = null;
            selectAction = null;
        }

        private void UnloadCurrentPage()
        {
            foreach (var contentReference in pageContent)
            {
                ContentManager.singleton.UnloadContentDefinition(contentReference, ignoreIfTracked: true);
            }
        }

        public async UniTask FillPage()
        {
            foreach (ModGUIDContentReference con in pageContent)
            {
                var contentDefinition = ContentManager.singleton.GetContentDefinition(con);
                GameObject contentItemGameobject = GameObject.Instantiate(this.contentItem, contentTransform, false);
                ModGUIDContentReference contentReference = con;
                contentItemGameobject.GetComponent<Selectable>().onSubmit.AddListener(() => { selectAction.Invoke(player, contentReference); });
                contentItemGameobject.GetComponentInChildren<TextMeshProUGUI>().text = contentDefinition.Name;
            }
        }
        
        public void ChangePage(int modifier)
        {
            
        }
    }
}