using System;
using System.Collections.Generic;

namespace rwby
{
    public static class RwbyDependencies
    {
        public static readonly Tuple<string, string>[] packages =
        {
            new Tuple<string, string>("com.unity.cinemachine@2.9.1", "com.unity.cinemachine@2.9.1"),
            new Tuple<string, string>("com.unity.addressables@1.20.5", "com.unity.addressables@1.20.5"),
            new Tuple<string, string>("com.github.siccity.xnode@1.8.0", "https://github.com/siccity/xNode.git#1.8.0"),
            new Tuple<string, string>("com.christides.hack-and-slash-framework@39.3.0", "https://github.com/christides11/hack-and-slash-framework.git#upm/v39.3.0"),
            new Tuple<string, string>("com.cysharp.unitask@2.3.1", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.3.1"),
            new Tuple<string, string>("com.dbrizov.naughtyattributes@2.1.4", "https://github.com/dbrizov/NaughtyAttributes.git#upm/v2.1.4"),
            new Tuple<string, string>("com.medvedya.select_implementation_property_drawer@1.1.2", "https://github.com/christides11/Select-implementation-property-drawer.git#1.1.2"),
        };
        public static readonly List<string> dlls = new List<string>()
        {
            "Animancer",
            "DOTween",
            "kcc.core",
            "Mahou.Helpers",
            "Rewired_Core",
            "Rewired_CSharp",
            "Rewired_Windows",
            "Rewired_Windows_Functions",
            "Rewired_Linux",
            "Rewired_Linux_Functions",
            "Rewired_OSX",
            "Rewired_OSX_Functions",
            "rwby.csharp",
            "UMod",
            "UnityUIExtensions",
        };
    }
}