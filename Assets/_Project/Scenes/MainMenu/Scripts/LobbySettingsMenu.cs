using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using UnityEngine;

namespace rwby
{
    public class LobbySettingsMenu : MonoBehaviour
    {
        public Transform contentFieldTransform;
        public Transform contentValueTransform;
        public LobbySettingsFieldContent fieldPrefab;
        public LobbySettingsStringValueContent stringValuePrefab;
        public LobbySettingsIntValueContent intValuePrefab;

        public float defaultHeight = 90;
        
        public void Open()
        {
            ClearOptions();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            ClearOptions();
        }

        public void ClearOptions()
        {
            foreach (Transform child in contentFieldTransform)
            {
                Destroy(child.gameObject);
            }
            
            foreach (Transform child in contentValueTransform)
            {
                Destroy(child.gameObject);
            }
        }

        public rwby.ui.Selectable AddOption(string value, float height = 0)
        {
            return AddOption("", value, height);
        }
        
        public rwby.ui.Selectable AddOption(string fieldName, string value, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            LobbySettingsFieldContent fc = GameObject.Instantiate(fieldPrefab, contentFieldTransform, false);
            fc.LayoutElement.preferredHeight = height;
            fc.text.text = fieldName;
            LobbySettingsStringValueContent svc = GameObject.Instantiate(stringValuePrefab, contentValueTransform, false);
            svc.LayoutElement.preferredHeight = height;
            svc.text.text = value;
            return svc.selectable;
        }

        public rwby.ui.Selectable[] AddOption(string fieldName, int value, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            LobbySettingsFieldContent fc = GameObject.Instantiate(fieldPrefab, contentFieldTransform, false);
            fc.LayoutElement.preferredHeight = height;
            fc.text.text = fieldName;
            LobbySettingsIntValueContent svc = GameObject.Instantiate(intValuePrefab, contentValueTransform, false);
            svc.LayoutElement.preferredHeight = height;
            svc.text.text = value.ToString();
            return new Selectable[2]{ svc.selectableSubtract, svc.selectableAdd };
        }
    }
}