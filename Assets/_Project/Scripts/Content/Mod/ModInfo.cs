using System;

namespace rwby
{
    /// <summary>
    /// Info that is stored for any mods found.
    /// </summary>
    [System.Serializable]
    public class  ModInfo
    {
        public ModBackingType backingType;
        public Uri path;
        public string modName;
        public string fileName;
        public string identifier;
        public bool commandLine;
        public bool disableRequiresRestart;
        public bool enableRequiresRestart;
    }
}