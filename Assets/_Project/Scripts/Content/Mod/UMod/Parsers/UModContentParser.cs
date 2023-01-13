using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UMod;

namespace rwby
{
    [System.Serializable]
    public class UModContentParser<T> : IContentParser where T : IContentDefinition
    {
        [SerializeField] private List<UModModDefinition.IdentifierAssetStringRelation> references
            = new List<UModModDefinition.IdentifierAssetStringRelation>();
        
        [NonSerialized] private Dictionary<int, UModAssetReference> content = new Dictionary<int, UModAssetReference>();
        [NonSerialized] private Dictionary<int, ModAsyncOperation<T>> contentHandles = new Dictionary<int, ModAsyncOperation<T>>();
        
        public override void Initialize()
        {
            content.Clear();
            GUIDToInt.Clear();
            IntToGUID.Clear();
            for (int i = 0; i < references.Count; i++)
            {
                content.Add(i, references[i].asset);
                GUIDToInt.Add(references[i].identifier, i);
                IntToGUID.Add(i, references[i].identifier);
            }
        }
        
        public override bool ContentExist(string contentIdentfier)
        {
            return ContentExist(GUIDToInt[contentIdentfier]);
        }

        public override bool ContentExist(int contentIdentifier)
        {
            return content.ContainsKey(contentIdentifier) ? true : false;
        }

        public override async UniTask<List<int>> LoadContentDefinitions(LoadedModDefinition modDefinition)
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
                    bool r = await LoadContentDefinition(modDefinition, contentIdentifier);
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

        public override async UniTask<bool> LoadContentDefinition(LoadedModDefinition modDefinition, string contentIdentifier)
        {
            return await LoadContentDefinition(modDefinition, GUIDToInt[contentIdentifier]);
        }

        public override async UniTask<bool> LoadContentDefinition(LoadedModDefinition modDefinition, int index)
        {
            LoadedUModModDefinition umodModDefinition = (LoadedUModModDefinition)modDefinition;
            // Content doesn't exist.
            if (content.ContainsKey(index) == false)
            {
                Debug.LogError($"Error loading {IntToGUID[index].ToString()}: Content does not exist.");
                return false;
            }
            
            bool handleExist = contentHandles.ContainsKey(index) == true;
            // Content already loaded.
            if (handleExist && contentHandles[index].IsSuccessful)
            {
                return true;
            }
            
            if (handleExist == false)
            {
                contentHandles.Add(index, umodModDefinition.host.Assets.LoadAsync<T>(content[index].lookupTable[content[index].tableID]));
            }
            
            if(contentHandles[index].IsDone == false || !contentHandles[index].IsSuccessful)
            {
                await contentHandles[index];
            }

            if(contentHandles[index].IsSuccessful)
            {
                if (contentHandles[index].Result.GetType().IsAssignableFrom(typeof(IUModModHostRef)))
                {
                    ((IUModModHostRef)contentHandles[index].Result).modHost = umodModDefinition.host;
                }
                contentHandles[index].Result.Identifier = index;
                return true;
            }

            Debug.LogError($"Error loading {GUIDToInt.First(x => x.Value == index).Key.ToString()}: could not load.");
            contentHandles.Remove(index);
            return false;
        }
    }
}