using Cysharp.Threading.Tasks;

namespace rwby
{
    public abstract class ISongDefinition : IContentDefinition
    {
        public override string Name { get; }
        public override string Description { get; }
        public abstract SongAudio Song { get; }


        public override UniTask<bool> Load()
        {
            throw new System.NotImplementedException();
        }
        
        public override bool Unload()
        {
            throw new System.NotImplementedException();
        }
    }
}