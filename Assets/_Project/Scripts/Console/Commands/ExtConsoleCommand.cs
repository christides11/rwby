using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}