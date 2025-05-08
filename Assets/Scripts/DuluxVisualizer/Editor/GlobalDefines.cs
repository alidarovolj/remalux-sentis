using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace DuluxVisualizer.Editor
{
      [InitializeOnLoad]
      public class GlobalDefines
      {
            // Список всех необходимых символов
            private static readonly string[] requiredSymbols = new string[]
            {
            "UNITY_SENTIS",
            "UNITY_AR_FOUNDATION_PRESENT",
            "AR_FOUNDATION_4_0_OR_NEWER",
            "AR_SUBSYSTEMS_4_0_OR_NEWER",
            "UNITY_XR_CORE_UTILS_PRESENT",
            "UNITY_BARRACUDA_PRESENT",
            "TEXTMESH_PRO_PRESENT"
            };

            static GlobalDefines()
            {
                  // Это будет вызвано, когда Unity перезагружает скрипты
                  AddRequiredSymbols();
            }

            [MenuItem("Tools/DuluxVisualizer/Add Required Define Symbols")]
            public static void AddRequiredSymbols()
            {
                  // Получаем текущие define symbols
                  string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                      EditorUserBuildSettings.selectedBuildTargetGroup);

                  // Конвертируем в список, игнорируя пустые
                  List<string> allDefines = definesString.Split(';')
                      .Where(d => !string.IsNullOrEmpty(d))
                      .ToList();

                  bool changed = false;

                  // Добавляем недостающие define symbols
                  foreach (string symbol in requiredSymbols)
                  {
                        if (!allDefines.Contains(symbol))
                        {
                              allDefines.Add(symbol);
                              changed = true;
                              Debug.Log($"Добавлен define symbol: {symbol}");
                        }
                  }

                  if (changed)
                  {
                        // Записываем обновленный список
                        string newDefines = string.Join(";", allDefines.ToArray());
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);

                        Debug.Log("Символы определения скриптов обновлены: " + newDefines);

                        // Принудительно обновляем AssetDatabase, чтобы Unity распознал изменения
                        AssetDatabase.Refresh();
                  }
                  else
                  {
                        Debug.Log("Все необходимые define symbols уже добавлены");
                  }
            }

            [MenuItem("Tools/DuluxVisualizer/Remove All Define Symbols")]
            public static void RemoveAllDefineSymbols()
            {
                  if (EditorUtility.DisplayDialog("Удаление символов",
                      "Это действие удалит все define symbols, добавленные для поддержки пакетов.\n\n" +
                      "Вы уверены?",
                      "Да, удалить", "Отмена"))
                  {
                        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup);

                        List<string> allDefines = definesString.Split(';')
                            .Where(d => !string.IsNullOrEmpty(d) && !requiredSymbols.Contains(d))
                            .ToList();

                        string newDefines = string.Join(";", allDefines.ToArray());
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(
                            EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);

                        Debug.Log("Define symbols удалены. Текущие символы: " + newDefines);
                        AssetDatabase.Refresh();
                  }
            }

            [MenuItem("Tools/DuluxVisualizer/Clean Library And Restart")]
            public static void CleanLibraryAndRestart()
            {
                  if (EditorUtility.DisplayDialog("Очистка проекта",
                      "Это действие удалит папку Library для полной перекомпиляции проекта.\n\n" +
                      "Unity закроется. Вам нужно будет открыть проект заново.\n\n" +
                      "Продолжить?",
                      "Да, очистить и перезапустить", "Отмена"))
                  {
                        string libraryPath = System.IO.Path.Combine(Application.dataPath, "..", "Library");
                        try
                        {
                              if (System.IO.Directory.Exists(libraryPath))
                              {
                                    System.IO.Directory.Delete(libraryPath, true);
                                    Debug.Log("Папка Library успешно удалена. Unity будет закрыт.");
                                    EditorApplication.ExecuteMenuItem("File/Exit");
                              }
                        }
                        catch (System.Exception e)
                        {
                              Debug.LogError($"Не удалось удалить папку Library: {e.Message}");
                        }
                  }
            }
      }
}