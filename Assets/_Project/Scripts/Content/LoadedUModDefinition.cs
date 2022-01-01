using UMod;

namespace rwby
{
    public class LoadedUModModDefinition : LoadedModDefinition
    {
        public ModHost host;

        public override void Unload()
        {
            host.UnloadMod();
        }
    }
}