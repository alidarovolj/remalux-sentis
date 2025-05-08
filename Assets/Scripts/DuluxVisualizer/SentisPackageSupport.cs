using UnityEngine;
using System;

namespace DuluxVisualizer
{
      /// <summary>
      /// Класс для обеспечения совместимости с Unity Sentis
      /// Решает проблемы с отсутствующими типами в текущей версии Unity
      /// </summary>
      public static class SentisPackageSupport
      {
            /// <summary>
            /// Проверяет доступность Unity Sentis
            /// </summary>
            public static bool IsSentisAvailable()
            {
                  try
                  {
                        // Используем reflection для безопасной проверки наличия Sentis
                        Type sentisType = Type.GetType("Unity.Sentis.WorkerFactory, Unity.Sentis");
                        if (sentisType != null)
                        {
                              Debug.Log("Unity Sentis доступен в проекте");
                              return true;
                        }
                        Debug.LogWarning("Unity Sentis не найден в проекте");
                        return false;
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"Ошибка при проверке Unity Sentis: {ex.Message}");
                        return false;
                  }
            }

            /// <summary>
            /// Проверяет правильность ссылок на сборки Unity Sentis
            /// </summary>
            public static bool CheckSentisAssemblyReferences()
            {
                  try
                  {
                        // Проверяем основные типы Sentis через reflection
                        Type[] typesToCheck = new Type[]
                        {
                    Type.GetType("Unity.Sentis.ModelAsset, Unity.Sentis"),
                    Type.GetType("Unity.Sentis.Model, Unity.Sentis"),
                    Type.GetType("Unity.Sentis.IWorker, Unity.Sentis"),
                    Type.GetType("Unity.Sentis.BackendType, Unity.Sentis"),
                    Type.GetType("Unity.Sentis.TensorFloat, Unity.Sentis")
                        };

                        foreach (var type in typesToCheck)
                        {
                              if (type == null)
                              {
                                    Debug.LogError($"Не найден важный тип Unity Sentis");
                                    return false;
                              }
                        }

                        Debug.Log("Все необходимые типы Unity Sentis найдены");
                        return true;
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"Ошибка при проверке сборок Unity Sentis: {ex.Message}");
                        return false;
                  }
            }

            /// <summary>
            /// Выводит предложения по исправлению проблем с Sentis
            /// </summary>
            public static void PrintSentisFixSuggestions()
            {
                  Debug.Log("Рекомендации по исправлению проблем с Unity Sentis:");
                  Debug.Log("1. Проверьте наличие пакета Unity Sentis в Package Manager");
                  Debug.Log("2. Убедитесь, что версия Unity Sentis (2.1.2) совместима с текущей версией Unity");
                  Debug.Log("3. Добавьте ссылку на сборку Unity.Sentis в assembly definition file");
                  Debug.Log("4. Проверьте настройки Player в Project Settings и включите поддержку необходимых API");
                  Debug.Log("5. Попробуйте обновить пакет Unity Sentis до последней доступной версии");
                  Debug.Log("6. В крайнем случае, создайте новый проект Unity и перенесите в него код");
            }
      }
}