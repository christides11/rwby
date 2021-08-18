using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using rwby;

namespace rwby.menus
{
    public class ContentSelect : MonoBehaviour
    {
        public delegate void EmptyAction();
        public delegate void ContentAction(ModObjectReference gamemodeReference);
        public event EmptyAction OnMenuClosed;
        public event ContentAction OnContentSelected;

        public GameObject contentPrefab;
        public Transform contentHolderPrefab;

        public TextMeshProUGUI contentName;

        private ModObjectReference currentSelectedContent;

        private ContentType contentType;

        public void CloseMenu()
        {
            gameObject.SetActive(false);
            OnMenuClosed?.Invoke();
        }

        public async UniTask OpenMenu(ContentType contentType)
        {
            this.contentType = contentType;
            await FillContentHolder();
            gameObject.SetActive(true);
        }

        private async UniTask FillContentHolder()
        {
            foreach (Transform child in contentHolderPrefab)
            {
                Destroy(child.gameObject);
            }

            bool loadResult = await ContentManager.instance.LoadContentDefinitions(contentType);

            List<ModObjectReference> contentReferences = ContentManager.instance.GetContentDefinitionReferences(contentType);

            foreach (ModObjectReference contentRef in contentReferences)
            {
                ModObjectReference cref = contentRef;
                GameObject gamemodeContentObject = GameObject.Instantiate(contentPrefab, contentHolderPrefab, false);
                gamemodeContentObject.GetComponentInChildren<TextMeshProUGUI>().text = cref.ToString();
                gamemodeContentObject.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { SelectContent(cref); });
            }

            if (contentReferences.Count > 0)
            {
                SelectContent(contentReferences[0]);
            }
        }

        private void SelectContent(ModObjectReference contentReference)
        {
            currentSelectedContent = contentReference;
            contentName.text = contentReference.ToString();
        }

        public void SelectContent()
        {
            OnContentSelected?.Invoke(currentSelectedContent);
        }
    }
}