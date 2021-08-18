using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "Settings", menuName = "RWBY/Config/Settings")]
    public class Settings : ScriptableObject
    {
        public string bootLoaderSceneName = "Singletons";
        public string mainMenuSceneName = "MainMenu";
        public AddressablesModDefinition baseMod;
    }
}