using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rwby
{
    public class ButtonPrompter : MonoBehaviour
    {
        public ControllerGlyphs controllerGlyphs;
        public GameObject controllerGlyph;
        public GameObject keyboardGlyph;
        public GameObject keyboardLongGlyph;
        public TextMeshProUGUI textPrefab;

        public void Clear()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void AddPrompt(string action, string text)
        {
            // Get the Rewired Player
            Player p = ReInput.players.GetPlayer(0); // just use Player 0 in this example

            // Get the last active controller the Player was using
            Controller activeController = p.controllers.GetLastActiveController();
            if(activeController == null) { // player hasn't used any controllers yet
                // No active controller, set a default
                if(p.controllers.joystickCount > 0) { // try to get the first joystick in player
                    activeController = p.controllers.Joysticks[0];
                } else { // no joysticks assigned, just get keyboard
                    activeController = p.controllers.Keyboard;
                }
            }

            if (string.IsNullOrEmpty(action)) return;
            InputAction ac = ReInput.mapping.GetAction(action);
            if (ac == null) return;
            
            AddControllerGlyph(p, activeController, ac);
            var t = GameObject.Instantiate(textPrefab, transform, false);
            t.text = text;
        }

        void AddControllerGlyph(Player p, Controller controller, InputAction action)
        {
            if(p == null || controller == null || action == null) return;
            
            // Find the first element mapped to this Action on this controller
            ActionElementMap aem = p.controllers.maps.GetFirstElementMapWithAction(controller, action.id, true);
            if(aem == null) return; // nothing was mapped on this controller for this Action

            switch (controller.type)
            {
                case ControllerType.Joystick:
                    Sprite glyph = null;
                    glyph = controllerGlyphs.GetGlyph((controller as Joystick).hardwareTypeGuid, aem.elementIdentifierId, aem.axisRange);
                    if(glyph == null) return;

                    var cgly = GameObject.Instantiate(controllerGlyph, transform, false);
                    cgly.GetComponent<Image>().sprite = glyph;
                    break;
                default:
                    var keyCode = aem.keyboardKeyCode;
                    if (keyCode == KeyboardKeyCode.None)
                    {
                        return;
                    }
                    
                    var s = aem.keyboardKeyCode.ToString();
                    GameObject ob = GameObject.Instantiate(s.Length > 1 ? keyboardLongGlyph : keyboardGlyph, transform, false);
                    ob.GetComponentInChildren<TextMeshProUGUI>().text = s;
                    break;
            }
        }
    }
}