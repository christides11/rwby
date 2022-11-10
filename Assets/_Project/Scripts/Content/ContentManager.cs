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

        public Dictionary<ContentGUID, HashSet<(int, int)>> currentlyLoadedContent =
            new Dictionary<ContentGUID, HashSet<(int, int)>>();

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

        public bool ContentExist<T>(ModGUIDContentReference contentReference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(contentReference.modGUID, out LoadedModDefinition mod)) return false;
            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)) return false;
            return parser.ContentExist(contentReference.contentIdx);
        }

        #region Loading
        public async UniTask LoadContentDefinitions(int contentType, bool track = true)
        {
            foreach (var m in modLoader.loadedModsByGUID.Keys)
            {
                await LoadContentDefinitions(m, contentType, track);
            }
        }

        public async UniTask<bool> LoadContentDefinitions(ContentGUID modGUID, int contentType, bool track = true)
        {
            if (!modLoader.TryGetLoadedMod(modGUID, out LoadedModDefinition mod)) return false;
            if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) return false;
            var result = await parser.LoadContentDefinitions(mod);
            if (track)
            {
                foreach (var r in result)
                {
                    TrackItem(new ModGUIDContentReference()
                        { modGUID = modGUID, contentType = contentType, contentIdx = r });
                }
            }

            return true;
        }

        public async UniTask<bool> LoadContentDefinition(ModGUIDContentReference contentReference, bool track = true)
        {
            if (!modLoader.TryGetLoadedMod(contentReference.modGUID, out LoadedModDefinition mod))
            {
                Debug.Log($"Load Failure. {contentReference.modGUID.ToString()}.");
                return false;
            }
            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)) return false;
            bool result = await parser.LoadContentDefinition(mod, contentReference.contentIdx);
            if(track && result) TrackItem(contentReference);
            return result;
        }
        #endregion

        #region Getting
        public List<ModGUIDContentReference> GetContentDefinitionReferences(int contentType)
        {
            List<ModGUIDContentReference> content = new List<ModGUIDContentReference>();
            foreach (var m in modLoader.loadedModsByGUID.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences(m, contentType));
            }
            return content;
        }

        public List<ModGUIDContentReference> GetContentDefinitionReferences(ContentGUID modGUID, int contentType)
        {
            List<ModGUIDContentReference> content = new List<ModGUIDContentReference>();

            if (!modLoader.TryGetLoadedMod(modGUID, out LoadedModDefinition mod)) return content;
            if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) return content;

            List<IContentDefinition> fds = parser.GetContentDefinitions();
            if (fds == null) return content;

            foreach (IContentDefinition fd in fds)
                content.Add(new ModGUIDContentReference(){ modGUID = modGUID, contentType = contentType, contentIdx = fd.Identifier });
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

        public List<T> GetContentDefinitions<T>(ContentGUID modGUID, int contentType) where T : IContentDefinition
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

        public IContentDefinition GetContentDefinition(ModGUIDContentReference contentReference)
        {
            return GetContentDefinition<IContentDefinition>(contentReference);
        }
        
        public T GetContentDefinition<T>(ModGUIDContentReference contentReference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(contentReference.modGUID, out LoadedModDefinition mod)) return null;
            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)) return null;
            return (T)parser.GetContentDefinition(contentReference.contentIdx);
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
                    UnloadContentDefinition(new ModGUIDContentReference(){ modGUID = key, contentType = tempList[i].Item1, contentIdx = tempList[i].Item2});
                }
            }
        }
        
        public bool UnloadContentDefinition(ModGUIDContentReference contentReference)
        {
            if (!modLoader.TryGetLoadedMod(contentReference.modGUID, out LoadedModDefinition mod))
            {
                Debug.Log($"Get loaded mod Failure. {contentReference.modGUID.ToString()}.");
                return false;
            }

            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)){
                Debug.Log($"Get content parser failure. {contentReference.ToString()}");
            }
            parser.UnloadContentDefinition(contentReference.contentIdx);
            UntrackItem(contentReference);
            return true;
        }
        #endregion
        
        #region Temporary Loading
        public List<ModGUIDContentReference> GetPaginatedContent(int contentType, int amtPerPage, int page)
        {
            var contentReferences = new List<ModGUIDContentReference>();
            int startAmt = page * amtPerPage;
            int currAmt = 0;
            
            foreach (var m in modLoader.loadedModsByGUID.Keys)
            {
                if (!modLoader.TryGetLoadedMod(m, out LoadedModDefinition mod)) continue;
                if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) continue;
                foreach(var v in parser.GUIDToInt)
                {
                    if (currAmt != startAmt)
                    {
                        currAmt++;
                        continue;
                    }
                    contentReferences.Add(new ModGUIDContentReference(m, contentType, v.Value));
                    if (contentReferences.Count == amtPerPage) return contentReferences;
                }
            }

            if (currAmt != startAmt) return null;
            return contentReferences;
        }
        #endregion
        
        private void TrackItem(ModGUIDContentReference contentReference)
        {
            if(currentlyLoadedContent.ContainsKey(contentReference.modGUID) == false) currentlyLoadedContent.Add(contentReference.modGUID, new HashSet<(int, int)>());
            currentlyLoadedContent[contentReference.modGUID].Add((contentReference.contentType, contentReference.contentIdx));
        }

        private void UntrackItem(ModGUIDContentReference contentReference)
        {
            if (currentlyLoadedContent.ContainsKey(contentReference.modGUID) == false) return;
            currentlyLoadedContent[contentReference.modGUID].Remove((contentReference.contentType, contentReference.contentIdx));
        }

        public ModGUIDContentReference ConvertModContentGUIDReference(ModContentGUIDReference contentReference)
        {
            ModGUIDContentReference temp = new ModGUIDContentReference(contentReference.modGUID, contentReference.contentType, 0);

            if (!modLoader.TryGetLoadedMod(contentReference.modGUID, out LoadedModDefinition mod)) return temp;
            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)) return temp;
            temp.contentIdx = parser.GUIDToInt[contentReference.contentGUID];
            return temp;
        }
    }
}