using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;

namespace rwby
{
    public class LoadedModDefinition
    {
        public ModHost host;
        public IModDefinition definition;

        public LoadedModDefinition()
        {

        }

        public LoadedModDefinition(ModHost host, IModDefinition definition)
        {
            this.host = host;
            this.definition = definition;
        }
    }
}