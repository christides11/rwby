using UnityEngine;
using Cinemachine;

namespace rwby
{
    [CreateAssetMenu(fileName = "Settings", menuName = "rwby/Config/Settings")]
    public class Settings : ScriptableObject
    {
        public string bootLoaderSceneName = "Singletons";
        public string mainMenuSceneName = "MainMenu";
        public AddressablesModDefinition baseMod;
        public Hurtbox hurtboxPrefab;
        public bool showHitboxes;
        public BaseHUD baseUI;

        [Header("Camera")] 
        public DummyCamera dummyCamera;
        public CameraSwitcher cameraSwitcher;
        public LockonCameraManager lockonCameraManager;
    }
}