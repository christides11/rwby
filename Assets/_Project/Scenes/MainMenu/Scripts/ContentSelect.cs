using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
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

        public void Awake()
        {
            singleton = this;
        }

        public async UniTask<ContentSelectInstance> OpenMenu(int player, int contentType, UnityEngine.Events.UnityAction<int, ModGUIDContentReference> selectAction)
        {
            await UniTask.WaitForEndOfFrame(this);
            if (ContentSelectInstances.ContainsKey(player)) return null;

            ContentSelectInstance instance = GameObject.Instantiate(instancePrefab, transform, false);
            ContentSelectInstances.Add(player, instance);
            instance.contentSelect = this;
            bool r = await instance.Open(player, contentType, selectAction);
            if (!r)
            {
                CloseMenu(player);
                return null;
            }
            OnOpenInstance?.Invoke(this, player);
            return instance;
        }

        public void CloseMenu(int player)
        {
            if (!ContentSelectInstances.ContainsKey(player)) return;
            ContentSelectInstances[player].Close();
            GameObject.Destroy(ContentSelectInstances[player].gameObject);
            ContentSelectInstances.Remove(player);
            OnCloseInstance?.Invoke(this, player);
        }
    }
}