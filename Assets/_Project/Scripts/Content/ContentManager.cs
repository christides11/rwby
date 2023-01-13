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

        public Dictionary<uint, HashSet<(int, int)>> currentlyLoadedContent =
            new Dictionary<uint, HashSet<(int, int)>>();

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

        public bool ContentExist<T>(ModIDContentReference contentReference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(contentReference.modID, out LoadedModDefinition mod)) return false;
            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)) return false;
            return parser.ContentExist(contentReference.contentIdx);
        }

        public async UniTask<(bool, IContentDefinition)> TryGetContentDefinition(ModIDContentReference contentReference, bool load = true, bool track = true)
        {
            (bool, IContentDefinition) returnVals = (false, null);
            if (load)
            {
                var loadResult = await LoadContentDefinition(contentReference, track);
                if (!loadResult) return returnVals;
            }
            returnVals.Item2  = GetContentDefinition(contentReference);
            if (returnVals.Item2 != null) returnVals.Item1 = true;
            return returnVals;
        }
        
        #region Loading
        public async UniTask LoadContentDefinitions(int contentType, bool track = true)
        {
            foreach (var m in modLoader.loadedModsByID.Keys)
            {
                await LoadContentDefinitions(m, contentType, track);
            }
        }

        public async UniTask<bool> LoadContentDefinitions(uint modID, int contentType, bool track = true)
        {
            if (!modLoader.TryGetLoadedMod(modID, out LoadedModDefinition mod)) return false;
            if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) return false;
            var result = await parser.LoadContentDefinitions(mod);
            if (track)
            {
                foreach (var r in result)
                {
                    TrackItem(new ModIDContentReference()
                        { modID = modID, contentType = contentType, contentIdx = r });
                }
            }

            return true;
        }

        public async UniTask<bool> LoadContentDefinition(ModIDContentReference contentReference, bool track = true)
        {
            if (!modLoader.TryGetLoadedMod(contentReference.modID, out LoadedModDefinition mod))
            {
                Debug.Log($"Failure getting loaded mod. {contentReference.ToString()}.");
                return false;
            }
            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)) return false;
            bool result = await parser.LoadContentDefinition(mod, contentReference.contentIdx);
            if(track && result) TrackItem(contentReference);
            return result;
        }
        #endregion

        #region Getting
        public List<ModIDContentReference> GetContentDefinitionReferences(int contentType)
        {
            List<ModIDContentReference> content = new List<ModIDContentReference>();
            foreach (var m in modLoader.loadedModsByID.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences(m, contentType));
            }
            return content;
        }

        public List<ModIDContentReference> GetContentDefinitionReferences(uint modID, int contentType)
        {
            List<ModIDContentReference> content = new List<ModIDContentReference>();

            if (!modLoader.TryGetLoadedMod(modID, out LoadedModDefinition mod)) return content;
            if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) return content;

            List<IContentDefinition> fds = parser.GetContentDefinitions();
            if (fds == null) return content;

            foreach (IContentDefinition fd in fds)
                content.Add(new ModIDContentReference(){ modID = modID, contentType = contentType, contentIdx = fd.Identifier });
            return content;
        }

        public List<T> GetContentDefinitions<T>(int contentType) where T : IContentDefinition
        {
            List<T> contents = new List<T>();

            foreach (var m in modLoader.loadedModsByID.Keys)
            {
                contents.InsertRange(contents.Count, GetContentDefinitions<T>(m, contentType));
            }
            return contents;
        }

        public List<T> GetContentDefinitions<T>(uint modGUID, int contentType) where T : IContentDefinition
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

        public IContentDefinition GetContentDefinition(ModIDContentReference contentReference)
        {
            return GetContentDefinition<IContentDefinition>(contentReference);
        }
        
        public T GetContentDefinition<T>(ModIDContentReference contentReference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(contentReference.modID, out LoadedModDefinition mod)) return null;
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
                    UnloadContentDefinition(new ModIDContentReference(){ modID = key, contentType = tempList[i].Item1, contentIdx = tempList[i].Item2});
                }
            }
        }
        
        public bool UnloadContentDefinition(ModIDContentReference contentReference, bool ignoreIfTracked = false)
        {
            
            if (!modLoader.TryGetLoadedMod(contentReference.modID, out LoadedModDefinition mod))
            {
                Debug.Log($"Get loaded mod Failure. {contentReference.modID.ToString()}.");
                return false;
            }

            if (!mod.definition.ContentParsers.TryGetValue(contentReference.contentType, out IContentParser parser)){
                Debug.Log($"Get content parser failure. {contentReference.ToString()}");
                return false;
            }
            if (ignoreIfTracked && IsItemTracked(contentReference)) return true;
            parser.UnloadContentDefinition(contentReference.contentIdx);
            UntrackItem(contentReference);
            return true;
        }
        #endregion
        
        #region Temporary Loading
        public async UniTask<List<ModIDContentReference>> GetPaginatedContent(int contentType, int amtPerPage, int page, HashSet<string> requiredTags)
        {
            var contentReferences = new List<ModIDContentReference>();
            int startAmt = page * amtPerPage;
            int currAmt = 0;
            
            foreach (var m in modLoader.loadedModsByID.Keys)
            {
                if (!modLoader.TryGetLoadedMod(m, out LoadedModDefinition mod)) continue;
                if (mod.definition == null) continue;
                if (!mod.definition.ContentParsers.TryGetValue(contentType, out IContentParser parser)) continue;
                foreach(var v in parser.GUIDToInt)
                {
                    if (currAmt != startAmt)
                    {
                        currAmt++;
                        continue;
                    }

                    var contentReference = new ModIDContentReference(m, contentType, v.Value);
                    var cLoadResult = await LoadContentDefinition(contentReference, false);
                    if (!cLoadResult) continue;
                    var contentDefinition = GetContentDefinition(contentReference);
                    if (!requiredTags.IsSupersetOf(contentDefinition.Tags)) continue;
                    contentReferences.Add(contentReference);
                    if (contentReferences.Count == amtPerPage) return contentReferences;
                }
            }

            if (currAmt != startAmt) return null;
            return contentReferences;
        }
        #endregion
        
        private void TrackItem(ModIDContentReference contentReference)
        {
            if(currentlyLoadedContent.ContainsKey(contentReference.modID) == false) currentlyLoadedContent.Add(contentReference.modID, new HashSet<(int, int)>());
            currentlyLoadedContent[contentReference.modID].Add((contentReference.contentType, contentReference.contentIdx));
        }

        private void UntrackItem(ModIDContentReference contentReference)
        {
            if (currentlyLoadedContent.ContainsKey(contentReference.modID) == false) return;
            currentlyLoadedContent[contentReference.modID].Remove((contentReference.contentType, contentReference.contentIdx));
        }

        private bool IsItemTracked(ModIDContentReference contentReference)
        {
            if (!currentlyLoadedContent.ContainsKey(contentReference.modID)) return false;
            return currentlyLoadedContent[contentReference.modID]
                .Contains((contentReference.contentType, contentReference.contentIdx));
        }

        public ModIDContentReference ConvertStringToGUIDReference(ModContentStringReference contentReference)
        {
            ModIDContentReference temp = new ModIDContentReference();

            if (!modLoader.modNamespaceToID.ContainsKey(contentReference.modGUID)) return temp;
            temp.modID = modLoader.modNamespaceToID[contentReference.modGUID];
            if (!modLoader.TryGetLoadedMod(contentReference.modGUID, out LoadedModDefinition mod)) return temp;
            temp.contentType = (int)contentReference.contentType;
            if (!mod.definition.ContentParsers.TryGetValue((int)contentReference.contentType, out IContentParser parser)) return temp;
            temp.contentIdx = parser.GUIDToInt[contentReference.contentGUID];
            return temp;
        }
    }
}