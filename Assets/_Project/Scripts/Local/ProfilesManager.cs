using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rewired;
using UnityEngine;

namespace rwby
{
    public class ProfilesManager : MonoBehaviour
    {
        public static string defaultProfileIdentifier = "Default";
        public delegate void ProfileAction(ProfilesManager profilesManager);
        public delegate void ProfileChangeAction(ProfilesManager profilesManager, int index);
        public event ProfileAction onProfileAdded;
        public event ProfileAction onProfileRemoved;
        public event ProfileChangeAction onProfileUpdated;
        
        private readonly byte version = 0;
        [SerializeField] protected List<ProfileDefinition> profiles = new List<ProfileDefinition>();
        public ReadOnlyCollection<ProfileDefinition> Profiles => profiles.AsReadOnly();
        
        
        public void Initialize()
        {
            if (!LoadProfiles())
            {
                ProfileDefinition temp = new ProfileDefinition
                {
                    undeletable = true,
                    profileName = defaultProfileIdentifier,
                    controllerCam = new ProfileDefinition.CameraVariables()
                    {
                        deadzoneHoz = 0.1f,
                        deadzoneVert = 0.1f,
                        speedHoz = 1.0f,
                        speedVert = 1.0f,
                        speedLockOnHoz = 1.0f,
                        speedLockOnVert = 1.0f
                    },
                    keyboardCam = new ProfileDefinition.CameraVariables()
                    {
                        speedHoz = 0.3f,
                        speedVert = 0.3f,
                        speedLockOnHoz = 0.3f,
                        speedLockOnVert = 0.3f
                    }
                };
                profiles.Add(temp);
                ApplyControlsToProfile(-1, 0);
                SaveProfiles();
                LoadProfiles();
            }
            SaveProfiles();
        }

        public void SaveProfiles()
        {
            SaveLoadJsonService.Save("profiles.json", profiles, true);
        }

        public bool LoadProfiles()
        {
            if (!SaveLoadJsonService.TryLoad("profiles.json", out List<ProfileDefinition> loadedProfiles)) return false;
            profiles = loadedProfiles;
            return true;
        }

        public void AddProfile(string profileName)
        {
            ProfileDefinition temp = profiles[0];
            temp.profileName = profileName;
            profiles.Add(temp);
            SaveProfiles();
        }

        public void RemoveProfile(int index)
        {
            profiles.RemoveAt(index);
            SaveProfiles();
        }

        public ProfileDefinition GetProfile(string name)
        {
            foreach(var p in profiles)
            {
                if (p.profileName.ToLower() == name.ToLower()) return p;
            }
            return new ProfileDefinition();
        }

        public void ApplyProfile(ProfileDefinition profileDefinition, int index)
        {
            profiles[index] = profileDefinition;
            onProfileUpdated?.Invoke(this, index);
        }

        public void RestoreDefaultControls(int playerID)
        {
            //var player = playerID == -1 ? ReInput.players.SystemPlayer : ReInput.players.GetPlayer(playerID);
            var player = playerID == -1 ? ReInput.players.GetPlayer(0) : ReInput.players.GetPlayer(playerID);
            player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
            player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
            player.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
            player.controllers.maps.LoadDefaultMaps(ControllerType.Custom);
        }
        
        public void ApplyControlsToProfile(int player, int profileIndex)
        {
            //Rewired.PlayerSaveData playerData = (player == -1 ? ReInput.players.GetSystemPlayer() : ReInput.players.GetPlayer(player)).GetSaveData(true);
            Rewired.PlayerSaveData playerData = (player == -1 ? ReInput.players.GetPlayer(0) : ReInput.players.GetPlayer(player)).GetSaveData(true);

            List<string> tempInputBehaviours = new List<string>();
            foreach(InputBehavior behavior in playerData.inputBehaviors) {
                tempInputBehaviours.Add(behavior.ToJsonString());
            }

            // CONTROLLERS
            List<string> tempControllerMaps = new List<string>();
            foreach(var joystickMapSaveData in playerData.joystickMapSaveData)
            {
                tempControllerMaps.Add(joystickMapSaveData.map.ToJsonString());
            }
            
            // KEYBOARD
            var tempKeyboardMaps = new List<string>();
            foreach (var keyboardMapSaveData in playerData.keyboardMapSaveData)
            {
                tempKeyboardMaps.Add(keyboardMapSaveData.map.ToJsonString());
            }
            
            // Mouse
            var tempMouseMaps = new List<string>();
            foreach (var mouseMapSaveData in playerData.mouseMapSaveData)
            {
                tempMouseMaps.Add(mouseMapSaveData.map.ToJsonString());
            }

            ProfileDefinition temp = profiles[profileIndex];
            temp.behaviours = tempInputBehaviours;
            temp.joystickMaps = tempControllerMaps;
            temp.keyboardMaps = tempKeyboardMaps;
            temp.mouseMaps = tempMouseMaps;
            temp.version = version;
            profiles[profileIndex] = temp;
        }

        public void ApplyProfileToPlayer(int playerID, string profileName)
        {
            if (String.IsNullOrEmpty(profileName)) profileName = defaultProfileIdentifier;

            var p = profiles.FirstOrDefault(x => x.profileName.ToLower() == profileName.ToLower());
            if (p.Equals(default(ProfileDefinition)))
            {
                p = profiles[0];
            }
            
            ApplyProfileToPlayer(playerID, profiles.IndexOf(p));
        }
        
        public void ApplyProfileToPlayer(int playerID, int profileIndex)
        {
            ProfileDefinition profile = profiles[profileIndex];
            //var player = playerID == -1 ? ReInput.players.SystemPlayer : ReInput.players.GetPlayer(playerID);
            var player = playerID == -1 ? ReInput.players.GetPlayer(0) : ReInput.players.GetPlayer(playerID);
            RestoreDefaultControls(playerID);

            IList<InputBehavior> behaviors = ReInput.mapping.GetInputBehaviors(player.id);
            for(int j = 0; j < behaviors.Count; j++)
            {
                string json = profile.GetInputBehaviour(behaviors[j].id);
                if(json == string.Empty) continue;
                behaviors[j].ImportJsonString(json);
            }
            
            // Load Joystick Maps
            bool foundJoystickMaps = false;
            List<List<string>> joystickMaps = new List<List<string>>();
            foreach(Joystick joystick in player.controllers.Joysticks)
            {
                List<string> maps = profile.joystickMaps;
                joystickMaps.Add(maps);
                if(maps.Count > 0) foundJoystickMaps = true;
            }

            // Joystick maps
            if (foundJoystickMaps)
            {
                int count = 0;
                player.controllers.maps.ClearMaps(ControllerType.Joystick, true);
                foreach (Joystick joystick in player.controllers.Joysticks)
                {
                    player.controllers.maps.AddMapsFromJson(ControllerType.Joystick, joystick.id,
                        joystickMaps[count]); // add joystick controller maps to player
                    count++;
                }
            }

            // LOAD KEYBOARD MAPS
            var keyboardMaps = profile.keyboardMaps;
            bool keyboardMapsValid = keyboardMaps != null && keyboardMaps.Count > 0;

            // KEYBOARD
            if (keyboardMapsValid)
            {
                player.controllers.maps.ClearMaps(ControllerType.Keyboard, true);
                player.controllers.maps.AddMapsFromJson(ControllerType.Keyboard, 0, keyboardMaps);
            }
            
            // MOUSE
            var mouseMaps = profile.mouseMaps;
            bool mouseMapsValid = mouseMaps != null && mouseMaps.Count > 0;

            if (mouseMapsValid)
            {
                player.controllers.maps.ClearMaps(ControllerType.Mouse, true);
                player.controllers.maps.AddMapsFromJson(ControllerType.Mouse, 0, mouseMaps);
            }
        }
    }
}