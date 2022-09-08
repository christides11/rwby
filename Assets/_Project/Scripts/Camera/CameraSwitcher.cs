using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class CameraSwitcher : MonoBehaviour
    {
        public Camera cam;
        public List<BasePlayerCamera> playerCameras = new List<BasePlayerCamera>();
        public int currentCamera;

        public void RegisterCamera(BasePlayerCamera cam)
        {
            playerCameras.Add(cam);
        }

        public void SwitchTo(int camera)
        {
            playerCameras[currentCamera].Deactivate();
            currentCamera = camera;
            playerCameras[currentCamera].Activate();
        }
    }
}