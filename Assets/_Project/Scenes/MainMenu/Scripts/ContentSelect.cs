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

        public async UniTask OpenMenu(int player, int contentType, UnityEngine.Events.UnityAction<int, ModGUIDContentReference> selectAction)
        {
            if (ContentSelectInstances.ContainsKey(player)) return;
            
            await ContentManager.singleton.LoadContentDefinitions(contentType);
            List<ModGUIDContentReference> conts = ContentManager.singleton.GetContentDefinitionReferences(contentType);
            if (conts.Count == 0) return;

            ContentSelectInstance instance = GameObject.Instantiate(instancePrefab, transform, false);
            ContentSelectInstances.Add(player, instance);
            
            foreach (ModGUIDContentReference con in conts)
            {
                GameObject contentItemGameobject = GameObject.Instantiate(this.contentItem, instance.contentTransform, false);
                ModGUIDContentReference contentReference = con;
                contentItemGameobject.GetComponent<Selectable>().onSubmit.AddListener(() => { selectAction.Invoke(player, contentReference); });
                contentItemGameobject.GetComponentInChildren<TextMeshProUGUI>().text = con.ToString();
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