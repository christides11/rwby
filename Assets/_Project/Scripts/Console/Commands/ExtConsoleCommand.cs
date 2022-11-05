using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using IngameDebugConsole;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rwby.Debugging
{
    public class ExtConsoleCommands
    {
        [ConsoleMethod( "quit", "Closes the application" )]
        public static void QuitApplication()
        {
            Application.Quit();
        }
        
        [ConsoleMethod( "unity-version", "Prints the unity version" )]
        public static void PrintUnityVersion()
        {
            Debug.Log(Application.unityVersion);
        }
        
        [ConsoleMethod( "version", "Prints the game version" )]
        public static void PrintGameVersion()
        {
            Debug.Log(Application.version);
        }
        
        [ConsoleMethod( "targetframerate", "Sets the targeted framerate" )]
        public static void SetTargetFramerate(int framerate)
        {
            Application.targetFrameRate = framerate;
            Debug.Log($"Set target framerate to {framerate}.");
        }
        
        [ConsoleMethod( "vsync", "Turn vsync on or off" )]
        public static void EnableVSync(int vSyncCount)
        {
            QualitySettings.vSyncCount = vSyncCount;
            Debug.Log($"VSync count set to {vSyncCount}.");
        }
        
        [ConsoleMethod( "say", "Say the text given" )]
        public static void Say(string msg)
        {
            ConsoleWindow.current.WriteLine(msg);
        }
        
        [ConsoleMethod( "host", "Host a lobby with given parameters" )]
        public static async void Host(string lobbyName, bool hostMode, int playerCount, int playersPerClient, string password,
            string gamemode, string gamemodeSettings)
        {
            Debug.Log("Trying to host lobby");
            //ConsoleWindow.current.WriteLine($"Starting hosting lobby");
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
                    
                    smc.SetMaxPlayersPerClient(playersPerClient);
                    await smc.CurrentGameMode.SetGamemodeSettings(gamemodeSettings);
                    break;
            }
        }

        [ConsoleMethod( "local", "Start a local game with given parameters" )]
        public static async void QuickTest(int localPlayers, string gamemode, string gamemodeSettings, string playerCharacter)
        {
            Debug.Log("Starting local game");
            
            string[] gamemodeRefStr = gamemode.Split(',');
            ModObjectSetContentReference gamemodeSetReference = new ModObjectSetContentReference(gamemodeRefStr[0], gamemodeRefStr[1]);

            ModContentGUIDReference gamemodeGUIDReference = new ModContentGUIDReference()
            {
                modGUID = gamemodeSetReference.modGUID,
                contentType = (int)ContentType.Gamemode,
                contentGUID = gamemodeSetReference.contentGUID
            };
            var gamemodeGUIDContentReference = ContentManager.singleton.ConvertModContentGUIDReference(gamemodeGUIDReference);
            var r = await ContentManager.singleton.LoadContentDefinition(gamemodeGUIDContentReference);

            if (!r)
            {
                Debug.Log($"Could not load gamemode {gamemodeRefStr}.");
                //ConsoleWindow.current.WriteLine($"Could not load gamemode {gamemodeRefStr}.", ConsoleMessageType.Error);
                return;
            }
            
            IGameModeDefinition gameModeDefinition = ContentManager.singleton
                .GetContentDefinition<IGameModeDefinition>(gamemodeGUIDContentReference);
                    
            if (gameModeDefinition == null)
            {
                Debug.Log("Invalid gamemode.");
                //ConsoleWindow.current.WriteLine("Invalid gamemode.", ConsoleMessageType.Error);
                return;
            }
                    
            bool loadResult = await gameModeDefinition.Load();
            if (!loadResult)
            {
                Debug.LogError("Error loading gamemode.");
                //ConsoleWindow.current.WriteLine("Error loading gamemode.", ConsoleMessageType.Error);
                return;
            }
            
            int sessionHandlerID = await GameManager.singleton.HostGamemodeSession("abc", 1, "", true, true);
            if (sessionHandlerID == -1)
            {
                Debug.LogError("Error starting session.");
                //ConsoleWindow.current.WriteLine("Error starting session.", ConsoleMessageType.Error);
                return;
            }
                    
            FusionLauncher fl = GameManager.singleton.networkManager.GetSessionHandler(sessionHandlerID);

            await UniTask.WaitUntil(() => fl.sessionManager != null);
            
            SessionManagerGamemode smc = (SessionManagerGamemode)fl.sessionManager;
            
            bool setGamemodeResult = await smc.TrySetGamemode(gamemodeGUIDContentReference);

            //smc.SetTeamCount(0);
            smc.SetMaxPlayersPerClient(localPlayers);
            await smc.CurrentGameMode.SetGamemodeSettings(gamemodeSettings);

            await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            await UniTask.WaitForEndOfFrame();
            
            var playerRef = fl._runner.LocalPlayer;
            ClientManager localClient = fl._runner.GetPlayerObject(playerRef).GetBehaviour<ClientManager>();
            localClient.CLIENT_SetPlayerCount((uint)localPlayers);
            localClient.profiles.Add(ProfilesManager.defaultProfileIdentifier);
            GameManager.singleton.localPlayerManager.AutoAssignControllers();
            //GameManager.singleton.localPlayerManager.systemPlayer.camera.enabled = false;
            

            string[] charaRefStr = playerCharacter.Split(',');
            ModObjectSetContentReference charSetRef = new ModObjectSetContentReference(charaRefStr[0], charaRefStr[1]);

            ModContentGUIDReference charaGUIDReference = new ModContentGUIDReference()
            {
                modGUID = charSetRef.modGUID,
                contentType = (int)ContentType.Fighter,
                contentGUID = charSetRef.contentGUID
            };
            var charaGUIDContentReference = ContentManager.singleton.ConvertModContentGUIDReference(charaGUIDReference);
            var charaLoadResult = await ContentManager.singleton.LoadContentDefinition(charaGUIDContentReference);
            if (!charaLoadResult)
            {
                Debug.LogError("Error loading character.");
                return;
            }
            
            smc.CLIENT_SetPlayerTeam(0, 1);
            smc.CLIENT_SetPlayerCharacterCount(0, 1);
            smc.CLIENT_SetPlayerCharacter(0, 0, charaGUIDContentReference);

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
            
            try
            {
                await UniTask.WaitUntil(() => smc.ClientDefinitions[0].players[0].characterReferences[0].IsValid(),
                    cancellationToken: cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.Log("Timeout waiting for valid character.");
                }
            }

            await UniTask.WaitForEndOfFrame();
            await UniTask.WaitForEndOfFrame();

            bool startMatchResult = await smc.TryStartMatch();
            if (!startMatchResult)
            {
                Debug.LogError("Error starting match.");
            }
        }
    }
}