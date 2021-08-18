using System;
using System.Collections.Generic;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
namespace SuperUnityBuild.BuildActions
{
    public class PackageManagement : BuildAction, IPreBuildAction, IPreBuildPerPlatformAction, IPostBuildAction, IPostBuildPerPlatformAction
    {
        [Header("Package Settings")]
        [Tooltip("List of package IDs to add")] public List<string> PackagesToAdd = new List<string>();
        [Tooltip("List of package IDs to remove")] public List<string> PackagesToRemove = new List<string>();

        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildDistribution distribution, DateTime buildTime, ref BuildOptions options, string configKey, string buildPath)
        {

            PackagesToRemove.ForEach(id => HandleRequest(Client.Remove(id), id));
            PackagesToAdd.ForEach(id => HandleRequest(Client.Add(id), id));
        }

        private void HandleRequest<T>(T request, string id) where T : Request
        {
            // Block until complete
            while (!request.IsCompleted) { }

            switch (request.Status)
            {
                case StatusCode.Success:
                    Debug.Log($"Package request succeeded for {id}");
                    break;

                case StatusCode.Failure:
                    Debug.LogError($"Package request failed for {id}: {request.Error}");
                    break;

                default:
                    break;
            }
        }
    }
}
