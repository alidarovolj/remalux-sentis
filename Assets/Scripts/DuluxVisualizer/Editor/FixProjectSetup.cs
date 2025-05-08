using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuluxVisualizer.Editor
{
      public class FixProjectSetup
      {
            [MenuItem("Tools/DuluxVisualizer/Fix Project Setup")]
            public static void FixProject()
            {
                  Debug.Log("=== Starting project setup fix ===");

                  // 1. Make sure all required define symbols are set
                  AddRequiredSymbols();

                  // 2. Ensure Unity package dummy implementations are available
                  EnsureDummyImplementationsExist();

                  // 3. Force a clean and recompile
                  ForceCleanAndRecompile();

                  Debug.Log("=== Project setup fixed ===");
                  Debug.Log("You may need to restart Unity for all changes to take effect.");
            }

            private static void AddRequiredSymbols()
            {
                  string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup);

                  List<string> allDefines = definesString.Split(';')
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
                              Debug.Log($"Adding required define symbol: {symbol}");
                        }
                  }

                  if (changed)
                  {
                        string newDefines = string.Join(";", allDefines.ToArray());
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);

                        Debug.Log("Updated scripting define symbols: " + newDefines);
                  }
            }

            private static void EnsureDummyImplementationsExist()
            {
                  // Check for our UnityPackageDummies.cs file
                  string dummiesPath = Path.Combine("Assets", "Scripts", "DuluxVisualizer", "UnityPackageDummies.cs");

                  if (!File.Exists(dummiesPath))
                  {
                        Debug.LogWarning("UnityPackageDummies.cs not found. Create it manually if needed.");
                  }
                  else
                  {
                        Debug.Log("UnityPackageDummies.cs file found.");
                  }
            }

            private static void ForceCleanAndRecompile()
            {
                  // Force Unity to clear script cache and rebuild
                  string tempFile = Path.Combine("Assets", "Scripts", "DuluxVisualizer", "TempForceCompile.cs");
                  string content = "// Temporary file to force recompile\n" +
                                  "// Generated on: " + System.DateTime.Now.ToString() + "\n" +
                                  "namespace DuluxVisualizer {\n" +
                                  "    public class TempForceCompile {\n" +
                                  "        // This class will be deleted\n" +
                                  "    }\n" +
                                  "}";

                  // Create the temporary file
                  File.WriteAllText(tempFile, content);
                  AssetDatabase.Refresh();

                  // Delete the temporary file to force another compile
                  if (File.Exists(tempFile))
                  {
                        File.Delete(tempFile);
                        AssetDatabase.Refresh();
                  }

                  // Suggest deleting the Library folder
                  if (EditorUtility.DisplayDialog("Complete Setup",
                      "Would you like to delete the Library folder to force a complete project rebuild?\n\n" +
                      "This will close Unity. You'll need to reopen the project manually.",
                      "Yes, delete and close", "No, just continue"))
                  {
                        string libraryPath = Path.Combine(Application.dataPath, "..", "Library");
                        try
                        {
                              if (Directory.Exists(libraryPath))
                              {
                                    Directory.Delete(libraryPath, true);
                                    Debug.Log("Library folder deleted successfully. Unity will now close.");
                                    EditorApplication.ExecuteMenuItem("File/Exit");
                              }
                        }
                        catch (System.Exception e)
                        {
                              Debug.LogError($"Failed to delete Library folder: {e.Message}");
                        }
                  }
            }
      }
}