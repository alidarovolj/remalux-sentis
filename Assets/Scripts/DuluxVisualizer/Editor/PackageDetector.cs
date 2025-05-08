using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace DuluxVisualizer.Editor
{
      [InitializeOnLoad]
      public class PackageDetector
      {
            private static ListRequest listRequest;
            private static Dictionary<string, bool> detectedPackages = new Dictionary<string, bool>();

            private static readonly Dictionary<string, string> packageToDefine = new Dictionary<string, string>
        {
            { "com.unity.sentis", "UNITY_SENTIS" },
            { "com.unity.xr.arfoundation", "UNITY_AR_FOUNDATION_PRESENT" },
            { "com.unity.xr.core-utils", "UNITY_XR_CORE_UTILS_PRESENT" },
            { "com.unity.barracuda", "UNITY_BARRACUDA_PRESENT" },
            { "com.unity.textmeshpro", "TEXTMESH_PRO_PRESENT" }
        };

            static PackageDetector()
            {
                  EditorApplication.update += Update;
                  CheckPackages();
            }

            [MenuItem("Tools/DuluxVisualizer/Detect Installed Packages")]
            public static void CheckPackages()
            {
                  listRequest = Client.List();
            }

            private static void Update()
            {
                  if (listRequest != null && listRequest.IsCompleted)
                  {
                        if (listRequest.Status == StatusCode.Success)
                        {
                              detectedPackages.Clear();

                              foreach (var package in listRequest.Result)
                              {
                                    if (packageToDefine.ContainsKey(package.name))
                                    {
                                          detectedPackages[package.name] = true;
                                          Debug.Log($"Detected package: {package.name} (version {package.version})");
                                    }
                              }

                              UpdateDefines();
                        }
                        else if (listRequest.Status >= StatusCode.Failure)
                        {
                              Debug.LogError($"Package detection failed: {listRequest.Error.message}");
                        }

                        listRequest = null;
                        EditorApplication.update -= Update;
                  }
            }

            private static void UpdateDefines()
            {
                  // Get current defines
                  string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup);

                  List<string> allDefines = definesString.Split(';')
                      .Where(d => !string.IsNullOrEmpty(d))
                      .ToList();

                  bool changed = false;

                  // Add defines for detected packages
                  foreach (var package in detectedPackages)
                  {
                        if (package.Value && packageToDefine.TryGetValue(package.Key, out string defineSymbol))
                        {
                              if (!allDefines.Contains(defineSymbol))
                              {
                                    allDefines.Add(defineSymbol);
                                    changed = true;
                                    Debug.Log($"Added define for detected package {package.Key}: {defineSymbol}");
                              }
                        }
                  }

                  // Remove defines for packages that aren't installed
                  foreach (var entry in packageToDefine)
                  {
                        string packageName = entry.Key;
                        string defineSymbol = entry.Value;

                        if (!detectedPackages.ContainsKey(packageName) || !detectedPackages[packageName])
                        {
                              if (allDefines.Contains(defineSymbol))
                              {
                                    allDefines.Remove(defineSymbol);
                                    changed = true;
                                    Debug.Log($"Removed define for missing package {packageName}: {defineSymbol}");
                              }
                        }
                  }

                  if (changed)
                  {
                        string newDefines = string.Join(";", allDefines.ToArray());
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);

                        Debug.Log("Updated defines based on installed packages: " + newDefines);
                        AssetDatabase.Refresh();
                  }
            }
      }
}