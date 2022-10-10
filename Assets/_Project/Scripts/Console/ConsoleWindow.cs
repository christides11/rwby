using System;
using System.Collections.Generic;
using Rewired.Integration.UnityUI;
using UnityEngine;
using TMPro;

namespace rwby.Debugging
{
    public enum ConsoleMessageType
    {
        Debug = 0,
        Error = 1,
        Warning = 2,
        Print = 3
    }
    public class ConsoleWindow : MonoBehaviour
    {
        public static ConsoleWindow current;

        public GameObject canvasGO;

        /*
        [Header("MP Info")]
        public TextMeshProUGUI rttText;
        public TextMeshProUGUI frameText;
        public TextMeshProUGUI leadServerText;
        public TextMeshProUGUI leadLocalText;
        public TextMeshProUGUI adjText;*/

        [Header("Console")]
        [SerializeField] private ConsoleReader consoleReader;
        [SerializeField] private TextMeshProUGUI consoleText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private List<Color> messageColors = new List<Color>(4);

        public void Awake()
        {
            current = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                canvasGO.SetActive(!canvasGO.activeSelf);
            }

            if (canvasGO.activeInHierarchy == false)
            {
                return;
            }

            if (RewiredEventSystem.current.currentSelectedGameObject == inputField.gameObject
                && UnityEngine.Input.GetKeyDown(KeyCode.Return) && !String.IsNullOrEmpty(inputField.text))
            {
                string input = inputField.text;
                inputField.text = "";
                WriteLine($"> {input}", ConsoleMessageType.Print);
                _ = consoleReader.Convert(input);
            }
        }

        public void Write(string text)
        {
            consoleText.text += text;
        }

        public void WriteLine(string text, ConsoleMessageType msgType = ConsoleMessageType.Debug)
        {
            Write($"<#{ColorUtility.ToHtmlStringRGBA(messageColors[(int)msgType])}>" + text + "</color>");
            Write("\n");
            switch (msgType)
            {
                case ConsoleMessageType.Debug:
                case ConsoleMessageType.Print:
                    Debug.Log(text);
                    break;
                case ConsoleMessageType.Error:
                    Debug.LogError(text);
                    break;
                case ConsoleMessageType.Warning:
                    Debug.LogWarning(text);
                    break;
                default:
                    Debug.Log(text);
                    break;
            }
        }

    }
}