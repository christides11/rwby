using Rewired.Integration.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace rwby.core.training
{
    public class CPUSettingsMenu : MonoBehaviour, IClosableMenu
    {
        public GamemodeTraining gamemodeTraining;

        public Transform cpusParent;
        public GameObject cpuEntryItem;
        public GameObject cpuAddItem;

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

            for(int i = 0; i < gamemodeTraining.cpuHandler.cpus.Count; i++)
            {
                int tempi = i;
                GameObject cpuEntryObject = GameObject.Instantiate(cpuEntryItem, cpusParent, false);
                PlayerPointerEventTrigger[] eventTriggers = cpuEntryObject.GetComponentsInChildren<PlayerPointerEventTrigger>();
                eventTriggers[0].OnPointerClickEvent.AddListener((a) => { RemoveCPU(tempi); });
                eventTriggers[1].OnPointerClickEvent.AddListener((a) => { SetCPUReference(tempi); });
                TextMeshProUGUI[] textMeshes = cpuEntryObject.GetComponentsInChildren<TextMeshProUGUI>();
                textMeshes[1].text = gamemodeTraining.cpuHandler.cpus[i].characterReference.IsValid()
                    ? gamemodeTraining.cpuHandler.cpus[i].characterReference.ToString()
                    : "";
            }

            GameObject cpuAddObject = GameObject.Instantiate(cpuAddItem, cpusParent, false);
            cpuAddObject.GetComponentInChildren<PlayerPointerEventTrigger>().OnPointerClickEvent.AddListener(AddCPU);
        }

        private void AddCPU(PlayerPointerEventData pointerEventData)
        {
            gamemodeTraining.cpuHandler.cpus.Add(new TrainingCPUReference());
        }

        private void RemoveCPU(int index)
        {

        }

        private void SetCPUReference(int index)
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