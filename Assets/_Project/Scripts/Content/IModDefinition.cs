using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace rwby
{
    public interface IModDefinition
    {
        string Description { get; }

        // CONTENT //
        bool ContentExist(ContentType contentType, string contentIdentfier);
        UniTask<bool> LoadContentDefinitions(ContentType contentType);
        UniTask<bool> LoadContentDefinition(ContentType contentType, string contentIdentifier);
        List<IContentDefinition> GetContentDefinitions(ContentType contentType);
        IContentDefinition GetContentDefinition(ContentType contentType, string contentIdentifier);
        void UnloadContentDefinitions(ContentType contentType);
        void UnloadContentDefinition(ContentType contentType, string contentIdentifier);
    }
}