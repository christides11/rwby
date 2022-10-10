using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rwby.Debugging
{
    public class ExtConsoleCommands
    {
        [Command("close", "Closes the application")]
        public static void QuitApplication()
        {
            Application.Quit();
        }

        [Command("unity-version", "Prints the current unity version.")]
        public static void PrintUnityVersion()
        {
            ConsoleWindow.current.WriteLine(Application.unityVersion);
        }

        [Command("version", "Prints the current game version.")]
        public static void PrintGameVersion()
        {
            ConsoleWindow.current.WriteLine(Application.version);
        }

        [Command("targetframerate", "Sets the targeted framerate.")]
        public static void SetTargetFramerate(int framerate)
        {
            Application.targetFrameRate = framerate;
            ConsoleWindow.current.WriteLine($"Set target framerate to {framerate}.");
        }

        [Command("vsync", "Turn vsync on or off.")]
        public static void EnableVSync(int vSyncCount)
        {
            QualitySettings.vSyncCount = vSyncCount;
            ConsoleWindow.current.WriteLine($"VSync Count set to {vSyncCount}.");
        }

        [Command("say", "Say the text given.")]
        public static void Say(string msg)
        {
            ConsoleWindow.current.WriteLine(msg);
        }

        [Command("host", "Host a lobby with given parameters.")]
        public static async void Host(string lobbyName, bool hostMode, int playerCount, int playersPerClient, string password,
            string gamemode, string gamemodeSettings)
        {
            ConsoleWindow.current.WriteLine($"Starting hosting lobby");
            switch (hostMode)
            {
                case true:
                    break;
                case false:
                    string[] gamemodeRefStr = gamemode.Split(',');
                    ModObjectSetContentReference gamemodeSetReference = new ModObjectSetContentReference(gamemodeRefStr[0], gamemodeRefStr[1]);

                    ModContentGUIDReference gamemodeGUIDReference = new ModContentGUIDReference()
                    {
                        modGUID = gamemodeSetReference.modGUID,
                        contentType = (int)ContentType.Gamemode,
                        contentGUID = gamemodeSetReference.contentGUID
                    };
                    var gamemodeGUIDContentReference =
                        ContentManager.singleton.ConvertModContentGUIDReference(gamemodeGUIDReference);

                    var r = await ContentManager.singleton.LoadContentDefinition(gamemodeGUIDContentReference);

                    if (!r)
                    {
                        ConsoleWindow.current.WriteLine($"Could not load gamemode {gamemodeRefStr}.", ConsoleMessageType.Error);
                        return;
                    }
                    
                    IGameModeDefinition gameModeDefinition = ContentManager.singleton
                        .GetContentDefinition<IGameModeDefinition>(gamemodeGUIDContentReference);
                    
                    if (gameModeDefinition == null)
                    {
                        ConsoleWindow.current.WriteLine("Invalid gamemode.", ConsoleMessageType.Error);
                        return;
                    }
                    
                    bool loadResult = await gameModeDefinition.Load();
                    if (!loadResult)
                    {
                        ConsoleWindow.current.WriteLine("Error loading gamemode.", ConsoleMessageType.Error);
                        return;
                    }
                    
                    //var selectedGamemode = GameObject.Instantiate(gameModeDefinition.GetGamemode(), Vector3.zero, Quaternion.identity).GetComponent<GameModeBase>();
                    
                    int sessionHandlerID = await GameManager.singleton.HostGamemodeSession(lobbyName, playerCount, password, false);
                    if (sessionHandlerID == -1)
                    {
                        ConsoleWindow.current.WriteLine("Error starting session.", ConsoleMessageType.Error);
                        return;
                    }
                    
                    FusionLauncher fl = GameManager.singleton.networkManager.GetSessionHandler(sessionHandlerID);

                    await UniTask.WaitUntil(() => fl.sessionManager != null);
            
                    SessionManagerGamemode smc = (SessionManagerGamemode)fl.sessionManager;
            
                    bool setGamemodeResult = await smc.TrySetGamemode(gamemodeGUIDContentReference);

                    smc.SetTeamCount(0);
                    smc.SetMaxPlayersPerClient(playersPerClient);
                    await smc.CurrentGameMode.SetGamemodeSettings(gamemodeSettings);
                    break;
            }
        }
    }
}