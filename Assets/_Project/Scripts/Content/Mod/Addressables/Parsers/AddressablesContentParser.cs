using System;
using System.Collections.Generic;
using UnityEngine;
using static rwby.AddressablesModDefinition;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace rwby
{
    [System.Serializable]
    public class AddressablesContentParser<T> : IContentParser where T : IContentDefinition
    {
        [SerializeField] private List<IdentifierAssetReferenceRelation<T>> references = new List<IdentifierAssetReferenceRelation<T>>();

        [NonSerialized] private Dictionary<string, AssetReferenceT<T>> content = new Dictionary<string, AssetReferenceT<T>>();
        [NonSerialized] private Dictionary<string, AsyncOperationHandle<T>> contentHandles = new Dictionary<string, AsyncOperationHandle<T>>();

        public override void Initialize()
        {
            content.Clear();
            foreach (IdentifierAssetReferenceRelation<T> a in references)
            {
                content.Add(a.identifier, a.asset);
            }
        }

        public override bool ContentExist(string contentIdentfier)
        {
            return content.ContainsKey(contentIdentfier) ? true : false;
        }

        public override async UniTask<bool> LoadContentDefinitions()
        {
            // All of the content is already loaded.
            if (contentHandles.Count == references.Count)
            {
                return true;
            }
            try
            {
                foreach (string contentIdentifier in content.Keys)
                {
                    await LoadContentDefinition(contentIdentifier);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            return false;
        }

        public override async UniTask<bool> LoadContentDefinition(string contentIdentifier)
        {
            // Content doesn't exist.
            if (content.ContainsKey(contentIdentifier) == false)
            {
                Debug.LogError($"Error loading {contentIdentifier}: Content does not exist.");
                return false;
            }

            bool handleExist = contentHandles.ContainsKey(contentIdentifier) == true;
            // Content already loaded.
            if (handleExist && contentHandles[contentIdentifier].Status == AsyncOperationStatus.Succeeded)
            {
                return true;
            }

            if (handleExist == false)
            {
                contentHandles.Add(contentIdentifier, Addressables.LoadAssetAsync<T>(content[contentIdentifier]));
            }

            if(contentHandles[contentIdentifier].IsDone == false || contentHandles[contentIdentifier].Status == AsyncOperationStatus.Failed)
            {
                await contentHandles[contentIdentifier];
            }

            if(contentHandles[contentIdentifier].Status == AsyncOperationStatus.Succeeded)
            {
                contentHandles[contentIdentifier].Result.Identifier = contentIdentifier;
                return true;
            }

            Debug.LogError($"Error loading {contentIdentifier}: could not load.");
            contentHandles.Remove(contentIdentifier);
            return false;
        }

        public override List<IContentDefinition> GetContentDefinitions()
        {
            List<IContentDefinition> contentList = new List<IContentDefinition>();
            foreach (var content in contentHandles.Values)
            {
                contentList.Add(content.Result);
            }
            return contentList;
        }

        public override IContentDefinition GetContentDefinition(string contentIdentifier)
        {
            // Content does not exist, or was not loaded.
            if (contentHandles.ContainsKey(contentIdentifier) == false)
            {
                return null;
            }
            return contentHandles[contentIdentifier].Result;
        }

        public override void UnloadContentDefinitions()
        {
            foreach (var v in contentHandles)
            {
                Addressables.Release(content[v.Key]);
            }
            contentHandles.Clear();
        }

        public override void UnloadContentDefinition(string contentIdentifier)
        {
            if (contentHandles.ContainsKey(contentIdentifier) == false) return;

            Addressables.Release(content[contentIdentifier]);
            contentHandles.Remove(contentIdentifier);
        }
    }
}