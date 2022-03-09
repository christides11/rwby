using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace rwby
{
    public class ContentManager : MonoBehaviour
    {
        public static ContentManager singleton;

        public Dictionary<Type, HashSet<ModObjectReference>> currentlyLoadedContent =
            new Dictionary<Type, HashSet<ModObjectReference>>();
        
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

        public bool ContentExist<T>(ModObjectReference objectReference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(objectReference.modIdentifier, out LoadedModDefinition mod)) return false;
            if (mod.definition == null) return false;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return false;
            return parser.ContentExist(objectReference.objectIdentifier);
        }

        #region Loading
        public async UniTask LoadContentDefinitions<T>() where T : IContentDefinition
        {
            foreach (var m in modLoader.loadedMods.Keys)
            {
                await LoadContentDefinitions<T>(m);
            }
        }

        public async UniTask<bool> LoadContentDefinitions<T>(ModIdentifierTuple modIdentifier) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(modIdentifier, out LoadedModDefinition mod)) return false;
            if (mod.definition == null) return true;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return false;
            var result = await parser.LoadContentDefinitions();
            foreach (var r in result) TrackItem(typeof(T), new ModObjectReference(modIdentifier, r));
            return true;
        }

        public async UniTask<bool> LoadContentDefinition<T>(ModObjectReference objectReference) where T : IContentDefinition
        {
            return await LoadContentDefinition(typeof(T), objectReference);
        }

        public async UniTask<bool> LoadContentDefinition(Type t, ModObjectReference objectReference)
        {
            if (!modLoader.TryGetLoadedMod(objectReference.modIdentifier, out LoadedModDefinition mod)) return false;
            if (mod.definition == null) return false;
            if (!mod.definition.ContentParsers.TryGetValue(t, out IContentParser parser)) return false;
            bool result = await parser.LoadContentDefinition(objectReference.objectIdentifier);
            if(result) TrackItem(t, objectReference);
            return result;
        }
        #endregion

        #region Getting
        public List<ModObjectReference> GetContentDefinitionReferences<T>() where T : IContentDefinition
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            foreach (var m in modLoader.loadedMods.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences<T>(m));
            }
            return content;
        }

        public List<ModObjectReference> GetContentDefinitionReferences<T>(ModIdentifierTuple modIdentifier) where T : IContentDefinition
        {
            List<ModObjectReference> content = new List<ModObjectReference>();

            if (!modLoader.TryGetLoadedMod(modIdentifier, out LoadedModDefinition mod)) return content;
            if (mod.definition == null) return content;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return content;

            List<IContentDefinition> fds = parser.GetContentDefinitions();
            if (fds == null) return content;

            foreach (IContentDefinition fd in fds)
            {
                content.Add(new ModObjectReference(modIdentifier, fd.Identifier));
            }
            return content;
        }

        public List<T> GetContentDefinitions<T>() where T : IContentDefinition
        {
            List<T> contents = new List<T>();

            foreach (var m in modLoader.loadedMods.Keys)
            {
                contents.InsertRange(contents.Count, GetContentDefinitions<T>(m));
            }
            return contents;
        }

        public List<T> GetContentDefinitions<T>(ModIdentifierTuple modIdentifier) where T : IContentDefinition
        {
            List<T> contents = new List<T>();

            if (!modLoader.TryGetLoadedMod(modIdentifier, out LoadedModDefinition mod)) return contents;
            if (mod.definition == null) return contents;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return contents;

            List<IContentDefinition> l = parser.GetContentDefinitions();
            if (l == null) return contents;

            foreach (IContentDefinition d in l)
            {
                contents.Add((T)d);
            }
            return contents;
        }

        public T GetContentDefinition<T>(ModObjectReference reference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(reference.modIdentifier, out LoadedModDefinition mod)) return null;
            if (mod.definition == null) return null;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return null;
            return (T)parser.GetContentDefinition(reference.objectIdentifier);
        }

        public IContentDefinition GetContentDefinition(Type t, ModObjectReference reference)
        {
            if (!modLoader.TryGetLoadedMod(reference.modIdentifier, out LoadedModDefinition mod)) return null;
            if (mod.definition == null) return null;
            if (!mod.definition.ContentParsers.TryGetValue(t, out IContentParser parser)) return null;
            return parser.GetContentDefinition(reference.objectIdentifier);
        }
        #endregion

        #region Unloading
        public void UnloadAllContent<T>() where T : IContentDefinition
        {
            foreach (var a in currentlyLoadedContent)
            {
                foreach (var b in a.Value)
                {
                    UnloadContentDefinition(a.Key, b);
                }
            }
        }

        public void UnloadContentDefinition<T>(ModObjectReference reference) where T : IContentDefinition
        {
            UnloadContentDefinition(typeof(T), reference);
        }

        public void UnloadContentDefinition(Type t, ModObjectReference reference)
        {
            if (!modLoader.TryGetLoadedMod(reference.modIdentifier, out LoadedModDefinition mod)) return;
            //if (mod.definition == null) return;
            if (!mod.definition.ContentParsers.TryGetValue(t, out IContentParser parser)) return;
            parser.UnloadContentDefinition(reference.objectIdentifier);
            UntrackItem(t, reference);
        }
        #endregion

        private void TrackItem(Type t, ModObjectReference objectReference)
        {
            if(currentlyLoadedContent.ContainsKey(t) == false) currentlyLoadedContent.Add(t, new HashSet<ModObjectReference>());
            currentlyLoadedContent[t].Add(objectReference);
        }

        private void UntrackItem(Type t, ModObjectReference objectReference)
        {
            if (currentlyLoadedContent.ContainsKey(t) == false) return;
            currentlyLoadedContent[t].Remove(objectReference);
        }
    }
}