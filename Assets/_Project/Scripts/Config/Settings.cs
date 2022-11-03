using UnityEngine;
using Cinemachine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

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
        public AudioMixer audioMixer;

        [Header("Camera")] 
        public DummyCamera dummyCamera;
        public CameraSwitcher cameraSwitcher;
        public LockonCameraManager lockonCameraManager;

        [Header("Render Pipeline Settings")] 
        public UniversalRendererData rendererData;
        public UniversalRenderPipelineAsset pipelineAsset;
    }
}