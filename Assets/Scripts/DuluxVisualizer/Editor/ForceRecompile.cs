using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace DuluxVisualizer.Editor
{
      [InitializeOnLoad]
      public class ForceRecompile
      {
            [MenuItem("Tools/DuluxVisualizer/Force Recompile")]
            public static void TriggerRecompile()
            {
                  // Method 1: Change the define symbols, which forces a recompile
                  string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup);

                  // Add a temporary define symbol
                  string tempSymbol = "FORCE_RECOMPILE_" + System.DateTime.Now.Ticks;
                  PlayerSettings.SetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup,
                      definesString + ";" + tempSymbol);

                  // Let Unity process changes
                  EditorApplication.delayCall += () =>
                  {
                        // Remove the temporary symbol
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup,
                      definesString);

                        Debug.Log("Recompilation triggered successfully.");

                        // Refresh AssetDatabase to pick up new changes
                        AssetDatabase.Refresh();
                  };
            }

            // Alternative method using a dummy file
            public static void TriggerRecompileWithDummyFile()
            {
                  string dummyScriptPath = Path.Combine(Application.dataPath, "Editor", "DummyScript.cs");
                  string dummyContent =
                      "// This is a temporary file to force Unity to recompile\n" +
                      "// It will be automatically deleted\n" +
                      "using UnityEngine;\n" +
                      "\n" +
                      "namespace DuluxVisualizer.Editor\n" +
                      "{\n" +
                      "    public class DummyScript\n" +
                      "    {\n" +
                      "        private static int DummyVariable = " + System.DateTime.Now.Ticks + ";\n" +
                      "    }\n" +
                      "}";

                  try
                  {
                        // Create directory if it doesn't exist
                        Directory.CreateDirectory(Path.GetDirectoryName(dummyScriptPath));

                        // Write the dummy file
                        File.WriteAllText(dummyScriptPath, dummyContent);

                        // Refresh the AssetDatabase to detect the new file
                        AssetDatabase.Refresh();

                        // Schedule deletion of the dummy file after compilation
                        EditorApplication.delayCall += () =>
                        {
                              try
                              {
                                    if (File.Exists(dummyScriptPath))
                                    {
                                          File.Delete(dummyScriptPath);
                                          AssetDatabase.Refresh();
                                    }
                              }
                              catch (System.Exception e)
                              {
                                    Debug.LogError("Failed to delete dummy script: " + e.Message);
                              }
                        };

                        Debug.Log("Recompilation triggered with dummy file.");
                  }
                  catch (System.Exception e)
                  {
                        Debug.LogError("Failed to create dummy script: " + e.Message);
                  }
            }

            [MenuItem("Tools/DuluxVisualizer/Complete Rebuild")]
            public static void CompleteRebuild()
            {
                  // Сначала добавляем define symbols
                  GlobalDefines.AddRequiredSymbols();

                  // Затем запускаем перекомпиляцию
                  TriggerRecompile();

                  // Показываем сообщение
                  EditorUtility.DisplayDialog("Полное перестроение",
                      "Скрипт-символы добавлены и запущена перекомпиляция.\n\n" +
                      "Если проблемы сохраняются, попробуйте использовать:\n" +
                      "Tools > DuluxVisualizer > Clean Library And Restart\n\n" +
                      "Это полностью очистит кэш проекта.",
                      "OK");
            }
      }
}