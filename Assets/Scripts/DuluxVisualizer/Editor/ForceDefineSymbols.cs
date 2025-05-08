using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace DuluxVisualizer.Editor
{
      [InitializeOnLoad]
      public class ForceDefineSymbols
      {
            // List of symbols we want to ensure are defined
            private static readonly string[] requiredSymbols = new string[]
            {
            "UNITY_SENTIS",
            "UNITY_AR_FOUNDATION_PRESENT",
            "AR_FOUNDATION_4_0_OR_NEWER",
            "AR_SUBSYSTEMS_4_0_OR_NEWER",
            "UNITY_XR_CORE_UTILS_PRESENT"
            };

            static ForceDefineSymbols()
            {
                  // This will run whenever scripts are recompiled
                  AddDefineSymbols();
            }

            [MenuItem("Tools/DuluxVisualizer/Force Define Symbols")]
            public static void AddDefineSymbols()
            {
                  string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup);

                  List<string> allDefines = definesString.Split(';').ToList();

                  bool changed = false;

                  // Add any missing required symbols
                  foreach (string symbol in requiredSymbols)
                  {
                        if (!allDefines.Contains(symbol))
                        {
                              allDefines.Add(symbol);
                              changed = true;
                              Debug.Log($"Adding required define symbol: {symbol}");
                        }
                  }

                  // Only update if we made changes
                  if (changed)
                  {
                        string newDefines = string.Join(";", allDefines.ToArray());
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);

                        Debug.Log("Updated scripting define symbols: " + newDefines);
                        AssetDatabase.Refresh();
                  }
            }
      }
}