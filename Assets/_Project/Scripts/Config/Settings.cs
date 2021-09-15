using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "Settings", menuName = "RWBY/Config/Settings")]
    public class Settings : ScriptableObject
    {
        public string bootLoaderSceneName = "Singletons";
        public string mainMenuSceneName = "MainMenu";
        public AddressablesModDefinition baseMod;
        public PlayerCamera playerCameraPrefab;
        public GameObject followVirtualCameraPrefab;
        public Hurtbox hurtboxPrefab;
        public bool showHitboxes;
        public BaseHUD baseUI;
    }
}