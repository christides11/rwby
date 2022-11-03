namespace rwby
{
    [System.Serializable]
    public class SettingsDataType
    {
        public int screenResX = 0;
        public int screenResY = 0;
        public int screenMode = 0; // WINDOWED, FULLSCREEN WINDOWED, EXCLUSIVE FULLSCREEN
        public int vsync = 0; // OFF, ON, TRIPLE BUFFER
        public int reduceInputLatency = 0; // OFF, ON
        public int frameRateCap = 0;
        public int antiAliasing = 0; // OFF, FXAA, SMAA, MSAA
        
        public int textureQuality = 3; // LOWEST, LOW, MEDIUM, HIGH
        
        public int shadowQuality = 3; // OFF, LOW, MEDIUM, HIGH
        public int ambientOcclusion = 3; // OFF, LOW, MEDIUM, HIGH
        
        public int depthOfField = 1;
        public int bloom = 1;
        public int lensFlares = 1;
        public int vignette = 1;
        public int motionBlurStrength = 1;
        public int fieldOfView = 1;
    }
}