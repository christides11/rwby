using Rewired.Integration.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
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
            SetupCPUs();
            gamemodeTraining.OnCPUListUpdated += SetupCPUs;
        }

        public bool TryClose()
        {
            gamemodeTraining.OnCPUListUpdated -= SetupCPUs;
            gameObject.SetActive(false);
            return true;
        }

        private void SetupCPUs()
        {
            //TODO
            /*
            foreach(Transform child in cpusParent)
            {
                Destroy(child.gameObject);
            }

            for(int i = 0; i < gamemodeTraining.cpus.Count; i++)
            {
                int tempi = i;
                GameObject cpuEntryObject = GameObject.Instantiate(cpuEntryItem, cpusParent, false);
                PlayerPointerEventTrigger[] eventTriggers = cpuEntryObject.GetComponentsInChildren<PlayerPointerEventTrigger>();
                eventTriggers[0].OnPointerClickEvent.AddListener((a) => { RemoveCPU(tempi); });
                eventTriggers[1].OnPointerClickEvent.AddListener((a) => { SetCPUReference(tempi); });
            }

            GameObject cpuAddObject = GameObject.Instantiate(cpuAddItem, cpusParent, false);
            cpuAddObject.GetComponentInChildren<PlayerPointerEventTrigger>().OnPointerClickEvent.AddListener(AddCPU);*/
        }

        private void AddCPU(PlayerPointerEventData pointerEventData)
        {
            gamemodeTraining.cpus.Add(new CPUReference());
        }

        private void RemoveCPU(int index)
        {

        }

        private void SetCPUReference(int index)
        {
            /*
            int tempIndex = index;
            _ = ContentSelect.singleton.OpenMenu<IFighterDefinition>((a, b) =>
            {
                var list = gamemodeTraining.cpus;
                var temp = list[tempIndex];
                temp.characterReference = b;
                list[tempIndex] = temp;
                ContentSelect.singleton.CloseMenu();
            });*/
        }
    }
}