using System.Collections.Generic;

namespace rwby
{
    public abstract class ISoundbankDefinition : IContentDefinition
    {
        public virtual List<SoundbankSoundEntry> Sounds { get; }
        public virtual Dictionary<string, int> SoundMap { get; }
    }
}