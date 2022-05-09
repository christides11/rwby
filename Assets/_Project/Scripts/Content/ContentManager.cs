using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

namespace rwby
{
    public class ContentManager : MonoBehaviour
    {
        public static ContentManager singleton;

        public Dictionary<string, HashSet<(int, string)>> currentlyLoadedContent =
            new Dictionary<string, HashSet<(int, string)>>(); 

        [SerializeField] private ModLoader modLoader;

        public void Initialize()
        {
            singleton = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                foreach (var v in currentlyLoadedContent)
                {
                    Debug.Log($"{v.Key.ToString()} : {v.Value.Count}");
                    foreach (var b in v.Value)
                    {
                        Debug.Log(b.ToString());
                    }
                }
            }
        }

        public bool ContentExist<T>(ModObjectGUIDReference objectReference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(objectReference.modGUID, out LoadedModDefinition mod)) return false;
            if (!mod.definition.ContentParsers.TryGetValue(objectReference.contentType, out IContentParser parser)) return false;
            return parser.ContentExist(objectReference.contentGUID);
        }

        #region Loading
        public async UniTask LoadContentDefinitions(int contentType)
        {
            foreach (var m in modLoader.loadedModsByGUID.Keys)
            {
                await LoadContentDefinitions(m, contentType);
            }
        }

        public async UniTask<bool> LoadContentDefinitions(string modGUID, int contentType)
        {
            if (!modLoader.TryGetLoadedMod(modGUID, out LoadedModDefinition mod)) return false;
            if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) return false;
            var result = await parser.LoadContentDefinitions();
            foreach (var r in result) TrackItem(new ModObjectGUIDReference(){ modGUID = modGUID, contentType = contentType, contentGUID = r});
            return true;
        }

        public async UniTask<bool> LoadContentDefinition(ModObjectGUIDReference objectReference)
        {
            if (!modLoader.TryGetLoadedMod(objectReference.modGUID, out LoadedModDefinition mod)) return false;
            if (!mod.definition.ContentParsers.TryGetValue(objectReference.contentType, out IContentParser parser)) return false;
            bool result = await parser.LoadContentDefinition(objectReference.contentGUID);
            if(result) TrackItem(objectReference);
            return result;
        }
        #endregion

        #region Getting
        public List<ModObjectGUIDReference> GetContentDefinitionReferences(int contentType)
        {
            List<ModObjectGUIDReference> content = new List<ModObjectGUIDReference>();
            foreach (var m in modLoader.loadedModsByGUID.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences(m, contentType));
            }
            return content;
        }

        public List<ModObjectGUIDReference> GetContentDefinitionReferences(string modGUID, int contentType)
        {
            List<ModObjectGUIDReference> content = new List<ModObjectGUIDReference>();

            if (!modLoader.TryGetLoadedMod(modGUID, out LoadedModDefinition mod)) return content;
            if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) return content;

            List<IContentDefinition> fds = parser.GetContentDefinitions();
            if (fds == null) return content;

            foreach (IContentDefinition fd in fds)
                content.Add(new ModObjectGUIDReference(){ modGUID = modGUID, contentType = contentType, contentGUID = fd.Identifier });
            return content;
        }

        public List<T> GetContentDefinitions<T>(int contentType) where T : IContentDefinition
        {
            List<T> contents = new List<T>();

            foreach (var m in modLoader.loadedModsByGUID.Keys)
            {
                contents.InsertRange(contents.Count, GetContentDefinitions<T>(m, contentType));
            }
            return contents;
        }

        public List<T> GetContentDefinitions<T>(string modGUID, int contentType) where T : IContentDefinition
        {
            List<T> contents = new List<T>();

            if (!modLoader.TryGetLoadedMod(modGUID, out LoadedModDefinition mod)) return contents;
            if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) return contents;

            List<IContentDefinition> l = parser.GetContentDefinitions();
            if (l == null) return contents;

            foreach (IContentDefinition d in l)
            {
                contents.Add((T)d);
            }
            return contents;
        }

        public IContentDefinition GetContentDefinition(ModObjectGUIDReference reference)
        {
            return GetContentDefinition<IContentDefinition>(reference);
        }
        
        public T GetContentDefinition<T>(ModObjectGUIDReference reference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(reference.modGUID, out LoadedModDefinition mod)) return null;
            if (!mod.definition.ContentParsers.TryGetValue(reference.contentType, out IContentParser parser)) return null;
            return (T)parser.GetContentDefinition(reference.contentGUID);
        }
        #endregion

        #region Unloading
        public void UnloadAllContent()
        {
            var copy = currentlyLoadedContent.Keys.ToList();
            foreach (var key in copy)
            {
                var tempList = currentlyLoadedContent[key].ToList();
                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    UnloadContentDefinition(new ModObjectGUIDReference(){ modGUID = key, contentType = tempList[i].Item1, contentGUID = tempList[i].Item2});
                }
            }
        }
        
        public void UnloadContentDefinition(ModObjectGUIDReference reference)
        {
            if (!modLoader.TryGetLoadedMod(reference.modGUID, out LoadedModDefinition mod)) return;
            if (!mod.definition.ContentParsers.TryGetValue(reference.contentType, out IContentParser parser)) return;
            parser.UnloadContentDefinition(reference.contentGUID);
            UntrackItem(reference);
        }
        #endregion

        private void TrackItem(ModObjectGUIDReference objectReference)
        {
            if(currentlyLoadedContent.ContainsKey(objectReference.modGUID) == false) currentlyLoadedContent.Add(objectReference.modGUID, new HashSet<(int, string)>());
            currentlyLoadedContent[objectReference.modGUID].Add((objectReference.contentType, objectReference.contentGUID));
        }

        private void UntrackItem(ModObjectGUIDReference objectReference)
        {
            if (currentlyLoadedContent.ContainsKey(objectReference.modGUID) == false) return;
            currentlyLoadedContent[objectReference.modGUID].Remove((objectReference.contentType, objectReference.contentGUID));
        }
    }
}