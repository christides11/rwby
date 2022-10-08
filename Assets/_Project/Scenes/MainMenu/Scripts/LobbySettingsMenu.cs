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

        public Dictionary<string, LobbySettingsFieldContent> idFieldDictionary =
            new Dictionary<string, LobbySettingsFieldContent>();

        public Dictionary<string, MonoBehaviour> idContentDictionary =
            new Dictionary<string, MonoBehaviour>();

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
            
            idFieldDictionary.Clear();
            idContentDictionary.Clear();
        }

        public void ClearOption(string id)
        {
            if (!idFieldDictionary.ContainsKey(id)) return;
            Destroy(idFieldDictionary[id]);
            Destroy(idContentDictionary[id]);
            idFieldDictionary.Remove(id);
            idContentDictionary.Remove(id);
        }

        public rwby.ui.Selectable AddOption(string id, string value, float height = 0)
        {
            return AddOption(id, "", value, height);
        }

        public void BringOptionToBottom(string id)
        {
            if (!idFieldDictionary.ContainsKey(id)) return;
            idFieldDictionary[id].transform.SetAsLastSibling();
            idContentDictionary[id].transform.SetAsLastSibling();
        }
        
        public rwby.ui.Selectable AddOption(string id, string fieldName, string value, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            LobbySettingsFieldContent fc = GameObject.Instantiate(fieldPrefab, contentFieldTransform, false);
            fc.LayoutElement.preferredHeight = height;
            fc.text.text = fieldName;
            LobbySettingsStringValueContent svc = GameObject.Instantiate(stringValuePrefab, contentValueTransform, false);
            svc.LayoutElement.preferredHeight = height;
            svc.text.text = value;
            
            idFieldDictionary.Add(id, fc);
            idContentDictionary.Add(id, svc);
            return svc.selectable;
        }

        public rwby.ui.Selectable[] AddOption(string id, string fieldName, int value, float height = 0)
        {
            if (height == 0) height = defaultHeight;
            LobbySettingsFieldContent fc = GameObject.Instantiate(fieldPrefab, contentFieldTransform, false);
            fc.LayoutElement.preferredHeight = height;
            fc.text.text = fieldName;
            LobbySettingsIntValueContent svc = GameObject.Instantiate(intValuePrefab, contentValueTransform, false);
            svc.LayoutElement.preferredHeight = height;
            svc.text.text = value.ToString();
            
            idFieldDictionary.Add(id, fc);
            idContentDictionary.Add(id, svc);
            return new Selectable[2]{ svc.selectableSubtract, svc.selectableAdd };
        }
    }
}