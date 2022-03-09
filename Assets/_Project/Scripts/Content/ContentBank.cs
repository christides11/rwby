using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class ContentBank
    {
        public Dictionary<ModObjectReference, IContentDefinition> content = new Dictionary<ModObjectReference, IContentDefinition>();
    }
}