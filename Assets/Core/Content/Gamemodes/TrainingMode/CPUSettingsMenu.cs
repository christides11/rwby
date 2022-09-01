using UnityEngine;

namespace rwby.core.training
{
    public class CPUSettingsMenu : MonoBehaviour, IClosableMenu
    {
        public GamemodeTraining gamemodeTraining;

        public Transform cpusParent;
        public GameObject cpuEntryItem;

        public void Open()
        {
            gameObject.SetActive(true);
            SetupCPUs(gamemodeTraining.cpuHandler);
            gamemodeTraining.cpuHandler.OnCPUListUpdated += SetupCPUs;
        }

        public bool TryClose()
        {
            gamemodeTraining.cpuHandler.OnCPUListUpdated -= SetupCPUs;
            gameObject.SetActive(false);
            return true;
        }
        private void SetupCPUs(TrainingCPUHandler cpuHandler)
        {
            foreach(Transform child in cpusParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < 4; i++)
            {
                int tempi = i;
                TrainingUIcpuItem cpuObj = GameObject.Instantiate(cpuEntryItem, cpusParent, false).GetComponent<TrainingUIcpuItem>();
                cpuObj.Init(cpuHandler, i);
                cpuObj.removeButton.OnPointerClickEvent.AddListener((a) => { RemoveCPU(tempi); });
                cpuObj.characterButton.OnPointerClickEvent.AddListener((a) => { OpenFighterPicker(tempi); });
            }
        }

        private void RemoveCPU(int index)
        {
            
        }
        
        private void OpenFighterPicker(int index)
        {
            int tempIndex = index;
            _ = ContentSelect.singleton.OpenMenu(0, (int)ContentType.Fighter, (a, b) =>
            {
                var list = gamemodeTraining.cpuHandler.cpus;
                var temp = list[tempIndex];
                temp.characterReference = b;
                list[tempIndex] = temp;
                ContentSelect.singleton.CloseMenu(0);
            });
        }
    }
}