using UnityEngine;
using UnityEditor;
using System.IO;

namespace DuluxVisualizer.Editor
{
      [InitializeOnLoad]
      public class ForceCompile
      {
            static ForceCompile()
            {
                  // Delay to ensure our symbols are processed first
                  EditorApplication.delayCall += () =>
                  {
                        // Force a recompile by modifying a dummy file
                        string dummyPath = Path.Combine("Assets", "Scripts", "DuluxVisualizer", "ForceCompileHelper.cs");

                        // Ensure directory exists
                        string directory = Path.GetDirectoryName(dummyPath);
                        if (!Directory.Exists(directory))
                        {
                              Directory.CreateDirectory(directory);
                        }

                        // Create or update file with timestamp to force recompile
                        string content = "// Auto-generated file to force Unity to recompile\n" +
                                  "// Last updated: " + System.DateTime.Now.ToString() + "\n" +
                                  "namespace DuluxVisualizer { public static class ForceCompileHelper { } }";

                        File.WriteAllText(dummyPath, content);

                        // Refresh asset database to detect the change
                        AssetDatabase.Refresh();
                  };
            }

            [MenuItem("Tools/DuluxVisualizer/Force Full Recompile")]
            public static void ForceFullRecompile()
            {
                  // Force Unity to do a complete recompile
                  string[] defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');

                  // Add a temporary symbol
                  var symbolList = new System.Collections.Generic.List<string>(defineSymbols);
                  string tempSymbol = "TEMP_RECOMPILE_" + System.DateTime.Now.Ticks.ToString();
                  symbolList.Add(tempSymbol);

                  // Set the new symbols with temporary symbol
                  PlayerSettings.SetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup,
                      string.Join(";", symbolList));

                  // Refresh to apply these changes
                  AssetDatabase.Refresh();

                  // Schedule removal of temp symbol after a delay
                  EditorApplication.delayCall += () =>
                  {
                        // Remove the temporary symbol
                        symbolList.Remove(tempSymbol);
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup,
                      string.Join(";", symbolList));

                        // Force another refresh
                        AssetDatabase.Refresh();

                        Debug.Log("Force recompile complete. The project should now rebuild with all defined symbols.");
                  };
            }
      }
}