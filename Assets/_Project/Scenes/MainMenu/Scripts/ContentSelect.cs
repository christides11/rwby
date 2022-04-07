using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using rwby;
using Rewired.Integration.UnityUI;
using rwby.ui;

namespace rwby
{
    public class ContentSelect : MonoBehaviour
    {
        public static ContentSelect singleton;

        public Dictionary<int, ContentSelectInstance> ContentSelectInstances =
            new Dictionary<int, ContentSelectInstance>();

        public delegate void InstanceAction(ContentSelect contentSelector, int id);
        public event InstanceAction OnOpenInstance;
        public event InstanceAction OnCloseInstance;

        public ContentSelectInstance instancePrefab;
        public GameObject contentItem;
        
        public void Awake()
        {
            singleton = this;
        }

        public async UniTask OpenMenu<T>(int player, UnityEngine.Events.UnityAction<int, ModObjectReference> selectAction) where T : IContentDefinition
        {
            if (ContentSelectInstances.ContainsKey(player)) return;
            
            await ContentManager.singleton.LoadContentDefinitions<T>();
            List<ModObjectReference> conts = ContentManager.singleton.GetContentDefinitionReferences<T>();
            if (conts.Count == 0) return;

            ContentSelectInstance instance = GameObject.Instantiate(instancePrefab, transform, false);
            ContentSelectInstances.Add(player, instance);
            
            foreach (ModObjectReference con in conts)
            {
                GameObject contentItem = GameObject.Instantiate(this.contentItem, instance.contentTransform, false);
                ModObjectReference objectReference = con;
                contentItem.GetComponent<Selectable>().onSubmit.AddListener(() => { selectAction.Invoke(player, objectReference); });
                contentItem.GetComponentInChildren<TextMeshProUGUI>().text = con.ToString();
            }
            
            instance.gameObject.SetActive(true);
            OnOpenInstance?.Invoke(this, player);
        }

        public void CloseMenu(int player)
        {
            if (!ContentSelectInstances.ContainsKey(player)) return;
            GameObject.Destroy(ContentSelectInstances[player].gameObject);
            ContentSelectInstances.Remove(player);
            OnCloseInstance?.Invoke(this, player);
        }
    }
}