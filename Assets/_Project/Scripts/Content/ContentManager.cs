using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace rwby
{
    public class ContentManager : MonoBehaviour
    {
        public static ContentManager singleton;

        [SerializeField] private ModLoader modLoader;

        public void Initialize()
        {
            singleton = this;
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
            foreach (string m in modLoader.loadedMods.Keys)
            {
                await LoadContentDefinitions<T>(m);
            }
        }

        public async UniTask<bool> LoadContentDefinitions<T>(string modIdentifier) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(modIdentifier, out LoadedModDefinition mod)) return false;
            if (mod.definition == null) return true;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return false;
            return await parser.LoadContentDefinitions();
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
            return await parser.LoadContentDefinition(objectReference.objectIdentifier);
        }
        #endregion

        #region Getting
        public List<ModObjectReference> GetContentDefinitionReferences<T>() where T : IContentDefinition
        {
            List<ModObjectReference> content = new List<ModObjectReference>();
            foreach (string m in modLoader.loadedMods.Keys)
            {
                content.InsertRange(content.Count, GetContentDefinitionReferences<T>(m));
            }
            return content;
        }

        public List<ModObjectReference> GetContentDefinitionReferences<T>(string modIdentifier) where T : IContentDefinition
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

            foreach (string m in modLoader.loadedMods.Keys)
            {
                contents.InsertRange(contents.Count, GetContentDefinitions<T>(m));
            }
            return contents;
        }

        public List<T> GetContentDefinitions<T>(string modIdentifier) where T : IContentDefinition
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
        public void UnloadContentDefinitions<T>() where T : IContentDefinition
        {
            foreach (string m in modLoader.loadedMods.Keys)
            {
                UnloadContentDefinitions<T>(m);
            }
        }

        public void UnloadContentDefinitions<T>(string modIdentifier) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(modIdentifier, out LoadedModDefinition mod)) return;
            if (mod.definition == null) return;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return;
            parser.UnloadContentDefinitions();
        }

        public void UnloadContentDefinition<T>(ModObjectReference reference) where T : IContentDefinition
        {
            if (!modLoader.TryGetLoadedMod(reference.modIdentifier, out LoadedModDefinition mod)) return;
            if (mod.definition == null) return;
            if (!mod.definition.ContentParsers.TryGetValue(typeof(T), out IContentParser parser)) return;
            parser.UnloadContentDefinition(reference.objectIdentifier);
        }
        #endregion
    }
}