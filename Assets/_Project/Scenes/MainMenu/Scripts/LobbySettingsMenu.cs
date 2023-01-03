using System.Collections;
using System.Collections.Generic;
using rwby.ui;
using UnityEngine;

namespace rwby
{
    public class LobbySettingsMenu : MonoBehaviour
    {
        public Transform contentTransform;
        public ContentButtonBase basePrefab;
        public ContentButtonIntValue intValuePrefab;
        public ContentButtonStringValue stringValuePrefab;
        public ContentButtonInputField inputFieldPrefab;
        public OptionSlider optionSliderPrefab;

        public float defaultHeight = 90;

        public Dictionary<string, ContentButtonBase> idContentDictionary =
            new Dictionary<string, ContentButtonBase>();

        public void Open()
        {
            ClearOptions();
        }

        public void Close()
        {
            ClearOptions();
        }

        public void ClearOptions()
        {
            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject);
            }

            idContentDictionary.Clear();
        }

        public void ClearOption(string id)
        {
            if (!idContentDictionary.ContainsKey(id)) return;
            Destroy(idContentDictionary[id].gameObject);
            idContentDictionary.Remove(id);
        }

        public ContentButtonStringValue AddOption(string id, string value, float height = 0)
        {
            return AddStringValueOption(id, "", value, height);
        }

        public void BringOptionToBottom(string id)
        {
            if (!idContentDictionary.ContainsKey(id)) return;
            idContentDictionary[id].transform.SetAsLastSibling();
        }

        public OptionSlider AddOptionSlider(string id, string fieldName, string[] values, int defaultOption, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            var svc = GameObject.Instantiate(optionSliderPrefab, contentTransform, false);
            svc.label.text = fieldName;
            svc.options = values;
            svc.SetOption(defaultOption);
            idContentDictionary.Add(id, svc);
            return svc;
        }
        
        public ContentButtonStringValue AddStringValueOption(string id, string fieldName, string value, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            var svc = GameObject.Instantiate(stringValuePrefab, contentTransform, false);
            //svc.LayoutElement.preferredHeight = height;
            svc.label.text = fieldName;
            svc.valueString.text = value;
            idContentDictionary.Add(id, svc);
            
            return svc;
        }
        
        public ContentButtonInputField AddInputField(string id, string fieldName, string defaultText, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            var svc = GameObject.Instantiate(inputFieldPrefab, contentTransform, false);
            //svc.LayoutElement.preferredHeight = height;
            svc.label.text = fieldName;
            svc.inputField.text = defaultText;
            idContentDictionary.Add(id, svc);
            
            return svc;
        }

        public ContentButtonIntValue AddIntValueOption(string id, string fieldName, int value, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            var svc = GameObject.Instantiate(intValuePrefab, contentTransform, false);
            //svc.LayoutElement.preferredHeight = height;
            svc.label.text = fieldName;
            svc.intValueText.text = value.ToString();
            idContentDictionary.Add(id, svc);

            return svc;
            //return new Selectable[2]{ svc.selectableSubtract, svc.selectableAdd };
        }
    }
}