using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DuluxVisualizer
{
      /// <summary>
      /// Компонент для проверки интеграции Unity Sentis
      /// Помогает диагностировать проблемы с пакетом Sentis
      /// </summary>
      public class SentisIntegrationChecker : MonoBehaviour
      {
            [Header("Настройки проверки")]
            [SerializeField] private bool checkOnStart = true;
            [SerializeField] private bool logDetailedInfo = true;

            [Header("UI для отображения статуса")]
            [SerializeField] private UnityEngine.UI.Text statusText;

            private void Start()
            {
                  if (checkOnStart)
                  {
                        CheckSentisIntegration();
                  }
            }

            /// <summary>
            /// Проверяет интеграцию Unity Sentis и выводит результаты
            /// </summary>
            public void CheckSentisIntegration()
            {
                  Debug.Log("=== Проверка интеграции Unity Sentis ===");

                  // Проверяем наличие сборки Sentis
                  bool sentisAvailable = SentisPackageSupport.IsSentisAvailable();

                  if (sentisAvailable)
                  {
                        Debug.Log("✓ Unity Sentis сборка найдена");

                        // Проверяем наличие необходимых типов
                        bool typesAvailable = SentisPackageSupport.CheckSentisAssemblyReferences();

                        if (typesAvailable)
                        {
                              Debug.Log("✓ Основные типы Unity Sentis доступны");

                              // Проверяем наличие модели в ресурсах
                              CheckForModels();

                              // Выводим детальную информацию
                              if (logDetailedInfo)
                              {
                                    LogSentisPackageInfo();
                              }

                              if (statusText != null)
                              {
                                    statusText.text = "Unity Sentis: OK";
                                    statusText.color = Color.green;
                              }
                        }
                        else
                        {
                              Debug.LogError("✗ Некоторые необходимые типы Unity Sentis недоступны");
                              SentisPackageSupport.PrintSentisFixSuggestions();

                              if (statusText != null)
                              {
                                    statusText.text = "Unity Sentis: Ошибка типов";
                                    statusText.color = Color.red;
                              }
                        }
                  }
                  else
                  {
                        Debug.LogError("✗ Unity Sentis сборка не найдена");
                        SentisPackageSupport.PrintSentisFixSuggestions();

                        if (statusText != null)
                        {
                              statusText.text = "Unity Sentis: Не найден";
                              statusText.color = Color.red;
                        }
                  }
            }

            private void CheckForModels()
            {
                  // Проверяем наличие ONNX моделей в ресурсах
                  var modelsInResources = Resources.LoadAll<UnityEngine.Object>("Models");
                  if (modelsInResources != null && modelsInResources.Length > 0)
                  {
                        Debug.Log($"✓ Найдено {modelsInResources.Length} моделей в Resources/Models");
                        foreach (var model in modelsInResources)
                        {
                              Debug.Log($"  - {model.name} ({model.GetType().Name})");
                        }
                  }
                  else
                  {
                        Debug.LogWarning("⚠ Модели в Resources/Models не найдены");
                  }

                  // Проверяем наличие ONNX моделей в StreamingAssets
                  string streamingAssetsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Models");
                  if (System.IO.Directory.Exists(streamingAssetsPath))
                  {
                        string[] files = System.IO.Directory.GetFiles(streamingAssetsPath, "*.onnx", System.IO.SearchOption.AllDirectories);
                        Debug.Log($"✓ Найдено {files.Length} ONNX моделей в StreamingAssets/Models");
                        foreach (var file in files)
                        {
                              Debug.Log($"  - {System.IO.Path.GetFileName(file)}");
                        }
                  }
                  else
                  {
                        Debug.LogWarning("⚠ Путь StreamingAssets/Models не существует");
                  }
            }

            private void LogSentisPackageInfo()
            {
                  try
                  {
                        Debug.Log("=== Информация о пакете Unity Sentis ===");

                        // Получаем информацию о сборке через reflection
                        Assembly sentisAssembly = Assembly.Load("Unity.Sentis");
                        if (sentisAssembly != null)
                        {
                              Debug.Log($"Версия сборки: {sentisAssembly.GetName().Version}");
                              Debug.Log($"Место сборки: {sentisAssembly.Location}");

                              // Пробуем получить версию продукта
                              object[] attributes = sentisAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                              if (attributes.Length > 0)
                              {
                                    AssemblyProductAttribute productAttr = (AssemblyProductAttribute)attributes[0];
                                    Debug.Log($"Продукт: {productAttr.Product}");
                              }
                        }
                        else
                        {
                              Debug.LogWarning("Не удалось загрузить сборку Unity.Sentis для получения информации");
                        }
                  }
                  catch (Exception ex)
                  {
                        Debug.LogError($"Ошибка при получении информации о пакете: {ex.Message}");
                  }
            }
      }
}