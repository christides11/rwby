using SuperUnityBuild.BuildTool;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SuperUnityBuild.BuildActions
{
    using System.Collections.Generic;

    public class ExportPackage : BuildAction, IPreBuildAction
    {
        public string packageName;
        public string folderPath;
        public ExportPackageOptions exportOptions;

        public override void Execute()
        {
            base.Execute();

            var exportedPackageAssetList = new List<string>();
            //Find all shaders that have "Surface" in their names and add them to the list
            foreach (var guid in AssetDatabase.FindAssets("", new []{folderPath}))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                exportedPackageAssetList.Add(path);
            }

            AssetDatabase.ExportPackage(
                exportedPackageAssetList.ToArray(),
                $"{packageName}.unitypackage",
                exportOptions);
        }

        protected override void DrawProperties(SerializedObject obj)
        {
            if (GUILayout.Button("Run Now", GUILayout.ExpandWidth(true)))
            {
                Execute();
            }
            base.DrawProperties(obj);
        }
    }
}
