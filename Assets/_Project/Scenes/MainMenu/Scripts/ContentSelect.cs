using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using rwby;
using Rewired.Integration.UnityUI;

namespace rwby.menus
{
    public class ContentSelect : MonoBehaviour
    {
        public static ContentSelect singleton;

        [SerializeField] GameObject contentBrowserLarge;
        //[SerializeField] GameObject contentBrowserInfo;

        [SerializeField] GameObject contentBrowserLarge_Content;

        public void Awake()
        {
            singleton = this;
        }

        public async UniTask OpenMenu<T>(UnityEngine.Events.UnityAction<PlayerPointerEventData, ModObjectReference> selectAction) where T : IContentDefinition
        {
            foreach (Transform child in contentBrowserLarge.transform)
            {
                Destroy(child.gameObject);
            }

            await ContentManager.singleton.LoadContentDefinitions<T>();
            List<ModObjectReference> conts = ContentManager.singleton.GetContentDefinitionReferences<T>();

            if (conts.Count == 0)
            {
                CloseMenu();
                return;
            }

            foreach (ModObjectReference con in conts)
            {
                GameObject contentItem = GameObject.Instantiate(contentBrowserLarge_Content, contentBrowserLarge.transform, false);
                PlayerPointerEventTrigger eventTrigger = contentItem.GetComponentInChildren<PlayerPointerEventTrigger>();
                ModObjectReference objectReference = con;
                eventTrigger.OnPointerClickEvent.AddListener((data) => { selectAction.Invoke(data, objectReference); });
                //eventTrigger.OnPointerClickEvent.AddListener((data) => { OnContentSelected?.Invoke(data, objectReference); CloseMenu(); });
            }

            contentBrowserLarge.SetActive(true);
        }

        public void CloseMenu()
        {
            foreach (Transform child in contentBrowserLarge.transform)
            {
                Destroy(child.gameObject);
            }

            contentBrowserLarge.SetActive(false);
        }
    }
}