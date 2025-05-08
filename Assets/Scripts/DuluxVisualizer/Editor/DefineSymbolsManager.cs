using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace DuluxVisualizer.Editor
{
      /// <summary>
      /// Editor script to ensure our compatibility symbols are defined
      /// </summary>
      [InitializeOnLoad]
      public class DefineSymbolsManager
      {
            private static ListRequest packageListRequest;
            private static readonly string[] packageNames = new string[]
            {
                  "com.unity.sentis",
                  "com.unity.xr.arfoundation",
                  "com.unity.xr.arsubsystems",
                  "com.unity.xr.core-utils"
            };

            static DefineSymbolsManager()
            {
                  // Get a list of all packages
                  packageListRequest = Client.List();
                  EditorApplication.update += OnPackageListComplete;
            }

            private static void OnPackageListComplete()
            {
                  if (!packageListRequest.IsCompleted)
                        return;

                  if (packageListRequest.Status == StatusCode.Success)
                  {
                        Dictionary<string, bool> packageAvailability = new Dictionary<string, bool>();

                        // Initialize all packages as not available
                        foreach (string packageName in packageNames)
                        {
                              packageAvailability[packageName] = false;
                        }

                        // Set available packages
                        foreach (var package in packageListRequest.Result)
                        {
                              if (packageAvailability.ContainsKey(package.name))
                              {
                                    packageAvailability[package.name] = true;
                                    Debug.Log($"Found package: {package.name} Version: {package.version}");
                              }
                        }

                        // Get current defines
                        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup);

                        List<string> allDefines = currentDefines.Split(';')
                            .Where(d => !string.IsNullOrEmpty(d))
                            .ToList();

                        bool changed = false;

                        // Handle Sentis
                        if (packageAvailability["com.unity.sentis"])
                        {
                              if (!allDefines.Contains("UNITY_SENTIS"))
                              {
                                    allDefines.Add("UNITY_SENTIS");
                                    changed = true;
                              }
                        }

                        // Handle ARFoundation
                        if (packageAvailability["com.unity.xr.arfoundation"])
                        {
                              if (!allDefines.Contains("UNITY_AR_FOUNDATION_PRESENT"))
                              {
                                    allDefines.Add("UNITY_AR_FOUNDATION_PRESENT");
                                    changed = true;
                              }

                              if (!allDefines.Contains("AR_FOUNDATION_4_0_OR_NEWER"))
                              {
                                    allDefines.Add("AR_FOUNDATION_4_0_OR_NEWER");
                                    changed = true;
                              }
                        }

                        // Handle ARSubsystems
                        if (packageAvailability["com.unity.xr.arsubsystems"])
                        {
                              if (!allDefines.Contains("AR_SUBSYSTEMS_4_0_OR_NEWER"))
                              {
                                    allDefines.Add("AR_SUBSYSTEMS_4_0_OR_NEWER");
                                    changed = true;
                              }
                        }

                        // Handle XR CoreUtils
                        if (packageAvailability["com.unity.xr.core-utils"])
                        {
                              if (!allDefines.Contains("UNITY_XR_CORE_UTILS_PRESENT"))
                              {
                                    allDefines.Add("UNITY_XR_CORE_UTILS_PRESENT");
                                    changed = true;
                              }
                        }

                        // Apply changes if needed
                        if (changed)
                        {
                              string newDefines = string.Join(";", allDefines.ToArray());
                              PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);

                              Debug.Log("Updated scripting define symbols: " + newDefines);
                              AssetDatabase.Refresh();
                        }
                  }
                  else if (packageListRequest.Status >= StatusCode.Failure)
                  {
                        Debug.LogError("Package list request failed: " + packageListRequest.Error.message);
                  }

                  EditorApplication.update -= OnPackageListComplete;
            }

            [MenuItem("Tools/DuluxVisualizer/Force Define Symbols")]
            public static void ManuallySetSymbols()
            {
                  // Force enable all symbols
                  string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup);

                  List<string> allDefines = currentDefines.Split(';')
                      .Where(d => !string.IsNullOrEmpty(d))
                      .ToList();

                  string[] requiredSymbols = new string[] {
                        "UNITY_SENTIS",
                        "UNITY_AR_FOUNDATION_PRESENT",
                        "AR_FOUNDATION_4_0_OR_NEWER",
                        "AR_SUBSYSTEMS_4_0_OR_NEWER",
                        "UNITY_XR_CORE_UTILS_PRESENT"
                  };

                  bool changed = false;

                  foreach (string symbol in requiredSymbols)
                  {
                        if (!allDefines.Contains(symbol))
                        {
                              allDefines.Add(symbol);
                              changed = true;
                        }
                  }

                  if (changed)
                  {
                        string newDefines = string.Join(";", allDefines.ToArray());
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);

                        Debug.Log("Forced scripting define symbols: " + newDefines);
                        AssetDatabase.Refresh();
                  }
            }
      }
}