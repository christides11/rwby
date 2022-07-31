using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rewired;
using UnityEngine;

namespace rwby
{
    public class ProfilesManager : MonoBehaviour
    {
        public static string defaultProfileIdentifier = "Default";
        public delegate void ProfileAction(ProfilesManager profilesManager);
        public event ProfileAction onProfileAdded;
        public event ProfileAction onProfileRemoved;
        
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
                    profileName = defaultProfileIdentifier
                };
                profiles.Add(temp);
                ApplyControlsToProfile(0, 0, true);
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
        
        public void ApplyControlsToProfile(int player, int profileIndex, bool systemPlayer = false)
        {
            Rewired.PlayerSaveData playerData = (systemPlayer ? ReInput.players.GetSystemPlayer() : ReInput.players.GetPlayer(player)).GetSaveData(true);

            List<string> tempInputBehaviours = new List<string>();
            foreach(InputBehavior behavior in playerData.inputBehaviors) {
                tempInputBehaviours.Add(behavior.ToJsonString());
            }

            List<string> tempControllerMaps = new List<string>();
            foreach(var joystickMapSaveData in playerData.joystickMapSaveData)
            {
                tempControllerMaps.Add(joystickMapSaveData.map.ToJsonString());
            }
            Debug.Log($"{playerData.joystickMapSaveData.Length} , {playerData.joystickMapCount}");
            Debug.Log($"{ReInput.players.GetPlayer(0).GetSaveData(true).joystickMapSaveData.Length} , " +
                      $"{ReInput.players.GetPlayer(0).GetSaveData(true).joystickMapCount}");
            ProfileDefinition temp = profiles[profileIndex];
            temp.behaviours = tempInputBehaviours;
            temp.joystickMaps = tempControllerMaps;
            temp.version = version;
            profiles[profileIndex] = temp;
        }

        public void ApplyProfileToPlayer(int playerID, int profileIndex, bool systemPlayer = false)
        {
            ProfileDefinition profile = profiles[profileIndex];
            var player = systemPlayer ? ReInput.players.SystemPlayer : ReInput.players.GetPlayer(playerID);
            player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
            
            
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
            if (foundJoystickMaps == false) return;
            int count = 0;
            foreach(Joystick joystick in player.controllers.Joysticks) {
                player.controllers.maps.AddMapsFromJson(ControllerType.Joystick, joystick.id, joystickMaps[count]); // add joystick controller maps to player
                count++;
            }
        }
    }
}