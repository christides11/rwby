using System;
using System.Collections.Generic;
using System.Linq;
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

        [NonSerialized] private Dictionary<int, AssetReferenceT<T>> content = new Dictionary<int, AssetReferenceT<T>>();
        [NonSerialized] private Dictionary<int, AsyncOperationHandle<T>> contentHandles = new Dictionary<int, AsyncOperationHandle<T>>();
        
        public override void Initialize()
        {
            content.Clear();
            GUIDToInt.Clear();
            for (int i = 0; i < references.Count; i++)
            {
                content.Add(i, references[i].asset);
                GUIDToInt.Add(references[i].identifier, i);
                IntToGUID.Add(i, references[i].identifier);
            }
        }

        public override bool ContentExist(ContentGUID contentIdentfier)
        {
            return ContentExist(GUIDToInt[contentIdentfier]);
        }

        public override bool ContentExist(int contentIdentifier)
        {
            return content.ContainsKey(contentIdentifier) ? true : false;
        }

        public override async UniTask<List<int>> LoadContentDefinitions()
        {
            List<int> results = new List<int>();
            // All of the content is already loaded.
            if (contentHandles.Count == references.Count)
            {
                return results;
            }
            try
            {
                foreach (var contentIdentifier in content.Keys)
                {
                    bool r = await LoadContentDefinition(contentIdentifier);
                    if(r) results.Add(contentIdentifier);
                }
                return results;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            return results;
        }

        public override async UniTask<bool> LoadContentDefinition(ContentGUID contentIdentifier)
        {
            return await LoadContentDefinition(GUIDToInt[contentIdentifier]);
        }

        public override async UniTask<bool> LoadContentDefinition(int index)
        {
            // Content doesn't exist.
            if (content.ContainsKey(index) == false)
            {
                Debug.LogError($"Error loading {IntToGUID[index].ToString()}: Content does not exist.");
                return false;
            }

            bool handleExist = contentHandles.ContainsKey(index) == true;
            // Content already loaded.
            if (handleExist && contentHandles[index].Status == AsyncOperationStatus.Succeeded)
            {
                return true;
            }

            if (handleExist == false)
            {
                contentHandles.Add(index, Addressables.LoadAssetAsync<T>(content[index]));
            }

            if(contentHandles[index].IsDone == false || contentHandles[index].Status == AsyncOperationStatus.Failed)
            {
                await contentHandles[index];
            }

            if(contentHandles[index].Status == AsyncOperationStatus.Succeeded)
            {
                contentHandles[index].Result.Identifier = index;
                return true;
            }

            Debug.LogError($"Error loading {GUIDToInt.First(x => x.Value == index).Key.ToString()}: could not load.");
            contentHandles.Remove(index);
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

        public override IContentDefinition GetContentDefinition(ContentGUID contentIdentifier)
        {
            // Content does not exist, or was not loaded.
            if (contentHandles.ContainsKey(GUIDToInt[contentIdentifier]) == false)
            {
                return null;
            }
            return contentHandles[GUIDToInt[contentIdentifier]].Result;
        }

        public override IContentDefinition GetContentDefinition(int index)
        {
            if (contentHandles.ContainsKey(index) == false)
            {
                return null;
            }
            return contentHandles[index].Result;
        }

        public override void UnloadContentDefinitions()
        {
            foreach (var v in contentHandles)
            {
                UnloadContentDefinition(v.Key);
            }
            contentHandles.Clear();
        }

        public override void UnloadContentDefinition(ContentGUID contentIdentifier)
        {
            if (contentHandles.ContainsKey(GUIDToInt[contentIdentifier]) == false) return;

            if(contentHandles[GUIDToInt[contentIdentifier]].Status == AsyncOperationStatus.Succeeded) Addressables.Release(contentHandles[GUIDToInt[contentIdentifier]]);
            contentHandles.Remove(GUIDToInt[contentIdentifier]);
        }

        public override void UnloadContentDefinition(int index)
        {
            if (contentHandles.ContainsKey(index) == false) return;

            if(contentHandles[index].Status == AsyncOperationStatus.Succeeded) Addressables.Release(contentHandles[index]);
            contentHandles.Remove(index);
        }
    }
}