using System.Collections;
using System.Collections.Generic;
using rwby;
using UnityEngine;

public interface IContentLoad
{
    IEnumerable<ModObjectReference> loadedContent { get; }
}
